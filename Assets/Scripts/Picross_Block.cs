using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picross_Block : MonoBehaviour
{
	public bool isActive = true;
	public Vector3 gridPos;

	void Awake()
	{

	}

	public void Deactivate()
	{
		isActive = false;
		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;

		// Animation clip
	}

	public void Reactivate()
	{
		isActive = true;
		GetComponent<MeshRenderer>().enabled = true;
		GetComponent<Collider>().enabled = true;

		// Animation
		//	- Grow from nothing
	}
}
