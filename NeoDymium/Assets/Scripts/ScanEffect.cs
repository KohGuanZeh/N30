using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanEffect : MonoBehaviour
{
	[Header("Effect Components")]
	[SerializeField] LineRenderer[] lineRs;
	[SerializeField] MeshFilter[] scanPlaneMesh;
	[SerializeField] MeshRenderer[] scanPlaneR;

	[Header("Scan Effect Properties")]
	[SerializeField] LayerMask hitLayers;
	[SerializeField] float startTimeOffset;
	[SerializeField] float maxDist = 50, endOffsetVal = 0.15f, speed = 0.45f;
	[SerializeField] float angleOfElevation = 15, outwardAngle = 25;
	[SerializeField] float planeElevationAngleOffset = 15;

	[Header("Scan Plane Variables")]
	[SerializeField] int vertCount; //How many Vertices Scan Plane should have
	[SerializeField] float stepAngle = 3; //Next Angle step that the Raycast should be Performed

    // Start is called before the first frame update
    void Start()
    {
		startTimeOffset = Random.Range(0f, 10f);

		float totalAngle = (angleOfElevation - planeElevationAngleOffset) * 2;
		vertCount = Mathf.FloorToInt(totalAngle / stepAngle) + 2; //Total Number of Vertices of the Plane
		stepAngle = totalAngle / (vertCount - 2); //Make the Angular Steps spread evenly throughout the total angle.
	}

	// Update is called once per frame
	void Update()
	{
		UpdateScanLines();
		DrawScanPlanes();
    }

	void UpdateScanLines()
	{
		float elevationAngle = MathFunctions.SmoothPingPong((float)LoadingScreen.inst.GetTimeElapsed() + startTimeOffset, angleOfElevation, speed, -angleOfElevation, true);
		float horAngle = outwardAngle;
		RaycastHit hit;

		for (int i = 0; i < lineRs.Length; i++)
		{
			if (i == 2) horAngle = -horAngle; //Reverse Horizontal Direction after the second Line Renderer
			if (i > 0) elevationAngle = -elevationAngle; //Reverse the Vertical Direction each time

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

	void DrawScanPlanes()
	{
		bool isLeft = false;

		foreach (MeshFilter meshFilter in scanPlaneMesh)
		{
			isLeft = !isLeft;
			Mesh mesh = new Mesh();

			mesh.SetVertices(SetVertices(vertCount, isLeft));
			mesh.SetTriangles(SetTris(vertCount), 0);
			mesh.RecalculateNormals();

			meshFilter.sharedMesh = mesh;
		}
	}

	List<Vector3> SetVertices(int vertCount, bool isLeft)
	{
		RaycastHit hit;
		float totalAngle = angleOfElevation - planeElevationAngleOffset;
		float horAngle = isLeft ? -outwardAngle: outwardAngle;
		List<Vector3> verts = new List<Vector3>();
		verts.Add(Vector3.zero); //Start Point

		for (int i = 0; i < vertCount - 1; i++)
		{
			float elevationAngle = totalAngle - stepAngle * i;
			Vector3 dir = (Quaternion.Euler(elevationAngle, horAngle, 0) * transform.forward).normalized; //Convert Angle to Direction Vector
			Vector3 localDir = (Quaternion.Euler(elevationAngle, horAngle, 0) * Vector3.forward).normalized;
			Vector3 maxPos = transform.position + maxDist * dir;
			Vector3 offset = dir * endOffsetVal;

			if (Physics.Linecast(transform.position, maxPos, out hit, hitLayers)) maxPos = hit.point;

			verts.Add((maxPos - transform.position).magnitude * localDir - offset);
		}

		for (int i = 1; i < verts.Count; i++)
		{
			Debug.DrawLine(transform.position + verts[i - 1], transform.position + verts[i], Color.green);
			if (i == verts.Count - 1) Debug.DrawLine(transform.position + verts[i], transform.position + verts[0], Color.green);
		}

		return verts;
	}

	List<int> SetTris(int vertCount)
	{
		List<int> tris = new List<int>();

		for (int i = 1; i < vertCount - 1; i++)
		{
			tris.Add(0);
			tris.Add(i);
			tris.Add(i + 1);
		}

		return tris;
	}
}
