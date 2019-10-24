﻿using UnityEngine;

public class AI : IHackable
{
	PatrollingAI ai;

	//movement and rotation
	public float walkSpeed = 10;
	public float groundOffset = 0.02f;

	CharacterController controller;
	[SerializeField] Transform camPos;
	float horLookSpeed = 1, vertLookSpeed = 1;
	float yaw, pitch;
	Vector3 velocity;

	//slope
	public bool onSlope;
	public LayerMask slopeLayer;
	bool isGrounded;

	public float slopeForce = 2f;
	public float distFromGround = 0.02f;

	[HideInInspector] public Animator anim;
	SoundManager soundManager;
	AudioSource audioSource;

	protected override void Start ()
	{
		controller = GetComponent<CharacterController>();
		ai = GetComponent<PatrollingAI>();
		anim = GetComponentInChildren<Animator>();
		audioSource = GetComponent<AudioSource> ();
		distFromGround = GetComponentInChildren<Renderer>().bounds.extents.y + 0.02f;
		soundManager = SoundManager.inst;
		audioSource.loop = false;
		audioSource.playOnAwake = false;
		audioSource.volume = soundManager.aiWalk.volume;
		audioSource.pitch = soundManager.aiWalk.pitch;
		controller.enabled = false;
		ai.enabled = true;
		hackableType = HackableType.AI;

		base.Start ();
	}

	protected override void Update ()
	{
		base.Update ();
		GroundAndSlopeCheck ();
		if (!hacked)
		{
			anim.SetFloat ("Speed", ai.agent.velocity.magnitude);
			Sound ();
		}
		else
			anim.SetFloat ("Speed", controller.velocity.magnitude);
	}

	void Sound ()
	{
		if (ai.agent.velocity.sqrMagnitude >= 0 && isGrounded && !audioSource.isPlaying)
			audioSource.Play ();
		
		if (ai.agent.velocity.sqrMagnitude == 0)
			audioSource.Stop ();
	}

	public override void OnHack ()
	{
		base.OnHack ();
		yaw = transform.eulerAngles.y;
		ai.agent.enabled = false;
		ai.hacked = true;
		ai.enabled = false;
		controller.enabled = true;
		audioSource.Stop ();
		Destroy (ai.GetComponent<Rigidbody> ());
	}

	public override void OnUnhack ()
	{
		base.OnUnhack ();
		ai.enabled = true;
		ai.agent.enabled = true;
		controller.enabled = false;
		ai.hacked = false;
		ai.registered = false;
		ai.sentBack = false;
		ai.gameObject.AddComponent<Rigidbody> ();
		ai.GetComponent<Rigidbody> ().useGravity = false;
	}

	public override Transform GetCameraRefPoint()
	{
		return camPos;
	}

	protected override void ExecuteHackingFunctionaliy ()
	{
		PlayerRotation ();
		PlayerMovement ();
	}
	
	void PlayerRotation()
	{
		//Camera and Player Rotation
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		transform.eulerAngles = new Vector3(0, yaw, 0);
		camera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
	}

	/* 
	void PlayerMovement()
	{
		//Default to Walk Speed if Aiming. If not aiming, check if Player is holding shift.

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		Vector3 horVelocity = (xMovement + zMovement).normalized * walkSpeed;
		anim.SetFloat("Speed", horVelocity.sqrMagnitude);

		if (horVelocity.sqrMagnitude != 0) player.SetBobSpeedAndOffset(5f, 0.03f);
		else player.SetBobSpeedAndOffset(1f, 0.015f);

		velocity = new Vector3(horVelocity.x, velocity.y, horVelocity.z);

		//Applying Gravity before moving
		velocity.y = onSlope ? -slopeForce : -9.81f * Time.deltaTime;

		if ((velocity.x != 0 || velocity.z != 0) && isGrounded && !soundManager.IsSourcePlaying (soundManager.playerWalk.sourceIndex))
		{
			if (isCrouching)
			{
				soundManager.PlaySound (soundManager.playerCrouchWalk);
			}
			else
			{
				soundManager.PlaySound (soundManager.playerWalk);
			}
		}

		if (velocity.x == 0 || velocity.z == 0)
			soundManager.StopSound (soundManager.playerWalk.sourceIndex);

		controller.Move(velocity * Time.deltaTime);
	}
	*/

	void PlayerMovement()
	{
		//if (controller.isGrounded)
		//Default to Walk Speed if Aiming. If not aiming, check if Player is holding shift.

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		Vector3 horVelocity = (xMovement + zMovement).normalized * walkSpeed;

		if (horVelocity.sqrMagnitude == 0) 
			player.SetBobSpeedAndOffset(0.75f, 0.01f); //Set Bobbing for Idle
		else
			player.SetBobSpeedAndOffset(3f, 0.04f);

		velocity = new Vector3(horVelocity.x, velocity.y, horVelocity.z);

		//Applying Gravity before moving
		velocity.y = isGrounded ? onSlope ? -slopeForce : -9.81f * Time.deltaTime : velocity.y - 9.81f * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);	

		//sound
		if ((velocity.x != 0 || velocity.z != 0) && isGrounded && !soundManager.IsSourcePlaying (soundManager.aiWalk.sourceIndex))
			soundManager.PlaySound (soundManager.aiWalk);

		if (velocity.x == 0 || velocity.z == 0)
			soundManager.StopSound (soundManager.aiWalk.sourceIndex);
	}

	void GroundAndSlopeCheck()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + controller.center, -Vector3.up, out hit, player.DistFromGround, player.groundLayer))
		{
			isGrounded = true;
			onSlope = hit.normal != Vector3.up ? true : false;
		}
		else
		{
			isGrounded = false;
			onSlope = false;
		}
	}

	public override void EnableDisableHackable (bool isEnable, ColorIdentifier controlPanelColor)
	{
		base.EnableDisableHackable(isEnable, controlPanelColor);
		anim.SetBool("Disabled", isDisabled);

		if (isDisabled)
		{
			if (hacked)
				ForcedUnhack ();

			ai.agent.enabled = false;
			ai.enabled = false;
			controller.enabled = false;
			ai.sentBack = true;
		}
	}
}