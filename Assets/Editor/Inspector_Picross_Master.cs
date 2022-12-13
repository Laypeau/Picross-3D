using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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

			if (GUILayout.Button("Load Play"))
			{
				master.GridLoadPlay();
			}

			if (GUILayout.Button("Load Edit"))
			{
				master.GridLoadEdit();
			}

			if (GUILayout.Button("Save to file"))
			{
				master.GridSave();
			}

		GUILayout.EndHorizontal();

		base.OnInspectorGUI();
	}
}
