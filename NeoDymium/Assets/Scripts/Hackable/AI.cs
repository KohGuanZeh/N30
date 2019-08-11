using UnityEngine;

public class AI : IHackable
{
	PatrollingAI ai;

	[SerializeField] CharacterController controller;
	[SerializeField] Transform camPos;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	[SerializeField] Vector3 velocity; //Player Velocity
	public float walkSpeed = 10; //Different Move Speed for Different Movement Action
	public float groundOffset = 0.02f;

	public bool onSlope;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public LayerMask slopeLayer;
	public float distFromGround;

	public Animator anim;

	protected override void Start ()
	{
		base.Start ();
		controller = GetComponent<CharacterController>();
		ai = GetComponent<PatrollingAI> ();
		anim = GetComponentInChildren<Animator>();
		distFromGround = GetComponentInChildren<Renderer>().bounds.extents.y + 0.02f ;
		controller.enabled = false;
		ai.enabled = true;
	}

	public override void OnHack ()
	{
		base.OnHack ();
		yaw = transform.eulerAngles.y;
		ai.agent.enabled = false;
		ai.hacked = true;
		ai.enabled = false;
		controller.enabled = true;
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

	/*public override void Disable ()
	{
		if (ai.manager)
			gameObject.layer = 9;
		else
		{
			ai.agent.enabled = false;
			ai.enabled = false;
			base.Disable ();
		}	
	}*/

	protected override void ExecuteHackingFunctionaliy ()
	{
		PlayerRotation ();
		PlayerMovement ();
		SlopeCheck ();
		//Interact ();
	}

	/*void Interact () 
	{
		if (Input.GetKeyDown (key: KeyCode.E))
		{
			RaycastHit hit;
			Physics.Raycast (camera.transform.position, camera.transform.forward, out hit, 3);

			if (hit.collider != null) 
				if (hit.collider.tag == "Interactable")
					hit.collider.GetComponent<IInteractable> ().TryInteract (color);
		}
	}*/

	void PlayerRotation()
	{
		//Camera and Player Rotation
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		transform.eulerAngles = new Vector3(0, yaw, 0);
		camera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
	}

	void PlayerMovement()
	{
		//if (controller.isGrounded)
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

		controller.Move(velocity * Time.deltaTime);
	}

	void SlopeCheck()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit, distFromGround, slopeLayer)) onSlope = true;
	}

	public override void EnableDisableHackable(bool isEnable, ColorIdentifier controlPanelColor)
	{
		base.EnableDisableHackable(isEnable, controlPanelColor);
		anim.SetBool("Disabled", isDisabled);
	}
}