using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Block : MonoBehaviour
{
	public bool isActive = true; //For saving
	public Vector3Int vertexColours = Vector3Int.zero; // Maybe change to a colour datatype
	public Vector3 gridPos;

	public void Destroy()
	{
		isActive = false;
		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;

		// Check for win
	}

	public void Mark()
	{

	}

	public void Deactivate()
	{
		isActive = false;
		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;

		// Animation -- Zoom to nothing

	}

	public void Activate()
	{
		isActive = true;
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<Collider>().enabled = true;

		// Animation -- Grow from nothing
	}
}
