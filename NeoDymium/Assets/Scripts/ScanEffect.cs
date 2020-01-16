using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanEffect : MonoBehaviour
{
	[SerializeField] LineRenderer lineRenderer;
	[SerializeField] LayerMask hitLayers;
	[SerializeField] float maxDist;
	[SerializeField] bool isLeft;
	[SerializeField] bool reversed;

    // Start is called before the first frame update
    void Start()
    {
	}

	// Update is called once per frame
	void Update()
	{
		float elevationAngle = MathFunctions.SmoothPingPong((float)LoadingScreen.inst.GetTimeElapsed(), 15, 0.25f, -15, true);
		if (reversed) elevationAngle = -elevationAngle;

		Vector3 dir = (Quaternion.Euler(elevationAngle, isLeft ? -25 : 25, 0) * Vector3.forward).normalized;
		Vector3 maxPosition = transform.position + maxDist * dir;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, maxPosition, out hit, hitLayers))
		{
			//print(hit.collider.gameObject.name);
			maxPosition = hit.point;
		}

		//Debug.DrawLine(transform.position, maxPosition, Color.red);
		lineRenderer.SetPosition(1, maxPosition - transform.position - (dir * 0.2f));
    }
}
