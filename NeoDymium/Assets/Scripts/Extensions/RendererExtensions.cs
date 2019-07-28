using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RendererExtensions
{
	/// <summary>
	/// Checks if a Renderer is visible from a Specific Camera
	/// </summary>
	/// <param name="renderer">Renderer that is to be checked for Visibility</param>
	/// <param name="camera">From which Camera Perspective</param>
	/// <returns>Returns Returns true if Renderer is Visible from Specified Camera. If not, returns false.</returns>
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera) //this Renderer means that it will be used as Renderer.IsVisibleFrom()
	{
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		if (GeometryUtility.TestPlanesAABB(camPlanes, renderer.bounds)) return (MultipleRaycastCheck(renderer, camera));
		else return false;
	}

	//May want to have different offsets or uae Charac
	public static bool MultipleRaycastCheck(Renderer renderer, Camera camera)
	{
		RaycastHit hit;
		if (Physics.Raycast(camera.transform.position, (renderer.bounds.center - camera.transform.position), out hit, camera.farClipPlane))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				//Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(renderer.bounds.extents.x, 0, 0), Color.red, 2);
				Debug.Log("Hit by Center");
				return true;
			} 
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y - 0.2f, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				//Debug.DrawLine(camera.transform.position, renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y - 0.2f, 0), Color.red, 2);
				Debug.Log("Hit by Top");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center - new Vector3(0, renderer.bounds.extents.y + 0.2f, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				//Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(0, renderer.bounds.extents.y + 0.2f, 0), Color.red, 2);
				Debug.Log("Hit by Bottom");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center + new Vector3(renderer.bounds.extents.x - 0.2f, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				//Debug.DrawLine(camera.transform.position, renderer.bounds.center + new Vector3(renderer.bounds.extents.x - 0.2f, 0, 0), Color.red, 2);
				Debug.Log("Hit by Right");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center - new Vector3(renderer.bounds.extents.x + 0.2f, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				//Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(renderer.bounds.extents.x + 0.2f, 0, 0), Color.red, 2);
				Debug.Log("Hit by Left");
				return true;
			}
		}

		return false;
	}
}
