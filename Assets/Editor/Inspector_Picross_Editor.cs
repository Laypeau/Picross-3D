using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Picross_Editor))]
public class Editor_Picross_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		Picross_Editor master = (Picross_Editor)target;

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
