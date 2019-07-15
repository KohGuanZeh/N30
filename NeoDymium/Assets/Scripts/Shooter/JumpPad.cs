using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpPad : MonoBehaviour
{
	[Header("Jump Pad Properties")]
	[SerializeField] PlayerShooterController player; //May want to store the Player here so there is no need to GetComponent
	[SerializeField] bool travelByTime; //Use time? If false, speed will be used instead
	[SerializeField] bool playerIsJumping; //To indicate if the Jump Pad should be lerping the Player
	[SerializeField] bool jumpPadIsSet; //Required as Intialising Start Postion on Start() causes problem since Jump Pad initializes first.

	[Header ("Start and Target Positions")]
	[SerializeField] Transform jumpDestination; //Stores the Transform of where the Player should go to
	[SerializeField] Vector3 startPosition, targetPosition; //Gets the Position of the Jump Destination + Player Collider Bounds Extents.
	[SerializeField] bool targetPosIsLower;

	[Header ("For Trajectories")]
	[SerializeField] float amplitude; //Maximum Height of the Trajectory. Need to be reversed engineered as the Amplitude changes depending on the Jump Pad's Position
	[SerializeField] float amplitudeOffset;
	[SerializeField] float targetStep, step; //Target Step should be a fixed for all Jump Pads. Modifiable for now for Designer's use. Target Step is the x value you want the arc to stop at in reference to Y = Sin(Pi)X
	[SerializeField] float travelTime, travelSpeed = 2; //Determines how long you want for the Player to reach target destination

	private void Start()
	{
		player = PlayerShooterController.inst;
		targetPosIsLower = (transform.position.y >= jumpDestination.position.y);
		ResetStep();

		//Setting Jump Pad Properties
		//Set once Player goes onto Jump Pad. Can be avoided if we set Player Script to intialise first either in Awake or in Unity Intialisation Pipeline
		//InitialiseJumpPadTrajectory();
	}

	private void Update()
	{
		if (playerIsJumping) LaunchTarget(player.transform);
	}

	public void LaunchTarget(Transform target)
	{
		//The Travel by Time part may not be accurate since its Using Time.deltaTime. Use Time.time for more accuracy
		step = Mathf.Min(step + Time.deltaTime * travelSpeed, 1); 
		player.transform.position = JumpPadTrajectory(startPosition, targetPosition, step);
		if (step >= 1)
		{
			playerIsJumping = false;
			player.lockMovement = false;
			ResetStep();
		}
	}

	//Target Step is the x value you want the arc to stop at in reference to Y = Sin(Pi)X. Feel free to rename it
	Vector3 JumpPadTrajectory(Vector3 startPos, Vector3 endPos, float currentStep)
	{
		Vector2 horPos = new Vector2((endPos.x - startPos.x) * currentStep + startPos.x, (endPos.z - startPos.z) * currentStep + startPos.z);
		float yPos = targetPosIsLower ? endPos.y + (amplitude * Mathf.Sin(targetStep * Mathf.PI * (1-currentStep))) : startPos.y + (amplitude  * Mathf.Sin(targetStep * Mathf.PI * currentStep)); //Formular Example y = amplitude * sin0.7 * pi x
		return Vector3.Lerp(player.transform.position, new Vector3(horPos.x, yPos, horPos.y), currentStep);
	}

	private void InitialiseJumpPadTrajectory()
	{
		startPosition = transform.position + new Vector3(0, player.distFromGround - 0.2f, 0);
		targetPosition = jumpDestination.position + new Vector3(0, player.distFromGround - 0.2f, 0);

		amplitude = targetPosIsLower ? startPosition.y + amplitudeOffset : targetPosition.y + amplitudeOffset;
		//Reversing formula to get target step: y = amplitude * sin * targetStep * pi * x (We want X to always be 1 since X is current Step)
		//amplitude = targetPosition.y - startPosition.y <= 1 ? 10 : (targetPosition.y - startPosition.y) / Mathf.Sin(targetStep * Mathf.PI); //If you want to fix Target Step instead
		targetStep = (Mathf.Asin(Mathf.Abs(targetPosition.y - startPosition.y) / amplitude) / Mathf.PI); 
		targetStep = 1 - targetStep; //need take 1 - target step since the first target step that you get is between 0-0.5 range (Since Sine curve goes up and down and have >2 x solutions for the same y).
		if (travelByTime) travelSpeed = 1 / travelTime;

		jumpPadIsSet = true;
	}

	void ResetStep()
	{
		step = 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") //If there is anything Parented under Player and has collider, may be an issue. Might use GetComponent instead though its more expensive
		{
			if (!jumpPadIsSet) InitialiseJumpPadTrajectory();

			player.StopPlayerMovementImmediately();
			player.lockMovement = true;
			other.transform.position = startPosition;
			playerIsJumping = true;
		}
	}
}
