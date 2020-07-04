using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Block : MonoBehaviour
{
	public bool isActive = true; //For saving
	public Color vertexColours = Color.red;
	public Vector3 gridPos; //Unnecessary
	public Mesh mesh;

	private 

	void Awake()
	{
		mesh = GetComponent<MeshFilter>().mesh;

		Color[] colours = new Color[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			colours[i] = vertexColours;
		}
		mesh.colors = colours;
	}

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

	public void SetColour(Color colour)
	{
		vertexColours = colour;

		Color[] colours = new Color[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			colours[i] = vertexColours;
		}
		mesh.colors = colours;
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
