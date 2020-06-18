using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Master : MonoBehaviour
{
	private Vector3[] tilePositions;
	private bool[] activeTiles;
	private LayerMask maskBlocks;

	public Vector3Int gameSize = Vector3Int.one;
	public GameObject blockPrefab;
	public Transform cameraFocus;

	[Header("Camera")]
	[Range(0f, 25f)] public float distance = 7f; //use pythag to set max distance based on cube corner
	public float xSensitivity = 0.2f;
	public float ySensitivity = 0.2f;
	private float prevX = 0f; //Make it able to be negative for terrible control scheme opportunities
	private float prevY = 0f;

	private void Awake()
	{
		if (blockPrefab == null)
			throw new UnityException("Block prefab not set");

		if (cameraFocus == null)
			throw new UnityException("Camera focus not set");

		tilePositions = new Vector3[gameSize.x * gameSize.y * gameSize.z];
		activeTiles = new bool[gameSize.x * gameSize.y * gameSize.z];
		maskBlocks = 8;
	}

	void Start()
	{
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

		if (Input.GetMouseButton(2))
		{
			cameraFocus.eulerAngles += new Vector3((prevX - Input.mousePosition.y) * xSensitivity, (Input.mousePosition.x - prevY) * ySensitivity, 0f);
		}
		prevX = Input.mousePosition.y;
		prevY = Input.mousePosition.x;

		//mouse scroll for back/forwards
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
					tilePositions[i] = new Vector3(x - (gameSize.x/2f) + 0.5f, y - (gameSize.y/2f) + 0.5f, z - (gameSize.z/2f) + 0.5f);
					activeTiles[i] = true;

					GameObject block = Instantiate(blockPrefab);
					block.name = tilePositions[i].ToString();
					block.layer = 8;
					block.transform.parent = transform;
					block.transform.position = tilePositions[i];
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
