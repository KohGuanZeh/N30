using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType { ShooterCam, ThirdPersonCam };

public class PlayerController : MonoBehaviour
{
	[Header("Other Properties")]
	[SerializeField] bool inShooterCamera = false;

	[Header("Player and Camera Movement")]
	public static PlayerController inst;
	[SerializeField] Rigidbody rb;
	[SerializeField] Camera playerCam;
	[SerializeField] float walkSpeed = 10;
	[SerializeField] float runSpeed = 20;
	[SerializeField] float horLookSpeed = 1, vertLookSpeed = 1;
	float yaw, pitch;

	[Header("Utility Arm")]
	public Transform closestTarget; //Cursor will auto Aim at the Closest Target
	public UtilityArm utilityArm;
	public Transform armAttachPoint;

	private void Awake()
	{
		inst = this;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();

		//Camera Set up
		playerCam = GetComponentInChildren<Camera>();
		playerCam.transform.parent.SetParent(null);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.C)) SwitchCameraMode();

		CameraMovement();
		PlayerMovement();

		if (inShooterCamera && utilityArm.attached && Input.GetMouseButtonDown(0)) Shoot();
	}

	void SwitchCameraMode()
	{
		inShooterCamera = !inShooterCamera;
		if (inShooterCamera) playerCam.transform.parent.SetParent(transform);
		else playerCam.transform.parent.SetParent(null);
	}

	void PlayerMovement()
	{
		float movementSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

		if (inShooterCamera)
		{
			Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
			Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
			rb.velocity = (xMovement + zMovement).normalized * movementSpeed;
		}
		else
		{
			Vector3 xMovement = Input.GetAxisRaw("Horizontal") * playerCam.transform.right;
			xMovement = new Vector3(xMovement.x, 0, xMovement.z);

			Vector3 zMovement = Input.GetAxisRaw("Vertical") * playerCam.transform.forward;
			zMovement = new Vector3(zMovement.x, 0, zMovement.z);

			Vector3 desiredDir = (xMovement + zMovement).normalized;

			if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.99f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.99f)
			{
				transform.rotation = Quaternion.LookRotation(desiredDir, transform.up);
				rb.velocity = transform.forward * movementSpeed;
			}
			else rb.velocity = new Vector3 (0, rb.velocity.y, 0);

			#region WASD Movement
			/*float playerLookRot = 0;
			int movement = 0;

			//Find out how to optimise this
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
			{
				if (Input.GetKey(KeyCode.W))
				{
					movement++;
					playerLookRot += playerCam.transform.eulerAngles.y;
					playerLookRot /= movement;
					transform.eulerAngles = new Vector3(0, playerLookRot, 0);
					rb.velocity = transform.forward * movementSpeed;
				}
				else if (Input.GetKey(KeyCode.S))
				{
					movement++;
					playerLookRot += playerCam.transform.eulerAngles.y - 180;
					playerLookRot /= movement;
					transform.eulerAngles = new Vector3(0, playerLookRot, 0);
					rb.velocity = transform.forward * movementSpeed;
				}

				if (Input.GetKey(KeyCode.A))
				{
					movement++;
					playerLookRot += playerCam.transform.eulerAngles.y - 90;
					playerLookRot /= movement;
					transform.eulerAngles = new Vector3(0, playerLookRot, 0);
					rb.velocity = transform.forward * movementSpeed;
				}
				else if (Input.GetKey(KeyCode.D))
				{
					movement++;
					playerLookRot += playerCam.transform.eulerAngles.y + 90;
					playerLookRot /= movement;
					transform.eulerAngles = new Vector3(0, playerLookRot, 0);
					rb.velocity = transform.forward * movementSpeed;
				}
			}
			else rb.velocity = Vector3.zero;*/
			#endregion
		}
	}

	void CameraMovement()
	{
		//Credits go to https://www.youtube.com/watch?v=lYIRm4QEqro
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		if (inShooterCamera)
		{
			transform.eulerAngles = new Vector3(0, yaw, 0);
			playerCam.transform.parent.localEulerAngles = new Vector3(pitch, 0, 0);
		}
		else
		{
			playerCam.transform.parent.position = transform.position;
			playerCam.transform.parent.localEulerAngles = new Vector3(pitch, yaw, 0);
		}
	}

	void Shoot()
	{
		print("Shoot");

		if (closestTarget == null) utilityArm.targetPoint = playerCam.transform.forward * utilityArm.shootRange;
		else utilityArm.targetPoint = closestTarget.position;

		utilityArm.attached = false;
	}
}