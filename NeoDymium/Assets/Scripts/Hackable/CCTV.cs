﻿using UnityEngine;

public class CCTV : IHackable
{
	[Header("CCTV Properties")]
	public bool isSpecialCam; //To view Hidden Layer Cameras //Not Sure if this Bool is needed //For Hidden Objects, put them in Hidden Layer Mask
	public bool isStatic;
	public float degreesDelta = 1; //Look Speed for Camera
	public float horRot, vertRot; //Runtime CCTV Rotations
	public float maxHorRotFromCenter = 90, maxVertRotFromCenter = 30;
	[SerializeField] float minHorRot, maxHorRot, minVertRot, maxVertRot; //Rotation Thresholds

	protected override void Start()
	{
		//Important Note that it is using Local Euler Angles... 
		//If there is to be changes in the Parenting... 
		//There is a need to change how the Script works
		horRot = transform.localEulerAngles.y;
		vertRot = transform.localEulerAngles.x;

		maxHorRot = maxHorRotFromCenter;
		minHorRot = -maxHorRotFromCenter;
		maxVertRot = maxVertRotFromCenter;
		minVertRot = -maxVertRotFromCenter;
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

		horRot += Input.GetAxis("Mouse X") * degreesDelta; //* Time.deltaTime;
		vertRot -= Input.GetAxis("Mouse Y") * degreesDelta; //* Time.deltaTime;

		horRot = Mathf.Clamp(horRot, minHorRot, maxHorRot);
		vertRot = Mathf.Clamp(vertRot, minVertRot, maxVertRot);
		transform.localEulerAngles = new Vector3(vertRot, horRot, 0);
	}
}