using UnityEngine;
using UnityEditor;

public class PrefabTool : EditorWindow
{
	GameObject replacementPrefab;
	string replacementName;

	[MenuItem ("Window/Prefab Tool")]
	public static void ShowWindow ()
	{
		GetWindow<PrefabTool> ("Prefab Tool");
	}

	void OnGUI ()
	{
		replacementPrefab = EditorGUILayout.ObjectField ("Replacement Prefab", replacementPrefab, typeof (GameObject), false) as GameObject;

		GUILayout.Label ("Overwrites everything except transform variables");
		if (GUILayout.Button ("Replace"))
		{
			if (replacementPrefab == null)
				return;

			foreach (GameObject obj in Selection.objects)
			{
				Transform parent = null;
				if (obj.transform.parent != null)
					parent = obj.transform.parent;
				
				Vector3 position = obj.transform.position;
				Quaternion rotation = obj.transform.rotation;
				Vector3 scale = obj.transform.localScale;

				GameObject newObj = (GameObject) PrefabUtility.InstantiatePrefab (replacementPrefab);
				newObj.transform.parent = parent;
				newObj.transform.position = position;
				newObj.transform.rotation = rotation;
				newObj.transform.localScale = scale;

				DestroyImmediate (obj);
			}
		}

		GUILayout.Label ("Rename");

		replacementName = EditorGUILayout.TextField ("Replacement Name", replacementName);

		if (GUILayout.Button ("Rename"))
		{
			foreach (GameObject obj in Selection.objects)
			{
				obj.name = replacementName;
			}
		}
	}
}