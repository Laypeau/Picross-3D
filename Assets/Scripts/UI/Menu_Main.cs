using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class Menu_Main : MonoBehaviour
{
	#region
	public Animator TitleMenu; //Set in inspector
	public Animator FolderMenu; //Set in inspector
	public GameObject tilePrefab; //Set in inspector

	public int CurrentFolderMenuIndex = 1;

	public string location = @"C:\Users\realw\Desktop\";
	DirectoryInfo dir;
	#endregion

	void Awake()
	{
		if (TitleMenu == null || FolderMenu == null) Debug.LogError("Menu panel references not set");
		dir = new DirectoryInfo(location);
	}

	void OnValidate()
	{
		dir = new DirectoryInfo(location);
	}

	public void QuitApplication()
	{
		Application.Quit();
	}

	public void OpenFolderMenu()
	{
		TitleMenu.SetBool("isActive", false);
		StartCoroutine(DelayedAnimatorSetBool(FolderMenu, 0.5f, "isActive", true));

		FileInfo[] info = dir.GetFiles("*.p3d");
		foreach (FileInfo file in info)
		{
			GameObject tile = Instantiate(tilePrefab);
		}
	}

	IEnumerator DelayedAnimatorSetBool(Animator animator, float delay, string name, bool value)
	{
		yield return new WaitForSeconds(delay);
		animator.SetBool(name, value);
	}

	public void OpenTitleMenu()
	{
		FolderMenu.SetBool("isActive", false);
		StartCoroutine(DelayedAnimatorSetBool(TitleMenu, 0.5f, "isActive", true));
	}
}
