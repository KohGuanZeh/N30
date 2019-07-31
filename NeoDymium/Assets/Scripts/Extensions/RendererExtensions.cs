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
		if (GeometryUtility.TestPlanesAABB(camPlanes, renderer.bounds)) return (MultipleRaycastCheck(renderer, camera)); //return PlayerCanBeSeen(renderer.GetComponentInParent<CharacterController>(), camera);
		else return false;
	}

	//May want to have different offsets or uae Character Controller
	public static bool MultipleRaycastCheck(Renderer renderer, Camera camera, float offset = 0.35f, bool debugMode = true)
	{
		RaycastHit hit;

		if (debugMode)
		{
			Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(renderer.bounds.extents.x, 0, 0), Color.red, 2);
			Debug.DrawLine(camera.transform.position, renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y - offset, 0), Color.red, 2);
			Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(0, renderer.bounds.extents.y + offset, 0), Color.red, 2);
			Debug.DrawLine(camera.transform.position, renderer.bounds.center + new Vector3(renderer.bounds.extents.x - offset, 0, 0), Color.red, 2);
			Debug.DrawLine(camera.transform.position, renderer.bounds.center - new Vector3(renderer.bounds.extents.x + offset, 0, 0), Color.red, 2);
		}

		//Need to Manually Adjust Player's Bounds
		if (Physics.Raycast(camera.transform.position, (renderer.bounds.center - camera.transform.position), out hit, camera.farClipPlane))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Center");
				return true;
			} 
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y - offset, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Top");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center - new Vector3(0, renderer.bounds.extents.y + offset, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Bottom");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center + new Vector3(renderer.bounds.extents.x - offset, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Right");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((renderer.bounds.center - new Vector3(renderer.bounds.extents.x + offset, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<Renderer>() == renderer)
			{
				Debug.Log("Hit by Left");
				return true;
			}
		}

		return false;
	}

	public static bool PlayerCanBeSeen(CharacterController player, Camera camera, float offset = 0.1f)
	{
		RaycastHit hit;

		Debug.DrawLine(camera.transform.position, player.bounds.center - new Vector3(player.bounds.extents.x, 0, 0), Color.red, 2);
		Debug.DrawLine(camera.transform.position, player.bounds.center + new Vector3(0, player.bounds.extents.y - offset, 0), Color.red, 2);
		Debug.DrawLine(camera.transform.position, player.bounds.center - new Vector3(0, player.bounds.extents.y + offset, 0), Color.red, 2);
		Debug.DrawLine(camera.transform.position, player.bounds.center + new Vector3(player.bounds.extents.x - offset, 0, 0), Color.red, 2);
		Debug.DrawLine(camera.transform.position, player.bounds.center - new Vector3(player.bounds.extents.x + offset, 0, 0), Color.red, 2);

		if (Physics.Raycast(camera.transform.position, (player.bounds.center - camera.transform.position), out hit, camera.farClipPlane))
		{
			Debug.Log(camera.transform.name + " Center hit " + hit.transform.name);
			if (hit.collider.GetComponentInChildren<CharacterController>() == player)
			{
				Debug.Log("Hit by Center");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((player.bounds.center + new Vector3(0, player.bounds.extents.y - offset, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			Debug.Log(camera.transform.name + " Top hit " + hit.transform.name);
			if (hit.collider.GetComponentInChildren<CharacterController>() == player)
			{
				Debug.Log("Hit by Top");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((player.bounds.center - new Vector3(0, player.bounds.extents.y + offset, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<CharacterController>() == player)
			{
				Debug.Log("Hit by Bottom");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((player.bounds.center + new Vector3(player.bounds.extents.x - offset, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<CharacterController>() == player)
			{
				Debug.Log("Hit by Right");
				return true;
			}
		}
		if (Physics.Raycast(camera.transform.position, ((player.bounds.center - new Vector3(player.bounds.extents.x + offset, 0, 0)) - camera.transform.position).normalized, out hit, Mathf.Infinity))
		{
			if (hit.collider.GetComponentInChildren<CharacterController>() == player)
			{
				Debug.Log("Hit by Left");
				return true;
			}
		}

		return false;
	}
}
