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

	[Header("Camera")]
	public float minCamDistance; //remember that the game is moved by half the size
	private float maxCamDistance;
	public float zoomSensitivity = 1f;
	private bool rotating = false;
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

	// fix the camera snapping on start
	// camera move when shift+mouse0 or mouseDown0 when not over blocks
	// merge mousedown 0 and 1 input statements
	// Create UI colour selector
	//	- use vertex colours

	void Update()
	{
		Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Input.GetMouseButtonDown(0))
		{
			Debug.DrawRay(clickRay.origin, clickRay.direction * 50f, Color.magenta);
			if (Physics.Raycast(clickRay, out RaycastHit rayHit, 50f))	//Layermasking didn't work so it was removed. Retry?
			{
				rayHit.transform.gameObject.GetComponent<Picross_Block>().Deactivate();
			}
		}
		else if (Input.GetMouseButtonDown(1) && Physics.Raycast(clickRay, out RaycastHit rayHit, 50f))
		{
			Debug.DrawRay(clickRay.origin, clickRay.direction * 50f, Color.green);
			transform.Find((rayHit.transform.position + rayHit.normal).ToString()).gameObject.GetComponent<Picross_Block>().Reactivate(); //lol at error
		}

		if (!rotating)
		{
			if (Input.GetMouseButton(2))
			{
				cameraFocus.rotation = Quaternion.Euler(0f, (Input.mousePosition.x - mousePrevY) * mouseSensitivityY, 0f) * cameraFocus.rotation * Quaternion.Euler((mousePrevX - Input.mousePosition.y) * mouseSensitivityX, 0f, 0f);

			}
			else
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
		}
		mousePrevX = Input.mousePosition.y;
		mousePrevY = Input.mousePosition.x;
		
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
		rotating = true;

		float startTime = Time.time;
		Quaternion initialRot = cameraFocus.rotation;

		while(startTime + duration >= Time.time) //make it evaluate an animation curve
		{
			float percentage = (Time.time - startTime) / duration;
			cameraFocus.rotation = Quaternion.Euler(0f, yAmount * percentage, 0f) * initialRot * Quaternion.Euler(xAmount * percentage, 0f, 0f);
			yield return null;
		}
		
		cameraFocus.rotation = Quaternion.Euler(0f, yAmount, 0f) * initialRot * Quaternion.Euler(xAmount, 0f, 0f);
		rotating = false;
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

					if (dataStrings[i + 4].Split(' ')[3] == "0")
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

		int i = 4;
		foreach (GameObject obj in blocks)
		{
			string temp;
			if (obj.GetComponent<Picross_Block>().isActive == true)
				temp = "1";
			else
				temp = "0";
			
			dataStrings[i] = obj.GetComponent<Picross_Block>().gridPos.x.ToString() + " " + obj.GetComponent<Picross_Block>().gridPos.y.ToString() + " " + obj.GetComponent<Picross_Block>().gridPos.z.ToString() + " " + temp;
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
