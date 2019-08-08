﻿using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
	[Header("General Variables")]
	public static PlayerController inst;
	[SerializeField] CapsuleCollider detectionColl; //Capsule Collider that is same size as Char Controller Collider. Char Controller Collider cant have proper raycast on its hemisphere so need to use this
	[SerializeField] UIManager ui;

	[Header("Player Movement")]
	[SerializeField] CharacterController controller;
	[SerializeField] Camera playerCam;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	[SerializeField] Vector3 velocity; //Player Velocity
	public float crouchSpeed = 5, walkSpeed = 10, runSpeed = 20; //Different Move Speed for Different Movement Action
	[SerializeField] bool isGrounded, onSlope;
	[SerializeField] LayerMask groundLayer;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public float groundOffset = 0.1f;
	public float DistFromGround
	{
		get { return (controller.height / 2) + groundOffset; } //playerCollider.bounds.extents.y + 0.2f; This is via collider}
	}

	[Header ("For Crouching")]
	public bool isCrouching = false;
	[SerializeField] Transform standCamPos, crouchCamPos;
	[SerializeField] float playerStandHeight, playerCrouchHeight;
	float crouchStandLerpTime;

	[Header("For Hacking and INteractions")]
	[SerializeField] Camera currentViewingCamera;
	public bool focus = false;
	public bool isHacking = false;
	public IHackable hackedObj;
	public LayerMask aimingRaycastLayers;
	public LayerMask hackableInteractableLayer;
	[SerializeField] RaycastHit aimRayHit;

	[Header("For Storing Previous Frame Hacking")]
	[SerializeField] Collider prevCollider;
	[SerializeField] IHackable focusedHackable;
	[SerializeField] IInteractable focusedInteractable;

	[Header("Stealth Gauge")]
	public float stealthThreshold;
	[SerializeField] float prevStealthGauge;
	public float stealthGauge;
	public float increaseMultiplier;
	public float decreaseMultiplier;

	[Header("Advanced Camera Movement")]
	public bool headBob = true;
	public float headBobLerpTime;
	public float maxHeadBobOffset;
	public float bobSpeed;
	public Vector3 currentHeadBobOffset;
	public Transform headRefPoint;

	[Header("For Animations")]
	[SerializeField] Animator anim;

	[Header ("Others")]
	public Action action;

	void Awake ()
	{
		inst = this;
	}

    void Start()
    {
		//Lock Cursor in Middle of Screen
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;
		yaw = transform.eulerAngles.y;

		//Getting Components
		ui = UIManager.inst;
		playerCam = GetComponentInChildren<Camera>();
		controller = GetComponent<CharacterController>();
		detectionColl = GetComponent<CapsuleCollider>();
		SetDetectionCollider();
		currentViewingCamera = playerCam;
		anim = GetComponentInChildren<Animator>();
		//Set Ground Check. May need to change the y
		//distFromGround = (controller.height/2) + groundOffset; //playerCollider.bounds.extents.y + 0.2f; This is via collider

		yaw = transform.eulerAngles.y;
		headRefPoint = standCamPos;
		action += LerpHeadBob;
	}

	void Update()
    {
		if (!ui.isPaused && !ui.isGameOver)
		{
			//mesh.mesh.RecalculateBounds();

			if (!isHacking)
			{
				GroundAndSlopeCheck();
				ToggleCrouch();
				PlayerRotation();
				PlayerMovement();
			}

			//if (Input.GetKeyDown(KeyCode.P)) ResetHeadBob();
			Aim();
			if (Input.GetKeyDown(KeyCode.E)) Interact();
			if (Input.GetMouseButtonDown(0)) Hack();
			if (Input.GetMouseButtonDown(1)) Unhack();

			if (stealthGauge >= stealthThreshold) ui.GameOver(); //May want to Add a Return if Stealth Gauge is over Stealth Threshold
			if (prevStealthGauge == stealthGauge) stealthGauge = Mathf.Max(stealthGauge - Time.deltaTime * decreaseMultiplier, 0);
			prevStealthGauge = stealthGauge;
			
			if (action != null) action();
		}
	}

	void PlayerRotation()
	{
		//Camera and Player Rotation
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		transform.eulerAngles = new Vector3(0, yaw, 0);
		playerCam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
	}

	void PlayerMovement()
	{
		//if (controller.isGrounded)
		//Default to Walk Speed if Aiming. If not aiming, check if Player is holding shift.
		float movementSpeed = isCrouching ? crouchSpeed : Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

		if (movementSpeed < walkSpeed) SetBobSpeedAndOffset(1f, 0.03f);
		else if (movementSpeed > walkSpeed) SetBobSpeedAndOffset(5f, 0.05f);
		else SetBobSpeedAndOffset(3f, 0.04f);

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		Vector3 horVelocity = (xMovement + zMovement).normalized * movementSpeed;

		if (horVelocity.sqrMagnitude == 0) SetBobSpeedAndOffset(0.75f, 0.01f); //Set Bobbing for Idle
		anim.SetFloat("Speed", horVelocity.sqrMagnitude);

		velocity = new Vector3(horVelocity.x, velocity.y, horVelocity.z);

		//Applying Gravity before moving
		velocity.y = isGrounded ? onSlope ? -slopeForce : -9.81f * Time.deltaTime : velocity.y - 9.81f * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);
	}

	void ToggleCrouch()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			isCrouching = !isCrouching;
			anim.SetBool("Crouch", isCrouching);
			action += LerpCrouchStand;
			if (!isHacking) headRefPoint = isCrouching ? crouchCamPos : standCamPos;
			headBob = false;
		}
	}

	void LerpCrouchStand()
	{
		crouchStandLerpTime = isCrouching ? Mathf.Min(crouchStandLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(crouchStandLerpTime - Time.deltaTime * 5, 0);
		playerCam.transform.position = Vector3.Lerp(standCamPos.position, crouchCamPos.position, crouchStandLerpTime);
		controller.height = Mathf.Lerp(playerStandHeight, playerCrouchHeight, crouchStandLerpTime);
		controller.center = Vector3.Lerp(new Vector3(0, playerStandHeight / 2 + groundOffset, 0), new Vector3(0, playerCrouchHeight / 2 + groundOffset, 0), crouchStandLerpTime);
		SetDetectionCollider();

		if (isCrouching && crouchStandLerpTime >= 1)
		{
			playerCam.transform.position = crouchCamPos.position;

			ResetHeadBobTime();
			headBob = (bool)headRefPoint; //Check if Player should resume to Bob head

			controller.height = playerCrouchHeight;
			controller.center = new Vector3(0, playerCrouchHeight / 2 + groundOffset, 0);
			//distFromGround = (controller.height / 2) + groundOffset; //Using Property so do not have to set everytime
			action -= LerpCrouchStand;
		}
		else if (!isCrouching && crouchStandLerpTime <= 0)
		{
			playerCam.transform.position = standCamPos.position;

			ResetHeadBobTime();
			headBob = (bool)headRefPoint; //Check if Player should resume to Bob head

			controller.height = playerStandHeight;
			controller.center = new Vector3(0, playerStandHeight / 2 + groundOffset, 0);
			//distFromGround = (controller.height / 2) + groundOffset; //Using Property so do not have to set everytime
			action -= LerpCrouchStand;
		}
	}

	void LerpHeadBob()
	{
		if (!headBob) return;

		headBobLerpTime += Time.deltaTime;
		currentHeadBobOffset = new Vector3(0, MathFunctions.SmoothPingPong(headBobLerpTime, maxHeadBobOffset, bobSpeed), 0);
		currentViewingCamera.transform.position = headRefPoint.position + currentHeadBobOffset;
	}

	void GroundAndSlopeCheck()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + controller.center, -Vector3.up, out hit, DistFromGround, groundLayer))
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

	//Do not want Controller.Move to be manipulated directly by other Scripts
	public void StopPlayerMovementImmediately()
	{
		velocity = new Vector3 (0, -9.81f * Time.deltaTime, 0);
	}

	void Aim()
	{
		if (Physics.Raycast(currentViewingCamera.transform.position, currentViewingCamera.transform.forward, out aimRayHit, Mathf.Infinity, aimingRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			if (aimRayHit.collider == null) return;
			if (prevCollider == aimRayHit.collider) return;
			else prevCollider = aimRayHit.collider;

			//The | is needed if the Layermask Stores multiple layers
			if (hackableInteractableLayer == (hackableInteractableLayer | 1 << aimRayHit.transform.gameObject.layer))
			{
				focus = true;

				switch (aimRayHit.collider.tag)
				{
					case ("Hackable"):
						focusedHackable = aimRayHit.collider.GetComponent<IHackable>();
						focusedInteractable = null;
						break;
					case ("Interactable"):
						focusedInteractable = aimRayHit.collider.GetComponent<IInteractable>();
						focusedHackable = null;
						break;
				}
			}
			else
			{
				focusedHackable = null;
				focusedInteractable = null;
				focus = false;
			}

			ui.AimFeedback(focus);
		}
	}

	void Interact()
	{
		if (!focusedInteractable || (aimRayHit.point - currentViewingCamera.transform.position).sqrMagnitude > 9) return;

		if (focusedInteractable.allowPlayerInteraction) focusedInteractable.Interact();

		#region Old Interact
		/*void Interact ()
		{
			if (Input.GetKeyDown (KeyCode.E))
			{
				RaycastHit hit;
				Physics.Raycast (playerCam.transform.position, playerCam.transform.forward , out hit, 3);

				if (hit.collider != null)
				{
					switch (hit.collider.tag)
					{
						case ("ControlPanel"):
						{
							hit.collider.GetComponent<ControlPanel> ().Activate ();
						}
						break;

						case ("ServerPanel"):
						{
							FindObjectOfType<ExitDoor> ().locked = false;
						}
						break;

						case ("ExitDoor"):
						{
							if (!hit.collider.GetComponent<ExitDoor> ().locked)
							hit.collider.GetComponent<ExitDoor> ().OpenDoor ();
						}
						break;
					}
				}
			}
		}*/
		#endregion
	}

	void Hack()
	{
		if (!focusedHackable) return;

		ui.StartHacking();
		if (hackedObj) hackedObj.OnUnhack();
		isHacking = true;
		hackedObj = focusedHackable;
		focusedHackable = null;
		hackedObj.OnHack();

		#region Old Hacking
		/*RaycastHit hit;
		Debug.DrawLine(currentViewingCamera.transform.position, currentViewingCamera.transform.position + currentViewingCamera.transform.forward * 100, Color.green, 5);
		if (Physics.Raycast(currentViewingCamera.transform.position, currentViewingCamera.transform.forward, out hit, Mathf.Infinity, hackingRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			if (hit.collider != null) Debug.Log(hit.collider.name + " is hit");

			if (hackableLayer == (hackableLayer | 1 << hit.transform.gameObject.layer)) //The | is needed if the Layermask Stores multiple layers
			{
				IHackable hackable = hit.transform.GetComponent<IHackable>();

				if (!hackable) return;
				else
				{
					ui.StartHacking();
					if (hackedObj) hackedObj.OnUnhack();
					isHacking = true;
					hackedObj = hackable;
					hackedObj.OnHack();
				}
			}
		}*/
		#endregion
	}

	public void Unhack()
	{
		if (!hackedObj) return;
		currentViewingCamera = playerCam;
		hackedObj.OnUnhack();
		hackedObj = null;
		isHacking = false;
	}

	public void ChangeViewCamera(Camera camera, Transform headRefPoint = null)
	{
		currentViewingCamera.depth = 0;
		ResetHeadBob(headRefPoint);
		currentViewingCamera = camera;
		currentViewingCamera.depth = 2;
		MinimapCamera.inst.ChangeTarget (camera.transform);
	}

	//Specific For Head Bobbing
	void ResetHeadBob(Transform newHeadRefPoint = null)
	{
		ResetHeadBobTime();
		if (headRefPoint) currentViewingCamera.transform.position = headRefPoint.position; //Reseting Camera Position

		//Overrides into new Head Reference Point
		headRefPoint = newHeadRefPoint;
		headBob = (bool)headRefPoint;
		print("Called and Head Bob is " + headBob + ". Time stamp is " + Time.time);
	}

	void ResetHeadBobTime()
	{
		headBobLerpTime = 0;
		currentHeadBobOffset = Vector3.zero;
	}

	public void SetBobSpeedAndOffset(float speed = 1, float maxOffset = 0.15f)
	{
		bobSpeed = speed;
		maxHeadBobOffset = maxOffset;
	}

	void SetDetectionCollider()
	{
		detectionColl.center = controller.center;
		detectionColl.height = controller.height;
		detectionColl.radius = controller.radius;
	}

	//For Getting Private Components
	public Camera GetPlayerCamera()
	{
		return playerCam;
	}

	public Collider GetPlayerCollider()
	{
		return detectionColl;
	}

	public Transform GetHeadRefTransform()
	{
		return isCrouching ? crouchCamPos : standCamPos;
	}
}