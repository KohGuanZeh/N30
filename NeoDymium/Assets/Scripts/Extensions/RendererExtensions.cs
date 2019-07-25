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
		return GeometryUtility.TestPlanesAABB(camPlanes, renderer.bounds);
	}
}
