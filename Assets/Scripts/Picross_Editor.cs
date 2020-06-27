using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Picross_Editor : MonoBehaviour
{
	private List<GameObject> blocks = new List<GameObject>();
	private int minX, maxX, minY, maxY, minZ, maxZ;
	private LayerMask maskBlocks;

	[Header("File Save/Load")]
	public string fileLocation = @"C:\Users\realw\Desktop\";
	public string fileSaveName = "Save.txt";
	public string fileLoadName = "Save.txt";
	public string puzzleSaveTitle = "";

	[Header("General")]
	public Vector3Int gameGenSize = Vector3Int.one; // Should be used only for GridGen()
	public GameObject blockPrefab;
	public Transform cameraFocus;
	private Transform gameCamera;
	private bool mouseOverBlock;

	[Header("Camera")]
	public float minCamDistance;
	private float maxCamDistance;
	public float zoomSensitivity = 1f;
	private bool keyRotating = false;
	private bool mouseRotating = false;
	private int mouseButton = -1;
	public float mouseSensitivityX = 0.3f;
	public float mouseSensitivityY = 0.3f;
	private float mousePrevX = 0f;
	private float mousePrevY = 0f;
	public float rotateX = 15f;
	public float rotateY = 15f;
	private bool blockCooldown = false;

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
		Ray blockRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(blockRay.origin, blockRay.direction * 50f);
		if (Physics.Raycast(blockRay, out RaycastHit rayHit, 50f)) // try using maskBlocks as a layermask
			mouseOverBlock = true;
		else
			mouseOverBlock = false;

		// Deactivate block
		if (Input.GetMouseButtonDown(0))
		{
			if (mouseOverBlock && !blockCooldown)
			{
				rayHit.transform.gameObject.GetComponent<Picross_Block>().Deactivate();
				StartCoroutine(ReconfigureCamera(0.2f));
			}
			else if (!keyRotating && !mouseRotating)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseRotating = true;
				mouseButton = 0;
			}
		}

		//Add block
		if (Input.GetMouseButtonDown(1))
		{
			if (mouseOverBlock && !blockCooldown)
			{
				try
				{
					transform.Find((rayHit.transform.position + rayHit.normal).ToString()).gameObject.GetComponent<Picross_Block>().Activate();
					StartCoroutine(ReconfigureCamera(0.2f));
				}
				catch
				{
					GameObject currentBlock = Instantiate(blockPrefab);
					currentBlock.transform.parent = transform;
					currentBlock.transform.position = rayHit.transform.position + rayHit.normal;
					currentBlock.name = (rayHit.transform.position + rayHit.normal).ToString();
					blocks.Add(currentBlock);
					StartCoroutine(ReconfigureCamera(0.2f));
				}
			}
			else if (!keyRotating && !mouseRotating)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseRotating = true;
				mouseButton = 1;
			}
		}

		if (Input.GetMouseButtonDown(2))
		{
			if (!keyRotating && !mouseRotating)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseRotating = true;
				mouseButton = 2;
			}
		}

		if (mouseRotating && Input.GetMouseButtonUp(mouseButton))
		{
			mouseRotating = false;
			mouseButton = -1;
		}

		if (mouseRotating)
		{
			cameraFocus.rotation = Quaternion.Euler(0f, (Input.mousePosition.x - mousePrevY) * mouseSensitivityY, 0f) * cameraFocus.rotation * Quaternion.Euler((mousePrevX - Input.mousePosition.y) * mouseSensitivityX, 0f, 0f);
			mousePrevX = Input.mousePosition.y;
			mousePrevY = Input.mousePosition.x;
		}

		if (!keyRotating && !mouseRotating)
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

	IEnumerator RotateCameraFocus(float xAmount, float yAmount, float duration)
	{
		keyRotating = true;

		float startTime = Time.time;
		Quaternion initialRot = cameraFocus.rotation;

		while (startTime + duration >= Time.time) //make it evaluate an animation curve
		{
			float percentage = (Time.time - startTime) / duration;
			cameraFocus.rotation = Quaternion.Euler(0f, yAmount * percentage, 0f) * initialRot * Quaternion.Euler(xAmount * percentage, 0f, 0f);
			yield return null;
		}

		cameraFocus.rotation = Quaternion.Euler(0f, yAmount, 0f) * initialRot * Quaternion.Euler(xAmount, 0f, 0f);
		keyRotating = false;
	}

	private IEnumerator ReconfigureCamera(float duration)
	{
		blockCooldown = true;

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

			minCamDistance = Mathf.Lerp(initialMinCamDistance, -1 * (gameSize + 1f), percentage);
			maxCamDistance = Mathf.Lerp(initialMaxCamDistance, -3 * (gameSize + 1f), percentage);

			if (gameCamera.localPosition.z > minCamDistance + 1)
				gameCamera.localPosition = new Vector3(0f, 0f, minCamDistance);

			if (gameCamera.localPosition.z < maxCamDistance)
				gameCamera.localPosition = new Vector3(0f, 0f, maxCamDistance);

			yield return null;
		}

		cameraFocus.position = new Vector3(minX + ((maxX - minX)/2f - 0.5f), minY + ((maxY - minY)/2f - 0.5f), minZ + ((maxZ - minZ)/2f - 0.5f));

		minCamDistance = -1 * (gameSize + 1f);
		maxCamDistance = -3 * (gameSize + 1f);
		
		if (gameCamera.localPosition.z > minCamDistance + 1)
			gameCamera.localPosition = new Vector3(0f, 0f, minCamDistance);

		if (gameCamera.localPosition.z < maxCamDistance)
			gameCamera.localPosition = new Vector3(0f, 0f, maxCamDistance);

		blockCooldown = false;
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
					currentBlock.layer = 8;
					currentBlock.transform.parent = transform;
					currentBlock.transform.position = currentPos;

					currentScript.gridPos = new Vector3(x, y, z);
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

	public void GridLoad()
	{
		GridClear();
		
		string[] dataStrings = System.IO.File.ReadAllLines(fileLocation + fileSaveName);
		Vector3Int gridSize = new Vector3Int(int.Parse(dataStrings[1].Split(' ')[0]), int.Parse(dataStrings[1].Split(' ')[1]), int.Parse(dataStrings[1].Split(' ')[2]));

		string[] activeBlocks = dataStrings[4].Split(' ');
		try
		{
			int i = 0;
			for (int x = 0; x < gridSize.x; x++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					for (int z = 0; z < gridSize.z; z++)
					{
						Vector3 currentPos = new Vector3(x, y, z);
						GameObject currentBlock = Instantiate(blockPrefab);
						Picross_Block currentScript = currentBlock.GetComponent<Picross_Block>();

						currentBlock.name = currentPos.ToString();
						currentBlock.layer = 8;
						currentBlock.transform.parent = transform;
						currentBlock.transform.position = currentPos;

						currentScript.gridPos = new Vector3(x, y, z);

						if (activeBlocks[i] == "0")
							currentScript.Deactivate();
						else
							currentScript.Activate();

						blocks.Add(currentBlock);
						i++;
					}
				}
			}
		}	
		catch
		{
			Debug.Log("Invalid level data");
		}

		StartCoroutine(ReconfigureCamera(0f));
		cameraFocus.rotation = Quaternion.Euler(35f, 135f, 0f);
		gameCamera.localPosition = new Vector3(0f, 0f, 2f * minCamDistance);
	}

	public void GridSave()
	{
		string[] dataStrings = new string[5];
		blocks.Sort(new BlockComparer());

		if (puzzleSaveTitle == "" || puzzleSaveTitle == null)
			dataStrings[0] = "[MISSING_NAME]";
		else
			dataStrings[0] = puzzleSaveTitle;

		Vector3 size = GridCalculateSize();
		dataStrings[1] = size.x.ToString() + " " + size.y.ToString() + " " + size.z.ToString();

		dataStrings[2] = "nothing here";
		dataStrings[3] = "nothing here";

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

		//Vertex colours

		System.IO.File.WriteAllLines(fileLocation + fileSaveName, dataStrings);
		Debug.Log("Saved to file");
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
		if (x.transform.position.x < y.transform.position.x)
		{
			return -1;
		}
		else if (x.transform.position.x > y.transform.position.x)
		{
			return 1;
		}
		else if (x.transform.position.y < y.transform.position.y)
		{
			return -1;
		}
		else if (x.transform.position.y > y.transform.position.y)
		{
			return 1;
		}
		else if (x.transform.position.z < y.transform.position.z)
		{
			return -1;
		}
		else if(x.transform.position.z > y.transform.position.z)
		{
			return 1;
		}
		else return 0;
	}
}
