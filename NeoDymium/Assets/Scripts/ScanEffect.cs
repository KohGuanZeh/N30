using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanEffect : MonoBehaviour
{
	[Header("Effect Components")]
	[SerializeField] LineRenderer linePreset; //Prefab of Lines
	[SerializeField] LineRenderer[] lineRs;
	[SerializeField] Material lineMat;
	[SerializeField] [ColorUsage(false, true)] Color[] emissiveColors;

	[Header("Scan Effect Properties")]
	[SerializeField] LayerMask hitLayers;
	[SerializeField] float startTimeOffset;
	[SerializeField] float maxDist = 50, endOffsetVal = 0.15f, speed = 0.45f;
	[SerializeField] float maxVertAngle, maxHorAngle, stepAngle;

	public void InitialiseScanLine(Camera cam, ColorIdentifier color)
	{
		startTimeOffset = Random.Range(0f, 10f);
		
		float vertFov = cam.fieldOfView;
		float horFov = GetHorFov(vertFov, cam.aspect);
		lineRs = new LineRenderer[Mathf.FloorToInt(horFov / stepAngle) + 1];
		stepAngle = horFov / lineRs.Length; //Recalculate Step Angle (Angle Interval)

		Material matInst = new Material(lineMat);

		for (int i = 0; i < lineRs.Length; i++)
		{
			lineRs[i] = Instantiate(linePreset, transform, false);
			if (i == 0) matInst.SetColor("_EmissiveColor", GetEmissiveColor(color));
			lineRs[i].material = matInst;
		} 

		//From Angle should be from -fov/2 to fov/2
		//maxVertAngle = vertFov/2; //Uncomment if you want Vert Angle to stay true to View Frustrum
		maxHorAngle = horFov/2;
	}

	// Update is called once per frame
	void Update()
	{
		UpdateScanLines();
    }

	void UpdateScanLines()
	{
		float elevationAngle = MathFunctions.SmoothPingPong((float)LoadingScreen.inst.GetTimeElapsed() + startTimeOffset, maxVertAngle, speed, -maxVertAngle, true);
		float horAngle = maxHorAngle;
		RaycastHit hit;

		for (int i = 0; i < lineRs.Length; i++)
		{
			if (i > 0)
			{
				elevationAngle = -elevationAngle; //Reverse the Vertical Direction each time
				horAngle -= stepAngle;
			}

			Vector3 dir = (Quaternion.Euler(elevationAngle, horAngle, 0) * transform.forward).normalized; //Convert Angle to Direction Vector
			Vector3 maxPos = transform.position + maxDist * dir;
			Vector3 offset = dir * endOffsetVal;

			if (Physics.Linecast(transform.position, maxPos, out hit, hitLayers)) maxPos = hit.point;
			//Debug.DrawLine(transform.position, maxPos, Color.red);

			//Set via World Position. Local Position gives weird results
			lineRs[i].SetPosition(0, transform.position);
			lineRs[i].SetPosition(1, maxPos - offset);
		}
	}

	float GetHorFov(float vertFov, float camAspect)
	{
		float vertFovRad = vertFov * Mathf.Deg2Rad;
		float horFovRad = 2 * Mathf.Atan(Mathf.Tan(vertFovRad / 2) * camAspect); //mathf.tan(vertFovRad/2) - Hieght of Frustrum - https://forum.unity.com/threads/how-to-calculate-horizontal-field-of-view.16114/
		return horFovRad * Mathf.Rad2Deg;
	}
	 Color GetEmissiveColor(ColorIdentifier colorId)
	{
		switch (colorId)
		{
			case ColorIdentifier.red:
				return emissiveColors[0];
			case ColorIdentifier.blue:
				return emissiveColors[1];
			case ColorIdentifier.yellow:
				return emissiveColors[2];
			case ColorIdentifier.green:
				return emissiveColors[3];
			default:
				return Color.white;
		}
	}
}
