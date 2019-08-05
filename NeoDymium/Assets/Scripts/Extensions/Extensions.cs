using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
	/// <summary>
	/// Checks if a Renderer is visible from a Specific Camera
	/// </summary>
	/// <param name="collider">Renderer that is to be checked for Visibility</param>
	/// <param name="camera">From which Camera Perspective</param>
	/// <returns>Returns Returns true if Renderer is Visible from Specified Camera. If not, returns false.</returns>
	public static bool IsVisibleFrom(this Collider collider, Camera camera) //this Renderer means that it will be used as Renderer.IsVisibleFrom()
	{
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		if (GeometryUtility.TestPlanesAABB(camPlanes, collider.bounds)) return (RaycastCheck(collider, camera));//return PlayerCanBeSeen(collider.GetComponentInParent<CharacterController>(), camera);
		else return false;
	}

	//Need to Manually Adjust Player's Bounds as the Player Bounds is still a little Bigger than what its meant to be... hence having some frames where you cant see the Player but still registered as detected
	public static bool RaycastCheck(Collider collider, Camera camera, float offset = 0.15f, bool debugMode = false)
	{
		RaycastHit hit;

		Vector3 boundsCenter = collider.bounds.center;
		Vector3 boundsExtents = collider.bounds.extents;
		Vector3 cameraPos = camera.transform.position;
		float camDist = camera.farClipPlane;

		if (debugMode)
		{
			Debug.DrawLine(cameraPos, boundsCenter, Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(0, boundsExtents.y - offset, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(0, -boundsExtents.y + offset, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(boundsExtents.x - offset, 0, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(-boundsExtents.x + offset, 0, 0), Color.red, 2);
		}

		if (Physics.Raycast(cameraPos, (boundsCenter - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == collider)
			{
				Debug.Log("Hit by Center");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(0, boundsExtents.y - offset, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider == collider)
			{
				Debug.Log("Hit by Top");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(0, -boundsExtents.y + offset, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider == collider)
			{
				Debug.Log("Hit by Bottom");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(boundsExtents.x - offset, 0, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider == collider)
			{
				Debug.Log("Hit by Right");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(-boundsExtents.x + offset, 0, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider == collider)
			{
				Debug.Log("Hit by Left");
				return true;
			}
		}

		return false;
	}

	#region Renderer Extensions
	/// <summary>
	/// Checks if a Renderer is visible from a Specific Camera
	/// </summary>
	/// <param name="renderer">Renderer that is to be checked for Visibility</param>
	/// <param name="camera">From which Camera Perspective</param>
	/// <returns>Returns Returns true if Renderer is Visible from Specified Camera. If not, returns false.</returns>
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera) //this Renderer means that it will be used as Renderer.IsVisibleFrom()
	{
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		if (GeometryUtility.TestPlanesAABB(camPlanes, renderer.bounds)) return (RaycastCheck(renderer, camera));//return PlayerCanBeSeen(renderer.GetComponentInParent<CharacterController>(), camera);
		else return false;
	}

	//Need to Manually Adjust Player's Bounds as the Player Bounds is still a little Bigger than what its meant to be... hence having some frames where you cant see the Player but still registered as detected
	public static bool RaycastCheck(Renderer renderer, Camera camera, float offset = 0.15f, bool debugMode = false)
	{
		RaycastHit hit;

		Vector3 boundsCenter = renderer.bounds.center;
		Vector3 boundsExtents = renderer.bounds.extents;
		Vector3 cameraPos = camera.transform.position;
		float camDist = camera.farClipPlane;

		if (debugMode)
		{
			Debug.DrawLine(cameraPos, boundsCenter, Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(0, boundsExtents.y - offset, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(0, -boundsExtents.y + offset, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(boundsExtents.x - offset, 0, 0), Color.red, 2);
			Debug.DrawLine(cameraPos, boundsCenter + new Vector3(-boundsExtents.x + offset, 0, 0), Color.red, 2);
		}

		if (Physics.Raycast(cameraPos, (boundsCenter - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Center");
				return true;
			} 
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(0, boundsExtents.y - offset, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Top");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(0, -boundsExtents.y + offset, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Bottom");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(boundsExtents.x - offset, 0, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Right");
				return true;
			}
		}
		if (Physics.Raycast(cameraPos, (boundsCenter + new Vector3(-boundsExtents.x + offset, 0, 0) - cameraPos).normalized, out hit, camDist))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Left");
				return true;
			}
		}

		return false;
	}
	#endregion
}
