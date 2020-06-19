using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Master : MonoBehaviour
{
	private Transform[] blocks;
	private LayerMask maskBlocks;

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
	private float mousePrevX = 0f; //Make it able to be negative for terrible control scheme opportunities
	private float mousePrevY = 0f;
	public float rotateX = 15f;
	public float rotateY = 15f;

	void Awake()
	{
		if (blockPrefab == null)
			throw new UnityException("Block prefab not set");

		if (cameraFocus == null)
			throw new UnityException("Camera focus not set");

		blocks = new Transform[gameSize.x * gameSize.y * gameSize.z];
		maskBlocks = 8;

		gameCamera = cameraFocus.transform.GetChild(0);
		minCamDistance = -1 * ((gameSize.magnitude / 2f) + 1f);
		maxCamDistance = -1 * ((gameSize.magnitude * 1.5f) + 1f);
	}

	void Start()
	{
		gameCamera.position = new Vector3(0f, 0f, 2 * minCamDistance);
		GenerateGrid();
	}

	void Update()
	{
		Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Input.GetMouseButtonDown(0))
		{
			Debug.DrawRay(clickRay.origin, clickRay.direction * 50f, Color.magenta);
			if (Physics.Raycast(clickRay, out RaycastHit rayHit, 50f))	//Layermasking didn't work so it was removed. Retry?
			{
				transform.Find(rayHit.transform.name).gameObject.SetActive(false);	
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			Debug.DrawRay(clickRay.origin, clickRay.direction * 50f, Color.green);
			if (Physics.Raycast(clickRay, out RaycastHit rayHit, 50f))
			{
				transform.Find((rayHit.transform.position + rayHit.normal).ToString()).gameObject.SetActive(true); //lol at error
			}
		}

		if (!rotating)
		{
			if (Input.GetMouseButton(2))
			{
				cameraFocus.rotation = Quaternion.Euler(0f, (Input.mousePosition.x - mousePrevY) * mouseSensitivityY, 0f) * cameraFocus.rotation * Quaternion.Euler((mousePrevX - Input.mousePosition.y) * mouseSensitivityX, 0f, 0f);
			}
			else
			{
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
	void GenerateGrid()
	{
		int i = 0;
		for (int x = 0; x < gameSize.x; x++)
		{
			for (int y = 0; y < gameSize.y; y++)
			{
				for (int z = 0; z < gameSize.z; z++)
				{
					Vector3 currentPos = new Vector3(x - (gameSize.x / 2f) + 0.5f, y - (gameSize.y / 2f) + 0.5f, z - (gameSize.z / 2f) + 0.5f);
					GameObject currentGameObject = Instantiate(blockPrefab);

					currentGameObject.name = currentPos.ToString();
					currentGameObject.layer = 8;
					currentGameObject.transform.parent = transform;
					currentGameObject.transform.position = currentPos;

					blocks[i] = currentGameObject.transform;
					i++;
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, gameSize);
	}
}
