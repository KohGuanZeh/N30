using UnityEngine;
using System;

public class PlayerShooterController : MonoBehaviour
{
<<<<<<< HEAD
	[Header("Player Properties")]
	public int currentHealth;
	public int maxHealth;
=======
	public static PlayerShooterController inst;
>>>>>>> c7c17753dff003b24e91089af447bfd20b52cf03

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
<<<<<<< HEAD
	public float spreadVal;
	public float effectiveRange;
=======
	public float minSpread, maxSpread;
	public float effectiveRange = 50;
>>>>>>> c7c17753dff003b24e91089af447bfd20b52cf03
	public bool inAimMode;
	public int gunDamage = 10;

	[Header("Adjustments for Aim Mode. For Design Use")]
	public float normFov = 60;
	public float aimFov = 30;
	public float normCamPos = -0.25f;
	public float aimCamPos = 0.1f;

	[Header ("Health")]
	public int maxHealth = 100;
	public int currentHealth;

	//For Camera, Particularly Aiming
	bool cameraLerping;
	Action cameraLerp;
	float cameraLerpStatus;

	void Awake () 
	{
		inst = this;
	}

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

		currentHealth = maxHealth;
    }

    void Update()
    {
		PlayerMovement();
		Aim();
		if (Input.GetMouseButtonDown(0)) RaycastShoot();

		if (cameraLerp != null) cameraLerp();
		if (currentHealth <= 0)
			Destroy (gameObject);
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
		Vector3 spread = new Vector3(UnityEngine.Random.Range(-spreadVal,spreadVal), UnityEngine.Random.Range(-spreadVal, spreadVal), 0);
		Ray shootRay = inAimMode ? new Ray(playerCam.transform.position, playerCam.transform.forward) : new Ray(playerCam.transform.position + spread, playerCam.transform.forward);
		RaycastHit hitInfo;

		if (Physics.Raycast(shootRay, out hitInfo, effectiveRange, expectedLayers))
		{
			print("Hit!");
			switch (hitInfo.collider.gameObject.tag) 
			{
				case ("Enemy"): 
				{
					hitInfo.collider.GetComponent<SimpleEnemy> ().health -= gunDamage;
				}
				break;

				case ("ExplodingBarrel"): 
				{
					hitInfo.collider.GetComponent<ExplodingBarrel> ().hitsLeft--;
				}
				break;
			}
			//hitInfo.collider.gameObject.SetActive(false);
		}
	}

	//Not Emphasied on yet
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