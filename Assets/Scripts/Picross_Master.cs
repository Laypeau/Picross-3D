﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Picross_Master : MonoBehaviour
{
	#region 
	public Vector3Int gameGenSize = Vector3Int.one; // Should be used only for GridGen()
	public GameObject blockPrefab; //Set in editor
	public Transform cameraFocus; //Set in editor

	private Transform gameCamera;
	private List<GameObject> blocks = new List<GameObject>();
	private int minX, maxX, minY, maxY, minZ, maxZ;
	private LayerMask maskBlocks;
	private bool mouseOverBlock;

	[Header("Input")]
	public Mode inputMode = Mode.Edit; //turn to property
	public enum Mode
	{
		Camera = 0,
		Edit = 1,
		Colour = 2,
		Play = 3
	}
	[SerializeField] private Color colourLMB = Color.green;
	[SerializeField] private Color colourRMB = Color.blue;

	[Header("Play mode")]
	public int mistakesAllowed = 3; //[HideInInspector] 
	public int mistakesMade = 0; //make a getter/setter
	public int totalBlocksSolution = 0; //protected set , make a btter name for it. it tracke all blocks that need to be dstroyed
	public int totalBlocksDestroyed = 0; //protected set
	//time limit

	[Header("File Save/Load")]
	public string fileLocation = @"C:\Users\realw\Desktop\";
	public string fileSaveName = "Save.p3d";
	public string puzzleSaveTitle = "";

	[Header("Camera")]
	[SerializeField] private AnimationCurve posLerpCurve = AnimationCurve.EaseInOut(0,0,1,1);
	[SerializeField] private float mouseSensitivityX = 0.2f;
	[SerializeField] private float mouseSensitivityY = 0.2f;
	[SerializeField] private float mouseSlideMultiplier = 7f;
	[SerializeField] private float zoomSensitivity = 0.5f;
	[SerializeField] private float rotateX = 15f;
	[SerializeField] private float rotateY = 15f;
	[SerializeField] private AnimationCurve keyRotateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	private Vector2 cameraVelocity;
	private float minCamDistance;
	private float maxCamDistance;
	private bool keyRotating = false;
	private int mouseButton = -1;
	private float mousePrevX = 0f;
	private float mousePrevY = 0f;

	private bool mouseLocked = false; // add a getter/setter
	#endregion

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(50, 50, 250, 250));
			if (GUILayout.Button("GridClear()")) GridClear();
			if (GUILayout.Button("GridGen()")) GridGen();
			if (GUILayout.Button("Save()")) GridSave();
		GUILayout.EndArea();
	}

	void Awake()
	{
		if (blockPrefab == null)
			throw new UnityException("Block prefab not set");

		if (cameraFocus == null)
			throw new UnityException("Camera focus not set");

		gameCamera = cameraFocus.transform.GetChild(0);
		if (gameCamera == null)
			throw new UnityException("Camera is not child of pivot point");

		maskBlocks = 8;
	}

	void Update()
	{
		#region [debug]
		if (Input.GetKeyDown(KeyCode.Alpha1))
			inputMode = Mode.Edit;
		if (Input.GetKeyDown(KeyCode.Alpha2))
			inputMode = Mode.Colour;
		if (Input.GetKeyDown(KeyCode.Alpha3))
			inputMode = Mode.Play;
		if (Input.GetKeyDown(KeyCode.Alpha4))
			inputMode = Mode.Camera;
		#endregion

		Ray blockRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(blockRay.origin, blockRay.direction * 50f);
		if (Physics.Raycast(blockRay, out RaycastHit rayHit, 50f)) // try using a layermask
			mouseOverBlock = true;
		else
			mouseOverBlock = false;

		if (Input.GetMouseButtonDown(0))
		{
			if (mouseOverBlock && !mouseLocked)
			{
				switch (inputMode)
				{
					case Mode.Edit:
						rayHit.transform.gameObject.GetComponent<Picross_Block>().Deactivate();
						StartCoroutine(ReconfigureCamera(0.1f));
						break;
					
					case Mode.Colour:
						rayHit.transform.gameObject.GetComponent<Picross_Block>().SetColour(colourLMB);
						break;

					case Mode.Camera:
						mousePrevX = Input.mousePosition.y;
						mousePrevY = Input.mousePosition.x;
						mouseButton = 0;
						break;
					
					case Mode.Play:
						rayHit.transform.gameObject.GetComponent<Picross_Block>().Destroy();
						break;

					default:
						mousePrevX = Input.mousePosition.y;
						mousePrevY = Input.mousePosition.x;
						mouseButton = 0;
						break;
				}
			}
			else if (!keyRotating && mouseButton == -1)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseButton = 0;
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			if (mouseOverBlock && !mouseLocked)
			{
				switch (inputMode)
				{
					case Mode.Edit:
						try
						{
							transform.Find((rayHit.transform.position + rayHit.normal).ToString()).gameObject.GetComponent<Picross_Block>().Activate();
							StartCoroutine(ReconfigureCamera(0.1f));
						}
						catch
						{
							GameObject currentBlock = Instantiate(blockPrefab);
							currentBlock.transform.parent = transform;
							currentBlock.transform.position = rayHit.transform.position + rayHit.normal;
							currentBlock.name = (rayHit.transform.position + rayHit.normal).ToString();
							blocks.Add(currentBlock);
							StartCoroutine(ReconfigureCamera(0.1f));
						}
						break;

					case Mode.Colour:
						rayHit.transform.gameObject.GetComponent<Picross_Block>().SetColour(colourRMB);
						break;

					case Mode.Camera:
						mousePrevX = Input.mousePosition.y;
						mousePrevY = Input.mousePosition.x;
						mouseButton = 1;
						break;

					case Mode.Play:
						rayHit.transform.gameObject.GetComponent<Picross_Block>().Mark();
						break;

					default:
						mousePrevX = Input.mousePosition.y;
						mousePrevY = Input.mousePosition.x;
						mouseButton = 1;
						break;
				}
			}
			else if (!keyRotating && mouseButton == -1)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseButton = 1;
			}
		}

		if (Input.GetMouseButtonDown(2))
		{
			if (!keyRotating && mouseButton == -1)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseButton = 2;
			}
		}

		// Move camera + Camera slide
		if (mouseButton != -1)
		{
			if (Input.GetMouseButtonUp(mouseButton))
			{
				mouseButton = -1;
				cameraVelocity = new Vector2(mousePrevX - Input.mousePosition.y, Input.mousePosition.x - mousePrevY);
			}
			else
			{
				cameraFocus.rotation = Quaternion.Euler(0f, (Input.mousePosition.x - mousePrevY) * mouseSensitivityY, 0f) * cameraFocus.rotation * Quaternion.Euler((mousePrevX - Input.mousePosition.y) * mouseSensitivityX, 0f, 0f);
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
			}
		}
		else
		{
			cameraFocus.rotation = Quaternion.Euler(0f, cameraVelocity.y * mouseSensitivityY * mouseSlideMultiplier * Time.deltaTime, 0f) * cameraFocus.rotation * Quaternion.Euler(cameraVelocity.x * mouseSensitivityX * mouseSlideMultiplier * Time.deltaTime, 0f, 0f);
		}

		// Keyboard camera controls
		if (!keyRotating && mouseButton == -1)
		{
			mousePrevX = 0f;
			mousePrevY = 0f;

			int hInput = 0;
			int vInput = 0;

			if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
				hInput -= 1;
			if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
				hInput += 1;

			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.A))
				vInput += 1;
			if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.A))
				vInput -= 1;

			if (hInput != 0 || vInput != 0)
				StartCoroutine(RotateCameraFocus(vInput * rotateY, hInput * rotateX, 0.25f));
		}

		// Zoom Camera
		if (Input.mouseScrollDelta.y != 0f)
		{
			if (Input.mouseScrollDelta.y > 0f)
			{
				gameCamera.localPosition = new Vector3(0f, 0f, Mathf.Clamp(gameCamera.localPosition.z + zoomSensitivity, maxCamDistance, minCamDistance));
			}
			else if (Input.mouseScrollDelta.y < 0f)
			{
				gameCamera.localPosition = new Vector3(0f, 0f, Mathf.Clamp(gameCamera.localPosition.z - zoomSensitivity, maxCamDistance, minCamDistance));
			}
		}
	}

	void FixedUpdate()
	{
		cameraVelocity = cameraVelocity * 0.95f; //maybe expose this field
	}

	public IEnumerator LockActions(float duration)
	{
		mouseLocked = true;
		yield return new WaitForSecondsRealtime(duration);
		mouseLocked = false;
	}

	private IEnumerator RotateCameraFocus(float xAmount, float yAmount, float duration)
	{
		keyRotating = true;

		float startTime = Time.time;
		Quaternion initialRot = cameraFocus.rotation;

		while (startTime + duration >= Time.time)
		{
			float percentage = (Time.time - startTime) / duration;
			cameraFocus.rotation = Quaternion.Euler(0f, yAmount * keyRotateCurve.Evaluate(percentage), 0f) * initialRot * Quaternion.Euler(xAmount * keyRotateCurve.Evaluate(percentage), 0f, 0f);
			yield return null;
		}

		cameraFocus.rotation = Quaternion.Euler(0f, yAmount, 0f) * initialRot * Quaternion.Euler(xAmount, 0f, 0f);
		keyRotating = false;
	}

	private IEnumerator ReconfigureCamera(float duration)
	{
		StartCoroutine(LockActions(duration));
		GridCalculateSize();
		Vector3 newPos = new Vector3(minX - 0.5f + (maxX-minX)/2f, minY - 0.5f + (maxY-minY)/2f, minZ - 0.5f + (maxZ-minZ)/2f);

		float gameSize = Vector3.Magnitude(new Vector3((maxX - minX)/2, (maxY - minY)/2, (maxZ - minZ)/2));
		float initialMinCamDistance = minCamDistance;
		float initialMaxCamDistance = maxCamDistance;

		float initialTime = Time.time;
		Vector3 initialPos = cameraFocus.position;
		while(Time.time < initialTime + duration)
		{
			float percentage = (Time.time - initialTime)/duration;

			cameraFocus.position = Vector3.Lerp(initialPos, newPos, percentage);

			minCamDistance = Mathf.Lerp(initialMinCamDistance, -1 * (gameSize + 1f), posLerpCurve.Evaluate(percentage));
			maxCamDistance = Mathf.Lerp(initialMaxCamDistance, -3 * (gameSize + 1f), posLerpCurve.Evaluate(percentage));

			if (gameCamera.localPosition.z > minCamDistance)
				gameCamera.localPosition = new Vector3(0f, 0f, minCamDistance);

			if (gameCamera.localPosition.z < maxCamDistance)
				gameCamera.localPosition = new Vector3(0f, 0f, maxCamDistance);

			yield return null;
		}

		cameraFocus.position = new Vector3(minX + ((maxX - minX)/2f - 0.5f), minY + ((maxY - minY)/2f - 0.5f), minZ + ((maxZ - minZ)/2f - 0.5f));

		minCamDistance = -1 * (gameSize + 1f);
		maxCamDistance = -3 * (gameSize + 1f);
		
		if (gameCamera.localPosition.z > minCamDistance)
			gameCamera.localPosition = new Vector3(0f, 0f, minCamDistance);

		if (gameCamera.localPosition.z < maxCamDistance)
			gameCamera.localPosition = new Vector3(0f, 0f, maxCamDistance);
	}

	private Vector3Int GridCalculateSize()
	{
		foreach (GameObject obj in blocks) //Make something check if there's less than 1 block
		{
			if (obj.GetComponent<Picross_Block>().isActive)
			{
				minX = (int)obj.transform.position.x;
				maxX = (int)obj.transform.position.x;
				minY = (int)obj.transform.position.y;
				maxY = (int)obj.transform.position.y;
				minZ = (int)obj.transform.position.z;
				maxZ = (int)obj.transform.position.z;

				break;
			}
		}

		foreach (GameObject obj in blocks)
		{
			if (obj.GetComponent<Picross_Block>().isActive)
			{
				int objX = (int)obj.transform.position.x;
				int objY = (int)obj.transform.position.y;
				int objZ = (int)obj.transform.position.z;

				if (objX < minX)
					minX = objX;
				else if (objX + 1 > maxX)
					maxX = objX + 1;

				if (objY < minY)
					minY = objY;
				else if (objY + 1 > maxY)
					maxY = objY + 1;

				if (objZ < minZ)
					minZ = objZ;
				else if (objZ + 1 > maxZ)
					maxZ = objZ + 1;
			}
		}
		return Vector3Int.CeilToInt(new Vector3(maxX-minX, maxY-minY, maxZ-minZ));
	}

	///<summary> Destroys all blocks </summary>
	public void GridClear()
	{
		if (blocks != null)
		{
			foreach (GameObject block in blocks)
			{
				Destroy(block);
			}
		}
		blocks.Clear();
	}

	///<summary> Generates cube of blocks </summary>
	public void GridGen()
	{
		GridClear();

		int i = 0;
		for (int x = 0; x < gameGenSize.x; x++)
		{
			for (int y = 0; y < gameGenSize.y; y++)
			{
				for (int z = 0; z < gameGenSize.z; z++)
				{
					//Vector3 currentPos = new Vector3(x - (gameSize.x / 2f) + 0.5f, y - (gameSize.y / 2f) + 0.5f, z - (gameSize.z / 2f) + 0.5f);
					Vector3 currentPos = new Vector3(x, y, z);
					GameObject currentBlock = Instantiate(blockPrefab);
					Picross_Block currentScript = currentBlock.GetComponent<Picross_Block>();

					currentBlock.name = currentPos.ToString();
					currentBlock.layer = 1<<8;
					currentBlock.transform.parent = transform;
					currentBlock.transform.position = currentPos;

					currentScript.Activate();

					blocks.Add(currentBlock);
					i++;
				}
			}
		}

		StartCoroutine(ReconfigureCamera(0f));
		cameraFocus.rotation = Quaternion.Euler(35f, 135f, 0f);
		gameCamera.localPosition = new Vector3(0f, 0f, 2f * minCamDistance);
	}

	///<summary> Only generates only the structure </summary>
	public void GridLoadEdit()	
	{
		GridClear();

		mistakesMade = 0;
		totalBlocksDestroyed = 0;
		totalBlocksSolution = 0;

		string[] dataStrings = System.IO.File.ReadAllLines(gameObject.GetComponent<Menu_Main>().location + fileSaveName);
		Vector3Int gridSize = new Vector3Int(int.Parse(dataStrings[3].Split(' ')[0]), int.Parse(dataStrings[3].Split(' ')[1]), int.Parse(dataStrings[3].Split(' ')[2]));

		string[] activeBlocks = dataStrings[4].Split(' ');
		string[] blockColours = dataStrings[5].Split(' ');
		try
		{
			int i = 0;
			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					for (int z = 0; z < gridSize.z; z++)
					{
						GameObject currentBlock = Instantiate(blockPrefab);
						Picross_Block currentScript = currentBlock.GetComponent<Picross_Block>();

						Vector3 currentPos = new Vector3(x, y, z);
						string[] RGB = blockColours[i].Split(',');
						Color currentColour = new Color(float.Parse(RGB[0]), float.Parse(RGB[1]), float.Parse(RGB[2]));

						currentBlock.name = currentPos.ToString();
						currentBlock.layer = 1<<8;
						currentBlock.transform.parent = transform;
						currentBlock.transform.position = currentPos;
						currentScript.SetColour(currentColour);

						if (activeBlocks[i] == "0")
							currentScript.Deactivate();
						else
							currentScript.Activate();

						blocks.Add(currentBlock);
						i++;
					}
				}
			}

			StartCoroutine(ReconfigureCamera(0f));
			cameraFocus.rotation = Quaternion.Euler(35f, 135f, 0f);
			gameCamera.localPosition = new Vector3(0f, 0f, 2f * minCamDistance);

			inputMode = Mode.Edit;
		}	
		catch (System.Exception e)
		{
			Debug.Log($"Error while loading level: {e}");
			foreach (GameObject child in GetComponentsInChildren<GameObject>(true)) Destroy(child);
		}
	}

	///<summary> Generates structure with false blocks </summary>
	public void GridLoadPlay()
	{
		GridClear();

		mistakesMade = 0;
		totalBlocksDestroyed = 0;
		totalBlocksSolution = 0;

		string[] dataStrings = System.IO.File.ReadAllLines(gameObject.GetComponent<Menu_Main>().location + fileSaveName);
		Vector3Int gridSize = new Vector3Int(int.Parse(dataStrings[3].Split(' ')[0]), int.Parse(dataStrings[3].Split(' ')[1]), int.Parse(dataStrings[3].Split(' ')[2]));

		string[] activeBlocks = dataStrings[4].Split(' ');
		string[] blockColours = dataStrings[5].Split(' ');
		try
		{
			int i = 0;
			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					for (int z = 0; z < gridSize.z; z++)
					{
						GameObject currentBlock = Instantiate(blockPrefab);
						Picross_Block currentScript = currentBlock.GetComponent<Picross_Block>();

						Vector3 currentPos = new Vector3(x, y, z);
						string[] RGB = blockColours[i].Split(',');
						Color currentColour = new Color(float.Parse(RGB[0]), float.Parse(RGB[1]), float.Parse(RGB[2]));

						currentBlock.name = currentPos.ToString();
						currentBlock.layer = 1<<8;
						currentBlock.transform.parent = transform;
						currentBlock.transform.position = currentPos;
						currentScript.SetColour(currentColour);

						if (activeBlocks[i] == "0")
						{
							totalBlocksSolution += 1;
							currentScript.isSolution = false;
						}
						else
						{
							currentScript.isSolution = true;
						}

						blocks.Add(currentBlock);
						i++;
					}
				}
			}

			StartCoroutine(ReconfigureCamera(0f));
			cameraFocus.rotation = Quaternion.Euler(35f, 135f, 0f);
			gameCamera.localPosition = new Vector3(0f, 0f, 2f * minCamDistance);

			inputMode = Mode.Play;
		}
		catch (System.Exception e)
		{
			Debug.Log($"Error while loading level: {e}");
			foreach (GameObject child in GetComponentsInChildren<GameObject>(true)) Destroy(child);
		}
	}

	///<summary>  </summary>
	public void GridSave()
	{
		string[] dataStrings = new string[6];
		blocks.Sort(new BlockComparer());

		if (puzzleSaveTitle == "" || puzzleSaveTitle == null) dataStrings[0] = "[MISSING_NAME]";
		else dataStrings[0] = puzzleSaveTitle;
		dataStrings[1] = "[AUTHOR_NAME]";
		dataStrings[2] = "[SOLVE_TIME]";

		Vector3 size = GridCalculateSize();
		dataStrings[3] = size.x.ToString() + " " + size.y.ToString() + " " + size.z.ToString();

		string[] activeBlocks = new string[(maxX - minX) * (maxY - minY) * (maxZ - minZ)];
		int i = 0;
		for (int x = minX; x < maxX; x++)
		{
			for (int y = minY; y < maxY; y++)
			{
				for (int z = minZ; z < maxZ; z++)
				{
					string name = new Vector3(x,y,z).ToString();
					try
					{
						if (transform.Find(name).GetComponent<Picross_Block>().isActive)
							activeBlocks[i] = "1";
						else
							activeBlocks[i] = "0";
					}
					catch
					{
						activeBlocks[i] = "0";
					}
					i++;
				}
			}
		}
		dataStrings[4] = string.Join(" ", activeBlocks);

		string[] blockColours = new string[(maxX - minX) * (maxY - minY) * (maxZ - minZ)];
		i = 0;
		for (int x = minX; x < maxX; x++)
		{
			for (int y = minY; y < maxY; y++)
			{
				for (int z = minZ; z < maxZ; z++)
				{
					string name = new Vector3(x, y, z).ToString();
					try
					{
						if (activeBlocks[i] == "1")
						{
							Color colour = transform.Find(name).GetComponent<Picross_Block>().vertexColours;
							blockColours[i] = $"{colour.r},{colour.g},{colour.b}";
						}
						else
						{
							blockColours[i] = "1,1,1";
						}
					}
					catch
					{
						blockColours[i] = "1,1,1";
					}
					i++;
				}
			}
		}
		dataStrings[5] = string.Join(" ", blockColours);


		System.IO.File.WriteAllLines(gameObject.GetComponent<Menu_Main>().location + fileSaveName, dataStrings);
		Debug.Log("Saved to file");
	}

	public void MistakesIncrement()
	{
		mistakesMade += 1;
		if (mistakesMade == mistakesAllowed)
		{
			Debug.Log("Game over");
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(cameraFocus.position, gameGenSize);
	}
}

/// <summary> Sorts blocks based on world co-ordinates, in the order of X,Y,Z from lowest true highest </summary>
public class BlockComparer : Comparer<GameObject>
{
	public override int Compare(GameObject x, GameObject y)
	{
		if (x.transform.position.x < y.transform.position.x) return -1;
		else if (x.transform.position.x > y.transform.position.x) return 1;
		else if (x.transform.position.y < y.transform.position.y) return -1;
		else if (x.transform.position.y > y.transform.position.y) return 1;
		else if (x.transform.position.z < y.transform.position.z) return -1;
		else if(x.transform.position.z > y.transform.position.z)return 1;
		else return 0;
	}
}
