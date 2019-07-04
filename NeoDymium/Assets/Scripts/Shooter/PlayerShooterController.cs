using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerShooterController : MonoBehaviour
{
	[Header("Player Movement")]
	[SerializeField] Rigidbody rb;
	[SerializeField] Camera playerCam;
	public float walkSpeed = 10, runSpeed = 20;
	public float horLookSpeed = 1, vertLookSpeed = 1;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation

	[Header("For Gun and Shooting")]
	public Transform gun;
	public Transform shootPoint;
	public LayerMask expectedLayers;
	public float minSpread, maxSpread;
	public float effectiveRange;
	public bool inAimMode;

	[Header("Adjustments for Aim Mode. For Design Use")]
	public float normFov;
	public float aimFov;
	public float normCamPos;
	public float aimCamPos;

	//For Camera, Particularly Aiming
	bool cameraLerping;
	Action cameraLerp;
	float cameraLerpStatus;

    // Start is called before the first frame update
    void Start()
    {
		//Lock Cursor in Middle of Screen
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = true;

		//Getting Components
		playerCam = GetComponentInChildren<Camera>();
		rb = GetComponent<Rigidbody>();

		//Set Camera
		playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, normCamPos);
		playerCam.fieldOfView = normFov;
    }

    // Update is called once per frame
    void Update()
    {
		PlayerMovement();
		Aim();
		if (Input.GetMouseButtonDown(0)) RaycastShoot();

		if (cameraLerp != null) cameraLerp();
    }

	void PlayerMovement()
	{
		//Camera and Player Rotation
		//Credits go to https://www.youtube.com/watch?v=lYIRm4QEqro
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		transform.eulerAngles = new Vector3(0, yaw, 0);
		playerCam.transform.localEulerAngles = new Vector3(pitch, 0, 0);

		//Player Movement
		//Default to Walk Speed if Aiming. If not aiming, check if Player is holding shift.
		float movementSpeed = inAimMode ? walkSpeed : Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

		Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
		Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
		rb.velocity = (xMovement + zMovement).normalized * movementSpeed;
	}

	void Aim()
	{
		if (Input.GetMouseButton(1))
		{
			inAimMode = true;
			if (!cameraLerping)
			{
				cameraLerping = true;
				cameraLerp += LerpCamera;
			}
		}
		else if (Input.GetMouseButtonUp(1))
		{
			inAimMode = false;
			if (!cameraLerping)
			{
				cameraLerping = true;
				cameraLerp += LerpCamera;
			}
		}
	}

	void LerpCamera()
	{
		cameraLerpStatus = inAimMode ? Mathf.Min (cameraLerpStatus + Time.deltaTime * 10, 1) : Mathf.Max(cameraLerpStatus - Time.deltaTime * 10, 0);

		playerCam.fieldOfView = Mathf.Lerp(normFov, aimFov, cameraLerpStatus);
		float playerCamZPos = Mathf.Lerp(normCamPos, aimCamPos, cameraLerpStatus);
		playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, playerCamZPos);

		if (inAimMode && cameraLerpStatus >= 0.995f)
		{
			playerCam.fieldOfView = aimFov;
			playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, aimCamPos);
			cameraLerp -= LerpCamera;
			cameraLerping = false;
		}
		else if (!inAimMode && cameraLerpStatus <= 0.005f)
		{
			playerCam.fieldOfView = normFov;
			playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, normCamPos);
			cameraLerp -= LerpCamera;
			cameraLerping = false;
		}
	}

	public void RaycastShoot()
	{
		Vector3 bulletDir = playerCam.transform.forward;
		Ray shootRay = inAimMode ? new Ray(playerCam.transform.position, playerCam.transform.forward) : new Ray(playerCam.transform.position, playerCam.transform.forward);
		RaycastHit hitInfo;

		if (Physics.Raycast(shootRay, out hitInfo, effectiveRange, expectedLayers))
		{
			print("Hit!");
			hitInfo.collider.gameObject.SetActive(false);
		}
	}

	public void ShootProjectile()
	{
		Vector3 bulletDir = playerCam.transform.forward;
		Ray shootRay = new Ray(playerCam.transform.position, playerCam.transform.forward);
		RaycastHit hitInfo;

		if (Physics.Raycast(shootRay, out hitInfo, effectiveRange, expectedLayers))
		{
			print("Hit!");
			hitInfo.collider.gameObject.SetActive(false);
		}
	}
}
