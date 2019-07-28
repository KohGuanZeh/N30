using UnityEngine;

public class AI : IHackable
{
	PatrollingAI ai;

	[SerializeField] CharacterController controller;
	[SerializeField] Camera playerCam;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	[SerializeField] Vector3 velocity; //Player Velocity
	public float walkSpeed = 10; //Different Move Speed for Different Movement Action
	public float groundOffset = 0.02f;

	public bool onSlope;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public LayerMask slopeLayer;
	public float distFromGround;

	protected override void Start ()
	{
		base.Start ();
		controller = GetComponent<CharacterController>();
		ai = GetComponent<PatrollingAI> ();
		controller.enabled = false;
		ai.enabled = true;
	}

	public override void OnHack ()
	{
		base.OnHack ();
		ai.enabled = false;
		controller.enabled = true;
		ai.hacked = true;
	}

	public override void OnUnhack ()
	{
		base.OnUnhack ();
		ai.enabled = true;
		controller.enabled = false;
		ai.hacked = false;
		ai.ReRoute ();
	}	

    protected override void ExecuteHackingFunctionaliy ()
	{
		PlayerRotation ();
		PlayerMovement ();
		SlopeCheck ();
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

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		Vector3 horVelocity = (xMovement + zMovement).normalized * walkSpeed;
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
}