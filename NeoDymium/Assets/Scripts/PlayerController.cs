using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
	//May want to use Character Controller instead of Rigidbody
	public static PlayerController inst;

	[Header("Player Renderer")]
	public Renderer playerRenderer;

	[Header("Player Movement")]
	[SerializeField] CharacterController controller;
	[SerializeField] Camera playerCam;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	[SerializeField] Vector3 velocity; //Player Velocity
	public float crouchSpeed = 5, walkSpeed = 10, runSpeed = 20; //Different Move Speed for Different Movement Action
	public float groundOffset = 0.02f;

	[Header ("For Crouching")]
	public bool isCrouching = false;
	[SerializeField] Transform standCamPos, crouchCamPos;
	[SerializeField] float playerStandHeight, playerCrouchHeight;

	[Header("For Hacking")]
	public bool isHacking = false;
	public IHackable hackedObj;
	public LayerMask hackableLayer;
	[SerializeField] Camera currentViewingCamera;

	[Header("Movement on Slope")]
	public bool onSlope;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public LayerMask slopeLayer;
	public float distFromGround
	{
		get { return (controller.height / 2) + groundOffset; } //playerCollider.bounds.extents.y + 0.2f; This is via collider}
	}  //Stores the Collider.Bounds.Extents.Y. (Extents is always half of the collider size). With Controller, it is CharacterController.Height/2

	[Header ("Others")]
	public bool paused;

	void Awake () 
	{
		inst = this;
		playerRenderer = GetComponentInChildren<Renderer>();
	}

    void Start()
    {
		//Lock Cursor in Middle of Screen
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;

		//Getting Components
		playerCam = GetComponentInChildren<Camera>();
		controller = GetComponent<CharacterController>();
		currentViewingCamera = playerCam;
		//Set Ground Check. May need to change the y
		//distFromGround = (controller.height/2) + groundOffset; //playerCollider.bounds.extents.y + 0.2f; This is via collider
	}

	void Update()
    {
		if (!paused)
		{
			if (!isHacking)
			{
				SlopeCheck();
				ToggleCrouch();
				PlayerRotation();
				PlayerMovement();
			}

			if (Input.GetMouseButtonDown(0)) Hack();
			if (Input.GetMouseButtonDown(1)) Unhack();
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

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		Vector3 horVelocity = (xMovement + zMovement).normalized * movementSpeed;
		velocity = new Vector3(horVelocity.x, velocity.y, horVelocity.z);

		//Applying Gravity before moving
		velocity.y = onSlope ? -slopeForce : -9.81f * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);
	}

	void ToggleCrouch()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			isCrouching = !isCrouching;
			if (isCrouching)
			{
				playerCam.transform.position = crouchCamPos.position;
				controller.height = playerCrouchHeight;
				controller.center = new Vector3(0, playerCrouchHeight / 2 + groundOffset, 0);
				//distFromGround = (controller.height / 2) + groundOffset; //Using Property so do not have to set everytime
			}
			else
			{
				playerCam.transform.position = standCamPos.position;
				controller.height = playerStandHeight;
				controller.center = new Vector3(0, playerStandHeight / 2 + groundOffset, 0);
				//distFromGround = (controller.height / 2) + groundOffset; //Using Property so do not have to set everytime
			}
		}
	}

	void SlopeCheck()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit, distFromGround, slopeLayer)) onSlope = true;
	}

	//Do not want Controller.Move to be manipulated directly by other Scripts
	public void StopPlayerMovementImmediately()
	{
		velocity = new Vector3 (0, -9.81f * Time.deltaTime, 0);
	}

	void Hack()
	{
		RaycastHit hit;
		if (Physics.Raycast(currentViewingCamera.transform.position, currentViewingCamera.transform.forward, out hit, Mathf.Infinity))
		{
			if (hackableLayer == (hackableLayer | 1 << hit.transform.gameObject.layer)) //The | is needed if the Layermask Stores multiple layers
			{
				IHackable hackable = hit.transform.GetComponent<IHackable>();

				if (!hackable) return;
				else
				{
					if (hackedObj) hackedObj.OnUnhack();
					isHacking = true;
					hackedObj = hackable;
					hackedObj.OnHack();
				}
			}
		}
	}
	void Unhack()
	{
		currentViewingCamera = playerCam;
		hackedObj.OnUnhack();
		hackedObj = null;
		isHacking = false;
	}

	public void ChangeViewCamera(Camera camera)
	{
		currentViewingCamera.depth = 0;
		currentViewingCamera = camera;
		currentViewingCamera.depth = 2;
	}

	public Camera GetPlayerCamera()
	{
		return playerCam;
	}
}