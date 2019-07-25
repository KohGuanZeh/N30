using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTV : IHackable
{
	[Header("CCTV Properties")]
	public bool isSpecialCam; //To view Hidden Layer Cameras //Not Sure if this Bool is needed //For Hidden Objects, put them in Hidden Layer Mask
	public bool isStatic;
	public float degreesDelta = 3;
	public float horRot, vertRot; //CCTV Rotations
	public float maxHorRotFromCenter = 90, maxVertRotFromCenter = 90;
	[SerializeField] float minHorRot, maxHorRot, minVertRot, maxVertRot; //Rotation Thresholds

	protected override void Start()
	{
		horRot = transform.eulerAngles.y;
		vertRot = transform.eulerAngles.x;

		maxHorRot = horRot + maxHorRotFromCenter;
		minHorRot = horRot - maxHorRotFromCenter;
		maxVertRot = vertRot + maxVertRotFromCenter;
		minVertRot = vertRot - maxVertRotFromCenter;
		base.Start();
	}

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}

	protected override void ExecuteHackingFunctionaliy()
	{
		if (isStatic) return;

		horRot += Input.GetAxisRaw("Horizontal") * degreesDelta * Time.deltaTime;
		vertRot -= Input.GetAxisRaw("Vertical") * degreesDelta * Time.deltaTime;

		horRot = Mathf.Clamp(horRot, minHorRot, maxHorRot);
		vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);
		transform.eulerAngles = new Vector3(vertRot, horRot, 0);
	}
}
