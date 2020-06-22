using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Master : MonoBehaviour
{
	private GameObject[] blocks;
	private LayerMask maskBlocks;

	[Header("File Save/Load")]
	public string fileLocation = @"C:\Users\realw\Desktop\";
	public string fileSaveName = "Save.txt";
	public string fileLoadName = "Save.txt";
	public string puzzleName = "";

	[Header("General")]
	public Vector3Int gameSize = Vector3Int.one;
	public GameObject blockPrefab;
	public Transform cameraFocus;
	private Transform gameCamera;
	private bool mouseOverBlock;

	[Header("Camera")]
	public float minCamDistance; //remember that the game is moved by half the size
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

	// make file saving chop off empty faces on the ends

	// make camera focus to object center instead of master transform
	// make camera lerp to position when setup
	// make camera store a percentage as it's local zdistance

	// Make mouse loop across screen
	// make keyboard camera controls look up an animation curve


	void Update()
	{
		Ray blockRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(blockRay.origin, blockRay.direction * 50f);
		if (Physics.Raycast(blockRay, out RaycastHit rayHit, 50f)) // try using maskBlocks as a layermask
			mouseOverBlock = true;
		else
			mouseOverBlock = false;

		Debug.Log(mouseOverBlock);

		if (Input.GetMouseButtonDown(0))
		{
			if (mouseOverBlock)
			{
				rayHit.transform.gameObject.GetComponent<Picross_Block>().Deactivate();
			}
			else if (!keyRotating && !mouseRotating)
			{
				mousePrevX = Input.mousePosition.y;
				mousePrevY = Input.mousePosition.x;
				mouseRotating = true;
				mouseButton = 0;
			}
		}
		
		if (Input.GetMouseButtonDown(1))
		{
			if (mouseOverBlock)
			{
				try
				{
					transform.Find((rayHit.transform.position + rayHit.normal).ToString()).gameObject.GetComponent<Picross_Block>().Reactivate();
				}
				catch
				{
					Debug.Log("Adding new block");
					gameSize = gameSize + Vector3Int.RoundToInt(rayHit.normal);
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

		while(startTime + duration >= Time.time) //make it evaluate an animation curve
		{
			float percentage = (Time.time - startTime) / duration;
			cameraFocus.rotation = Quaternion.Euler(0f, yAmount * percentage, 0f) * initialRot * Quaternion.Euler(xAmount * percentage, 0f, 0f);
			yield return null;
		}
		
		cameraFocus.rotation = Quaternion.Euler(0f, yAmount, 0f) * initialRot * Quaternion.Euler(xAmount, 0f, 0f);
		keyRotating = false;
		yield return null;
	}

	private void ConfigureCamera()
	{
		cameraFocus.position = transform.position;
		cameraFocus.rotation = Quaternion.Euler(0f, 0f, 0f);

		minCamDistance = -1 * ((gameSize.magnitude / 2f) + 1f);
		maxCamDistance = -1 * ((gameSize.magnitude * 1.5f) + 1f);

		gameCamera.position = new Vector3(0f, 0f, 2 * minCamDistance);
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

	}

	public void GridGen()
	{
		GridClear();
		blocks = new GameObject[gameSize.x * gameSize.y * gameSize.z];
		ConfigureCamera();

		int i = 0;
		for (int x = 0; x < gameSize.x; x++)
		{
			for (int y = 0; y < gameSize.y; y++)
			{
				for (int z = 0; z < gameSize.z; z++)
				{
					Vector3 currentPos = new Vector3(x - (gameSize.x/2f) + 0.5f, y - (gameSize.y/2f) + 0.5f, z - (gameSize.z/2f) + 0.5f);
					GameObject currentBlock = Instantiate(blockPrefab);
					Picross_Block currentScript = currentBlock.GetComponent<Picross_Block>();

					currentBlock.name = currentPos.ToString();
					currentBlock.layer = 8;
					currentBlock.transform.parent = transform;
					currentBlock.transform.position = currentPos;

					currentScript.gridPos = new Vector3(x,y,z);
					currentScript.Reactivate();

					blocks[i] = currentBlock;
					i++;
				}
			}
		}
	}

	public void GridLoad()
	{
		GridClear();
		string[] dataStrings = System.IO.File.ReadAllLines(fileLocation + fileSaveName);
		gameSize = new Vector3Int(int.Parse(dataStrings[1].Split(' ')[0]), int.Parse(dataStrings[1].Split(' ')[1]), int.Parse(dataStrings[1].Split(' ')[2]));
		ConfigureCamera();

		string[] activeBlocks =  dataStrings[4].Split(' '); // cast to int[] maybe ? https://stackoverflow.com/questions/2068120/c-sharp-cast-entire-array

		int i = 0;
		for (int x = 0; x < gameSize.x; x++)
		{
			for (int y = 0; y < gameSize.y; y++)
			{
				for (int z = 0; z < gameSize.z; z++)
				{
					Vector3 currentPos = new Vector3(x - (gameSize.x / 2f) + 0.5f, y - (gameSize.y / 2f) + 0.5f, z - (gameSize.z / 2f) + 0.5f);
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
						currentScript.Reactivate();

					blocks[i] = currentBlock;
					i++;
				}
			}
		}
	}

	public void GridSave()
	{
		string[] dataStrings = new string[blocks.Length + 4];

		if (puzzleName == "" || puzzleName == null)
			dataStrings[0] = "[MISSING_NAME]";
		else
			dataStrings[0] = puzzleName;

		dataStrings[1] = gameSize.x.ToString() + " " + gameSize.y.ToString() + " " + gameSize.z.ToString();

		dataStrings[2] = "nothing here";
		dataStrings[3] = "nothing here";

		string[] activeBlocks = new string[blocks.Length];
		int i = 0;
		foreach (GameObject obj in blocks)
		{
			if (obj.GetComponent<Picross_Block>().isActive == true)
				activeBlocks[i] = "1";
			else
				activeBlocks[i] = "0";

			dataStrings[4] = string.Join(" ", activeBlocks);
			i++;
		}

		System.IO.File.WriteAllLines(fileLocation + fileSaveName, dataStrings);
		Debug.Log("Saved to file");
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, gameSize);
	}
}
