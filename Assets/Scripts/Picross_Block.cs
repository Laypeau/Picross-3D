using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Block : MonoBehaviour
{
	public Color vertexColours = Color.red;
	public bool isSolution = false;
	public bool isActive = true; //If the block is currently active within the scene. Used for saving
	public bool isMarked = false;

	private Picross_Master master;
	[SerializeField]private Mesh mesh;
	private Material blockMat;

	void Awake()
	{
		master = GameObject.FindObjectOfType<Picross_Master>();
		mesh = GetComponent<MeshFilter>().mesh;
		blockMat = GetComponent<MeshRenderer>().material;

		Color[] colours = new Color[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			colours[i] = vertexColours;
		}
		mesh.colors = colours;
	}

	/// <summary> Sets the vertex colours of this particular instance </summary>
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

	/// <summary> PLAY MODE -- Removes the cube, taking into account whether it is marked or part of the solution </summary>
	public void Destroy()
	{
		if (isMarked)
		{
			// Do some VFX
		}
		else
		{
			if (isSolution)
			{
				Debug.Log("Incorrect block destroyed");
				master.MistakesIncrement();
				StartCoroutine(master.LockActions(0.2f));
				// Do some visual feedback
			}
			else
			{
				isActive = false;
				GetComponent<MeshRenderer>().enabled = false;
				GetComponent<Collider>().enabled = false;

				master.totalBlocksDestroyed += 1;
			}

			if (master.totalBlocksDestroyed == master.totalBlocksSolution)
			{
				Debug.Log("Winner is you");
			}
		}
	}

	/// <summary> PLAY MODE -- Marks the cube so Destroy() has no effect on it </summary>
	public void Mark()
	{
		if (isMarked)
		{
			isMarked = false;
			blockMat.SetInt("Marked",0);
		}
		else
		{
			isMarked = true;
			blockMat.SetInt("Marked", 1);
		}
	}

	/// <summary> EDIT MODE -- Deactivates the cube </summary>
	public void Deactivate()
	{
		isActive = false;
		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;

		// Animation -- Zoom to nothing
	}

	/// <summary> EDIT MODE -- Activates the cube </summary>
	public void Activate()
	{
		isActive = true;
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<Collider>().enabled = true;

		// Animation -- Grow from nothing
	}
}
