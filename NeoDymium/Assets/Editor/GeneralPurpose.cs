using UnityEngine;
using UnityEditor;

public class GeneralPurpose : EditorWindow
{
    [MenuItem ("Window/General Purpose Tools")]
	public static void ShowWindow ()
	{
		GetWindow<GeneralPurpose> ("General Purpose Tools");
	}

	void OnGUI ()
	{
		GUILayout.Label ("Reorder children of selected game object according to their names");
		if (GUILayout.Button ("Dewit"))
		{
			GameObject parent = Selection.gameObjects[0];
			
		}
	}

}