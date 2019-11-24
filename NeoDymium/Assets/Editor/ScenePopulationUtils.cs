using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Developed by Koh Guan Zeh
namespace XellExtraUtils
{
	public class ScenePopulationUtils : EditorWindow
	{
		enum Axis {X = 0, Y = 1, Z = 2};
		Axis rotationAxis;
		float minRot, maxRot;
		bool adjLocalRot;

		[MenuItem("Window/Scene Utils")]
		static void ShowWindow()
		{
			GetWindow<ScenePopulationUtils>("Scene Utils");
		}

		private void OnGUI()
		{
			GUILayout.Label("For Randomising Rotation");
			rotationAxis = (Axis)EditorGUILayout.EnumPopup("Rotation Axis", rotationAxis);
			minRot = EditorGUILayout.FloatField("Minimum Rotation", minRot);
			maxRot = EditorGUILayout.FloatField("Minimum Rotation", maxRot);
			adjLocalRot = EditorGUILayout.Toggle("Adjust Local Rotation", adjLocalRot);

			if (GUILayout.Button("Randomise Rotation"))
			{
				if (adjLocalRot) RandomiseRotationLocal();
				else RandomiseRotationGlobal();
			}
		}

		Vector3 GetAxisVector(Axis axis)
		{
			switch (axis)
			{
				case Axis.X:
					return new Vector3(1, 0, 0);
				case Axis.Y:
					return new Vector3(0, 1, 0);
				case Axis.Z:
					return new Vector3(0, 0, 1);
				default:
					return Vector3.zero;
			}
		}

		Vector3 ResetAxisRotation(Axis axis, Vector3 eulerAngle)
		{
			Vector3 newEulerAngle = eulerAngle;

			switch (axis)
			{
				case Axis.X:
					newEulerAngle.x = 0;
					break;
				case Axis.Y:
					newEulerAngle.y = 0;
					break;
				case Axis.Z:
					newEulerAngle.z = 0;
					break;
			}

			return newEulerAngle;
		}

		void RandomiseRotationLocal()
		{
			foreach (GameObject obj in Selection.objects)
			{
				Undo.RecordObject(obj.transform, "Undo Rotation");
				obj.transform.localEulerAngles = ResetAxisRotation(rotationAxis, obj.transform.localEulerAngles);
				obj.transform.localEulerAngles += GetAxisVector(rotationAxis) * Random.Range(minRot, maxRot);
				EditorUtility.SetDirty(obj);
			}
		}

		void RandomiseRotationGlobal()
		{
			foreach (GameObject obj in Selection.objects)
			{
				Undo.RecordObject(obj.transform, "Undo Rotation");
				obj.transform.eulerAngles = ResetAxisRotation(rotationAxis, obj.transform.eulerAngles);
				obj.transform.eulerAngles += GetAxisVector(rotationAxis) * Random.Range(minRot, maxRot);
				EditorUtility.SetDirty(obj);
			}
		}
	}
}
