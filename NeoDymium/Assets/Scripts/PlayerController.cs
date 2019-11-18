using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : MonoBehaviour
{
	[Header("General Variables")]
	public static PlayerController inst;
	[SerializeField] CapsuleCollider detectionColl; //Capsule Collider that is same size as Char Controller Collider. Char Controller Collider cant have proper raycast on its hemisphere so need to use this
	public Collider controllerCol;
	[SerializeField] UIManager ui;
	[SerializeField] LoadingScreen loadingScreen;
	[SerializeField] AreaNamesManager areaNamesManager;
	[SerializeField] AreaNames areaNames;

	[Header("Player Movement")]
	[SerializeField] CharacterController controller;
	[SerializeField] Camera playerCam;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	[SerializeField] Vector3 velocity; //Player Velocity
	public float crouchSpeed = 5, walkSpeed = 10, runSpeed = 20; //Different Move Speed for Different Movement Action
	[SerializeField] bool isGrounded, onSlope;
	[SerializeField] public LayerMask groundLayer;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public float groundOffset = 0.125f;
	public float DistFromGround
	{
		get { return ((controller.height / 2) + groundOffset); } //playerCollider.bounds.extents.y + 0.2f; This is via collider}
	}

	[Header ("For Crouching")]
	public bool isCrouching = false;
	[SerializeField] Transform standCamPos, crouchCamPos;
	[SerializeField] float playerStandHeight, playerCrouchHeight;
	float crouchStandLerpTime;

	[Header("For Hacking and Interactions")]
	//[SerializeField] RenderTexture renderTexture; //May be used if current feedback fails
	[SerializeField] Camera currentViewingCamera;
	public Camera CurrentViewingCamera { get { return currentViewingCamera; } } //Public Property to Get Current Camera
	[SerializeField] Camera prevViewingCamera;
	[SerializeField] float hackingLerpTime;
	[SerializeField] bool isHacking = false; //Returns true if Player is undergoing Hacking Animation
	public bool isFocusing = false; //Check if Player is focusing on any Interactable or Hackable. //Passed to UI Manager to check what Info it should Display
	public bool inSpInteraction = false; //Checks if Player is in Special Interaction. Special Interaction prevents Players from Crouching, Moving and Rotating and Cursor will appear
	public bool inHackable = false; //Checks if the Player is in a Hackable Object
	public IHackable hackedObj;
	public LayerMask aimingRaycastLayers;
	public LayerMask hackableInteractableLayer;
	[SerializeField] RaycastHit aimRayHit;

	[Header("For Storing Previous Frame Hacking")]
	[SerializeField] Collider prevCollider;
	[SerializeField] IHackable detectedHackable;
	[SerializeField] IInteractable detectedInteractable;

	[Header("Stealth Gauge")]
	public bool isDetected;
	public float detectionGauge;
	public float detectionThreshold;
	public float prevDetectionGauge; //Keeps track of Previous Frame Stealth Gauge Value. If there is no change, it means that Player is no longer Detected
	public float increaseMult;
	public float decreaseMult;
	public Outline detectedOutline; //Player Outline

	[Header("Checkpoint System")]
	public bool overwriteCheckpoints;
	public int forcedCheckPointsPassed;
	public int checkPointsPassed;
	[SerializeField] Checkpoint[] checkPoints;

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
	public PostProcessProfile ppp;
	public bool holdingPass;
	public bool vipPass;
	Pass previousPass;
	private bool areaNameUpdated = false;
	SoundManager soundManager;
	bool playedSound;
	InstructionsManager instructManager;

	[Header("For Editor SS")]
	[SerializeField] bool disablePlayerMove;

	//For Getting Private Components. May want to use Properties instead
	#region Additional Functions To Get Private Vars
	public float GetYaw()
	{
		return yaw;
	}

	public float GetPitch()
	{
		return pitch;
	}

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

	public float GetPlayerHeight()
	{
		return controller.height;
	}
	#endregion

	void Awake ()
	{
		inst = this;
	}

    void Start()
    {
		if (!overwriteCheckpoints)
			checkPointsPassed = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + " Checkpoint");
		else
			checkPointsPassed = forcedCheckPointsPassed;

		if (checkPoints.Length > 0)
		{
			for (int i = 0; i < checkPointsPassed + 1; i++) checkPoints[i].GetHackableMemory(i);
			checkPoints[checkPointsPassed].LoadCheckPoint();
		}

		//Lock Cursor in Middle of Screen
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		//Getting Components
		ui = UIManager.inst;
		loadingScreen = LoadingScreen.inst;
		areaNamesManager = AreaNamesManager.inst;
		areaNames = AreaNames.inst;
		soundManager = SoundManager.inst;
		instructManager = InstructionsManager.inst;
		playerCam = GetComponentInChildren<Camera>();
		controller = GetComponent<CharacterController>();
		detectionColl = GetComponent<CapsuleCollider>();
		controllerCol = GetComponent<Collider> ();
		SetDetectionCollider();
		currentViewingCamera = playerCam;
		anim = GetComponentInChildren<Animator>();
		playedSound = false;

		detectedOutline = GetComponentInChildren<Outline>();
		detectedOutline.enabled = false;

		yaw = transform.eulerAngles.y;
		pitch = playerCam.transform.eulerAngles.x;
		headRefPoint = standCamPos;
		action += LerpHeadBob;
	}

	void Update()
    {
		//For Editor SS
		if (Input.GetKeyDown(KeyCode.F))
		{
			disablePlayerMove = !disablePlayerMove;
			Cursor.lockState = disablePlayerMove ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = disablePlayerMove;
		} 
		if (disablePlayerMove) return;

		if (!ui.isPaused && !ui.isGameOver)
		{
			if (loadingScreen.isLoading) return;

			//mesh.mesh.RecalculateBounds();
			if (!inSpInteraction)
			{
				if (!inHackable)
				{
					GroundAndSlopeCheck();
					if (!instructManager.lockCameraRotation) ToggleCrouch();
					if (!instructManager.lockCameraRotation) PlayerRotation();
					if (!instructManager.lockCameraRotation) PlayerMovement();
				}

				Aim();
				if (detectedHackable || detectedInteractable) UpdateDisplayMessages();
				if (Input.GetKeyDown(KeyCode.E))
				{
					if (detectedHackable) WipeMemory();
					if (detectedInteractable) Interact();
				}
				if (Input.GetMouseButtonDown(0) && !isHacking) Hack();
				if (Input.GetMouseButtonDown(1)) Unhack();
			}

			if (prevDetectionGauge == detectionGauge) DecreaseDetectionGauge();
			else prevDetectionGauge = detectionGauge;

			if (action != null) action();
		}
	}

	void DetectionSound ()
	{	
		if (isDetected && !playedSound)
		{
			soundManager.PlaySound (soundManager.playerDetected);
			playedSound = true;
		}	
		
		if (!isDetected)
		{
			playedSound = false;
		}
	}

	#region Player Movement
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
		float movementSpeed = isCrouching ? crouchSpeed : walkSpeed; //: Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

		if (movementSpeed < walkSpeed) SetBobSpeedAndOffset(1f, 0.03f);
		//else if (movementSpeed > walkSpeed) SetBobSpeedAndOffset(5f, 0.05f);
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

		//sound
		if (horVelocity.sqrMagnitude != 0 && isGrounded && !soundManager.IsSourcePlaying (soundManager.playerWalk.sourceIndex))
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
	}

	void GroundAndSlopeCheck()
	{
		RaycastHit hit;
		Vector3 origin = transform.position + controller.center;
		Debug.DrawLine(origin, origin + -Vector3.up * DistFromGround, Color.red);
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

	void ToggleCrouch()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			if (isCrouching && !StandAvailability()) return;
			isCrouching = !isCrouching;
			anim.SetBool("Crouch", isCrouching);
			action += LerpCrouchStand;
			soundManager.PlaySound (soundManager.playerCrouch);
			if (!inHackable) headRefPoint = isCrouching ? crouchCamPos : standCamPos;
			headBob = false;
		}
	}

	bool StandAvailability()
	{
		//Debug.DrawLine(transform.position, transform.position + Vector3.up * (playerStandHeight + 0.2f), Color.red, 5);
		//The Origin should have offset since the Transform.position shift abit due to the Character Controller Collider
		if (Physics.Raycast(transform.position + (Vector3.up * 0.05f), Vector3.up, playerStandHeight + 0.1f, groundLayer)) return false;
		else return true;
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

	void SetDetectionCollider()
	{
		detectionColl.center = controller.center;
		detectionColl.height = controller.height;
		detectionColl.radius = controller.radius;
	}

	//Do not want Controller.Move to be manipulated directly by other Scripts
	public void StopPlayerMovementImmediately()
	{
		velocity = new Vector3 (0, -9.81f * Time.deltaTime, 0);
	}
	#endregion

	#region Hacking and Interaction
	void Aim()
	{
		if (Physics.Raycast(currentViewingCamera.transform.position, currentViewingCamera.transform.forward, out aimRayHit, Mathf.Infinity, aimingRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			//Segment may not even be needed
			if (aimRayHit.collider == null)
			{
				detectedHackable = null;
				detectedInteractable = null;
				isFocusing = false;
				prevCollider = null;
				ui.Focus(isFocusing);
				return;
			}

			if (prevCollider == aimRayHit.collider) return;
			else prevCollider = aimRayHit.collider;

			//The | is needed if the Layermask Stores multiple layers
			if (hackableInteractableLayer == (hackableInteractableLayer | 1 << aimRayHit.transform.gameObject.layer))
			{
				isFocusing = true;

				switch (aimRayHit.collider.tag)
				{
					case ("Hackable"):
						detectedHackable = aimRayHit.collider.GetComponent<IHackable>();
						detectedInteractable = null;

						string hackError = detectedHackable.GetError(0);
						if (hackError != string.Empty) ui.DisplayInstructionsAndErrors(true, new bool[] {true, true}, detectedHackable.hasPlayerMemory && !inHackable);
						else
						{
							if (detectedHackable.hasPlayerMemory && !inHackable)
							{
								string wipeError = detectedHackable.GetError(1);
								if (wipeError != string.Empty) ui.DisplayInstructionsAndErrors(true, new bool[] { false, true }, true);
								else
								{
									if (WithinWipeDistance()) ui.DisplayInstructionsAndErrors(true, new bool[] { false, false }, true);
									else ui.DisplayInstructionsAndErrors(true, new bool[] { false, true }, true);
								}
							}
							else ui.DisplayInstructionsAndErrors(true, new bool[] { false, false }, false);
						}

						break;

					case ("Interactable"):

						detectedInteractable = aimRayHit.collider.GetComponent<IInteractable>();
						detectedHackable = null;

						string interactError = detectedInteractable.GetError();
						if (interactError != string.Empty) ui.DisplayInstructionsAndErrors(false, new bool[] { true, false }, false);
						else ui.DisplayInstructionsAndErrors(false, new bool[] { !WithinInteractDistance(), false }, false);

						break;
				}
			}
			else
			{
				detectedHackable = null;
				detectedInteractable = null;
				isFocusing = false;
			}

			//If Interactable or Hackable is being Focused on, Crosshair should focus
			ui.Focus(isFocusing);
		}
		else
		{
			detectedHackable = null;
			detectedInteractable = null;
			isFocusing = false;
			prevCollider = null;
			ui.Focus(isFocusing);
			return;
		}
	}


	void UpdateDisplayMessages()
	{
		if (detectedInteractable)
		{
			string interactError = detectedInteractable.GetError();
			if (interactError != string.Empty) ui.DisplayInstructionsAndErrors(false, new bool[] { true, false }, false);
			else ui.DisplayInstructionsAndErrors(false, new bool[] { !WithinInteractDistance(), false }, false);
		}
		else if (detectedHackable)
		{
			string hackError = detectedHackable.GetError(0);
			if (hackError != string.Empty) ui.DisplayInstructionsAndErrors(true, new bool[] { true, true }, detectedHackable.hasPlayerMemory && !inHackable);
			else
			{
				if (detectedHackable.hasPlayerMemory && !inHackable)
				{
					string wipeError = detectedHackable.GetError(1);
					if (wipeError != string.Empty) ui.DisplayInstructionsAndErrors(true, new bool[] { false, true }, true);
					else
					{
						if (WithinWipeDistance()) ui.DisplayInstructionsAndErrors(true, new bool[] { false, false }, true);
						else ui.DisplayInstructionsAndErrors(true, new bool[] { false, true }, true);
					}
				}
				else ui.DisplayInstructionsAndErrors(true, new bool[] { false, false }, false);
			}
		}
		else return;
	}

	bool WithinInteractDistance()
	{
		//Does not Check if Interactable is Null
		if ((aimRayHit.point - currentViewingCamera.transform.position).sqrMagnitude > 9) return false;
		else return true;
	}

	void Interact()
	{
		if (!detectedInteractable) return;

		string interactError = detectedInteractable.GetError();
		if (interactError == string.Empty) interactError = WithinWipeDistance() ? interactError : "Target is out of reach";
		ui.DisplayError(interactError); //May want to use Display Error to Return a Bool. If the Bool is true, return so you dont have to do additional Checks

		if (interactError != string.Empty) return; //Prevent Interaction so long an Error is being produced

		if (inHackable) detectedInteractable.TryInteract(hackedObj.color); //Not sure how to better structure this
		else if (detectedInteractable.allowPlayerInteraction) detectedInteractable.Interact();
	}

	bool WithinWipeDistance()
	{
		//Does not Check if Hackable is Null
		if ((aimRayHit.point - currentViewingCamera.transform.position).sqrMagnitude > 25) return false;
		else return true;
	}

	void WipeMemory()
	{
		if (!detectedHackable) return;
		if (!detectedHackable.hasPlayerMemory) return;

		string wipeError = detectedHackable.GetError(1);
		if (wipeError == string.Empty) wipeError = WithinWipeDistance() ? wipeError : "Target is out of reach";
		ui.DisplayError(wipeError); //May want to use Display Error to Return a Bool. If the Bool is true, return so you dont have to do additional Checks

		if (wipeError != string.Empty) return; //Prevent Wiping of Memory if Error is produced

		detectedHackable.hasPlayerMemory = false;
	}

	void Hack()
	{
		if (!detectedHackable) return;
		if (!detectedHackable || hackedObj == detectedHackable) return;

		string hackError = detectedHackable.GetError();
		ui.DisplayError(hackError);

		if (hackError != string.Empty) return; //Prevent Hacking if Error is produced

		if (!inHackable || hackedObj.hackableType != detectedHackable.hackableType) ui.SwitchUI(detectedHackable.hackableType);

		anim.SetFloat("Speed", 0);

		isHacking = true;
		inHackable = true;
		if (hackedObj) hackedObj.OnUnhack();
		hackedObj = detectedHackable;
		ResetHeadBob(hackedObj.GetCameraRefPoint()); //Need to somehow get Head Bobbing for AI
		detectedHackable = null;

		if (hackedObj.camera)
		{
			prevViewingCamera = currentViewingCamera;
			currentViewingCamera = hackedObj.camera;

			currentViewingCamera.rect = new Rect(Vector2.zero, new Vector2(1, 0));

			prevViewingCamera.depth = 1; //If Previous Viewing Camera is Player Camera, Change the Depth to 0 instead of 1
			currentViewingCamera.depth = 2;

			currentViewingCamera.enabled = true;

			hackingLerpTime = 0;
			ui.ResetInstructionsDisplayOnHack();
			ui.SetUIColors(hackedObj.color);
			ui.StartUILerp(false);
			action += HackUnhackAnimation; //If Hackable has Camera, do Animation with Camera
		}

		hackedObj.OnHack();

		//sound
		soundManager.PlaySound (soundManager.hack);
	}

	public void Unhack(bool forced = false) //Check if it is Forced Unhacking
	{
		if (!inHackable) return;

		ui.SwitchUI(HackableType.none);

		isHacking = true;
		inHackable = false;
		hackedObj.OnUnhack();
		hackedObj = null;
		detectedHackable = null;

		prevViewingCamera = currentViewingCamera;
		currentViewingCamera = playerCam;
		currentViewingCamera.depth = 0;

		soundManager.PlaySound (soundManager.unhack);

		if (forced) return; //If Forced Unhacking, immediately set Previous Viewing Camera to Null

		ResetHeadBob(GetHeadRefTransform()); //Need to somehow get Head Bobbing for AI
		currentViewingCamera.enabled = true;

		//hackingLerpTime = 1;
		ui.SetUIColors();
		ui.StartUILerp(false);
		action -= HackUnhackAnimation;
		action += HackUnhackAnimation; //If Hackable has Camera, do Animation with Camera
	}

	public void ForcedUnhackAnimEvent() //Change Camera View Once Static Screen Appears
	{
		ResetHeadBob(GetHeadRefTransform()); //Need to somehow get Head Bobbing for AI
		currentViewingCamera.enabled = true;

		prevViewingCamera.rect = new Rect(Vector2.zero, new Vector2(1, 0));
		prevViewingCamera.depth = -1;
		prevViewingCamera.enabled = false;
		prevViewingCamera = null;

		ui.SetUIColors();
		ui.StartUILerp(false);

		isHacking = false;
	}

	void HackUnhackAnimation()
	{
		hackingLerpTime =  inHackable ? Mathf.Min(hackingLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(hackingLerpTime - Time.deltaTime * 5, 0);
		//Since the Camera Size only Clamps from 0 to 1, it is essentailly the same as Hacking Lerp Time.
		//Therefore the Camera Viewport's size.y = hackingLerpTime
		Vector2 size = new Vector2(1, hackingLerpTime);
		Vector2 pos = new Vector2(0, 0.5f - (hackingLerpTime / 2));

		if (inHackable)
		{
			currentViewingCamera.rect = new Rect(pos, size);

			if (hackingLerpTime >= 1)
			{
				isHacking = false;

				size = Vector2.one;
				pos = Vector2.zero;
				currentViewingCamera.rect = new Rect(pos, size);

				prevViewingCamera.depth = -1; //Player Camera Depth will always be at 0
				prevViewingCamera.enabled = false;
				prevViewingCamera = null;

				ui.StartUILerp(true);
				action -= HackUnhackAnimation;
			}
		}
		else //If going back to Player Camera, tweak the Prev Viewing Camera instead
		{
			prevViewingCamera.rect = new Rect(pos, size);

			if (hackingLerpTime <= 0)
			{
				isHacking = false;

				size = new Vector2(1, 0);
				pos = Vector2.zero;
				prevViewingCamera.rect = new Rect(pos, size);

				prevViewingCamera.depth = -1; //Player Camera Depth will always be at 0
				prevViewingCamera.enabled = false;
				prevViewingCamera = null;

				action -= HackUnhackAnimation;
			}
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (!areaNameUpdated && other.tag == "AreaNames")
		{
			areaNamesManager.areaNameText.text = other.gameObject.GetComponent<AreaNames>().currentAreaName;
			areaNames.fadeNow = true;
			areaNameUpdated = true;
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "AreaNames")
		{
			areaNameUpdated = false;
		}
	}

	#region Old Hacking
	/*void Hack()
	{
		if (!detectedHackable || hackedObj == detectedHackable) return;
		if (detectedHackable.enabledShields.Count > 0) return;

		if (hackedObj) hackedObj.OnUnhack();
		inHackable = true;
		hackedObj = detectedHackable;
		detectedHackable = null;
		hackedObj.OnHack();*/

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
				inHackable = true;
				hackedObj = hackable;
				hackedObj.OnHack();
			}
		}
	}*/
	#endregion
	#endregion

	#region Player Detection
	public void IncreaseDetectionGauge()
	{
		isDetected = true;
		detectedOutline.enabled = true;
		detectionGauge = Mathf.Min(detectionGauge + Time.deltaTime * increaseMult, detectionThreshold);
		//if (detectionGauge >= detectionThreshold) ui.GameOver();
	}

	public void DecreaseDetectionGauge()
	{
		if (detectionGauge <= 0 || detectionGauge == detectionThreshold) return;
		detectedOutline.enabled = false;
		isDetected = false;
		detectionGauge = Mathf.Max(detectionGauge - Time.deltaTime * decreaseMult, 0);
		prevDetectionGauge = detectionGauge;
	}
	#endregion

	#region Head Bobbing
	void LerpHeadBob()
	{
		if (!headBob) return;

		headBobLerpTime += Time.deltaTime;
		currentHeadBobOffset = new Vector3(0, MathFunctions.SmoothPingPong(headBobLerpTime, maxHeadBobOffset, bobSpeed), 0);
		currentViewingCamera.transform.position = headRefPoint.position + currentHeadBobOffset;
	}

	void ResetHeadBob(Transform newHeadRefPoint = null)
	{
		ResetHeadBobTime();
		if (headRefPoint) currentViewingCamera.transform.position = headRefPoint.position; //Reseting Camera Position

		//Overrides into new Head Reference Point
		headRefPoint = newHeadRefPoint;
		headBob = (bool)headRefPoint;
		//print("Called and Head Bob is " + headBob + ". Time stamp is " + Time.time);
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
	#endregion
}
