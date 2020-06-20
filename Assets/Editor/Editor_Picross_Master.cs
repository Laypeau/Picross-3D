using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Picross_Master))]
public class Editor_Picross_Master : Editor
{
	public override void OnInspectorGUI()
	{
		Picross_Master master = (Picross_Master)target;

		GUILayout.BeginHorizontal();

			if (GUILayout.Button("Generate full"))
			{
				master.GridGen();
			}

			if (GUILayout.Button("Clear Grid"))
			{
				master.GridClear();
			}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();

			if (GUILayout.Button("Load from file"))
			{
				master.GridLoad();
			}

			if (GUILayout.Button("Save to file"))
			{
				master.GridSave();
			}

		GUILayout.EndHorizontal();

		base.OnInspectorGUI();
	}
}
