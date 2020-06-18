using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Master : MonoBehaviour
{
	public Vector3Int gameSize = Vector3Int.one;
	public GameObject blockPrefab;

	private Vector3[] tilePositions;
	private bool[] activeTiles;
	private LayerMask maskBlocks;

	private void Awake()
	{
		if (blockPrefab == null)
			throw new UnityException("Block prefab not set");

		tilePositions = new Vector3[gameSize.x * gameSize.y * gameSize.z];
		activeTiles = new bool[gameSize.x * gameSize.y * gameSize.z];
		maskBlocks = 8;
	}

	void Start()
	{
		Debug.Log(LayerMask.NameToLayer("Blocks"));
		GenerateGrid();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red);
			if (Physics.Raycast(ray, out RaycastHit rayHit, 50f))
			{
				Debug.Log(rayHit.transform.name);
				transform.Find(rayHit.transform.name).gameObject.SetActive(false);	
			}
		}
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
