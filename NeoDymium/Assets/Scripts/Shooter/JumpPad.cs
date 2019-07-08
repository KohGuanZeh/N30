using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpPad : MonoBehaviour
{
	[Header("Jump Pad Properties")]
	[SerializeField] PlayerShooterController player; //May want to store the Player here so there is no need to GetComponent
	[SerializeField] Transform jumpDestination; //Stores the Transform of where the Player should go to
	[SerializeField] float amplitude; //Maximum Height of the Trajectory. Need to be reversed engineered as the Amplitude changes depending on the Jump Pad's Position
	[SerializeField] float targetStep = 0.7f, step; //Target Step should be a fixed for all Jump Pads. Modifiable for now for Designer's use. Target Step is the x value you want the arc to stop at in reference to Y = Sin(Pi)X
	[SerializeField] bool travelByTime; //Use time? If false, speed will be used instead
	[SerializeField] float travelTime, travelSpeed = 2; //Determines how long you want for the Player to reach target destination
	[SerializeField] Vector3 startPosition, targetPosition; //Gets the Position of the Jump Destination + Player Collider Bounds Extents.
	[SerializeField] bool playerIsJumping; //To indicate if the Jump Pad should be lerping the Player

	[SerializeField] bool jumpPadIsSet; //Required as Intialising Start Postion on Start() causes problem since Jump Pad initializes first.

	private void Start()
	{
		player = PlayerShooterController.inst;

		//Setting Jump Pad Properties
		//Set once Player goes onto Jump Pad. Can be avoided if we set Player Script to intialise first either in Awake or in Unity Intialisation Pipeline
		/*startPosition = transform.position + new Vector3(0, player.distFromGround, 0);
		targetPosition = jumpDestination.position + new Vector3(0, player.distFromGround, 0);
		amplitude = (targetPosition.y - startPosition.y) / Mathf.Sin(targetStep * Mathf.PI);
		if (travelByTime) travelSpeed = 1 / travelTime;*/
	}

	private void Update()
	{
		if (playerIsJumping) LaunchTarget(player.transform);
	}

	public void LaunchTarget(Transform target)
	{
		//player.transform.position = jumpDestination.position + new Vector3(0, PlayerShooterController.inst.distFromGround, 0);
		step = Mathf.Min(step + Time.deltaTime * travelSpeed, 1); //The Travel by Time part may not be accurate since its Using Time.deltaTime. Use Time.time for more accuracy
		player.transform.position = JumpPadTrajectory(startPosition, targetPosition, step);
		if (step >= 1)
		{
			playerIsJumping = false;
			//player.ToggleRbKinematic(false);
			player.lockMovement = false;

			step = 0;
		}
	}

	//Target Step is the x value you want the arc to stop at in reference to Y = Sin(Pi)X. Feel free to rename it
	Vector3 JumpPadTrajectory(Vector3 startPos, Vector3 endPos, float currentStep)
	{
		Vector2 horPos = new Vector2((endPos.x - startPos.x) * currentStep + startPos.x, (endPos.z - startPos.z) * currentStep + startPos.z);
		float yPos = startPos.y + (amplitude  * Mathf.Sin(targetStep * Mathf.PI * currentStep)); //Formular Example y = sin0.7 * pi x
		return Vector3.Lerp(player.transform.position, new Vector3(horPos.x, yPos, horPos.y), currentStep);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") //If there is anything Parented under Player and has collider, may be an issue. Might use GetComponent instead though its more expensive
		{
			if (!jumpPadIsSet)
			{
				startPosition = transform.position + new Vector3(0, player.distFromGround, 0);
				targetPosition = jumpDestination.position + new Vector3(0, player.distFromGround, 0);
				amplitude = (targetPosition.y - startPosition.y) / Mathf.Sin(targetStep * Mathf.PI);
				if (travelByTime) travelSpeed = 1 / travelTime;
			}

			player.StopPlayerMovementImmediately();
			//player.ToggleRbKinematic(true);
			player.lockMovement = true;
			other.transform.position = startPosition;
			playerIsJumping = true;
		}
	}
}
