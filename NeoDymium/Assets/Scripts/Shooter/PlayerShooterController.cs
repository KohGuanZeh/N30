using UnityEngine;
using System;

public class PlayerShooterController : MonoBehaviour
{
	//May want to use Character Controller instead of Rigidbody
	public static PlayerShooterController inst;

	[Header("Player Properties")]
	public int maxHealth = 100;
	public int currentHealth;

	[Header("Player Movement")]
	[SerializeField] CharacterController controller;
	[SerializeField] Camera playerCam;
	[SerializeField] Vector3 velocity;
	[SerializeField] float yaw, pitch; //Determines Camera and Player Rotation
	public bool lockMovement, lockRotation; //To Lock Player Movement and Rotation if needed, for things like Jump Pad
	public float walkSpeed = 10, runSpeed = 20, jumpSpeed = 10;
	[SerializeField] float slopeForce; //For now manually inputting a value to clamp the Player down. Look for Terry to come up with a fix
	public float horLookSpeed = 1, vertLookSpeed = 1;
	public float distFromGround; //Stores the Collider.Bounds.Extents.Y. (Extents is always half of the collider size). With Controller, it is CharacterController.Height/2
	public bool isGrounded, onSlope;
	public bool onIce;
	public LayerMask groundLayer;
	public float knockbackTimer;
	//public Vector3 knockbackVel; //Save the Original Knockback Velocity so that it can be lerped. //Only needed if jump allow player to move in air but not knockback

	[Header("For Gravity Testing")] //Required since there is no gravity scale
	[SerializeField] Vector3 groundNormal;
	[SerializeField] bool testGravity;
	[SerializeField] float originalGravity = -9.81f;
	[SerializeField] float currentGravity;
	[SerializeField] float gravityScale;

	[Header("For Gun and Shooting")]
	public IGun currentGun;
	public IGun[] gunInventory;
	public Transform shootPoint;
	public LayerMask shootLayers;
	public float spreadVal;
	public bool inAimMode;
	public bool autoReload = true;//,canRapidFire = false, raycastShoot = false;

	[Header("Grenade")]
	public Transform grenadePos;
	public GameObject grenade;
    public float throwSpeed = 50;
	public float explosionRadius = 5;
	public float explosionTime = 3;

	[Header("Adjustments for Aim Mode. For Design Use")]
	public float normFov = 60;
	public float aimFov = 30;
	public float normCamPos = -0.25f;
	public float aimCamPos = 0.1f;

	[Header ("Others")]
	public bool paused;

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
		Cursor.visible = false;

		//Getting Components
		playerCam = GetComponentInChildren<Camera>();
		controller = GetComponent<CharacterController>();

		//Set Camera
		playerCam.transform.localPosition = new Vector3(playerCam.transform.localPosition.x, playerCam.transform.localPosition.y, normCamPos);
		playerCam.fieldOfView = normFov;

		//Set Ground Check. May need to change the y
		currentGravity = originalGravity * gravityScale;
		distFromGround = (controller.height/2) + 0.2f; //playerCollider.bounds.extents.y + 0.2f; This is via collider

		//Set Player Properties
		currentHealth = maxHealth;

		//Need to instantiate the Guns first if not it will directly affect prefab
		IGun[] inventoryGuns = new IGun[gunInventory.Length];
		for (int i = 0; i < gunInventory.Length; i++)
		{
			IGun newGun = Instantiate(gunInventory[i], transform.position, Quaternion.identity);
			newGun.transform.parent = transform;
			inventoryGuns[i] = newGun;
		}
		gunInventory = inventoryGuns;
		currentGun = gunInventory[0];
		currentGun.InitialiseGun(this, playerCam, shootPoint, shootLayers);
    }

    void Update()
    {
		if (!paused)
		{
			//Temporary Solution to adjust and Test Gravity
			if (testGravity)
			{
				Physics.gravity = new Vector3(0, originalGravity * gravityScale, 0);
				currentGravity = -9.81f * gravityScale;
			}

			//if (Input.GetKeyDown(KeyCode.P)) canRapidFire = !canRapidFire;
			//if (Input.GetKeyDown(KeyCode.O)) raycastShoot = !raycastShoot;

			GroundCheck();
			if (!lockRotation) PlayerRotation();
			if (!lockMovement) PlayerMovement();

			knockbackTimer -= Time.deltaTime;

			Aim();
			SelectGun(); //Needs to be instantiated... If not it will edit the Prefab
			Grenade();

			if (currentGun.isRapidFire && Input.GetMouseButton(0)) currentGun.Shoot(); //Rapid Fire Shoot
			else if (Input.GetMouseButtonDown(0)) currentGun.Shoot(); //No Rapid Fire Shoot

			#region Old Firing
			/*if (canRapidFire && Input.GetMouseButton(0))
			{
				currentGun.Shoot();
				if (currentGun.shootsProjectiles) ShootProjectile();
				else RaycastShoot();
				if (raycastShoot) RaycastShoot();
				else ShootProjectile();
			}
			else if (Input.GetMouseButtonDown(0))
			{
				currentGun.Shoot();
				if (currentGun.shootsProjectiles) ShootProjectile();
				else RaycastShoot();
				if (raycastShoot) RaycastShoot();
				else ShootProjectile();
			}*/
			#endregion

			if (cameraLerp != null) cameraLerp();
			if (currentHealth <= 0)
				Destroy(gameObject);
		}
	}

	void PlayerRotation()
	{
		//Camera and Player Rotation
		//Separated Since Jump Pad can allow Player to look around but not move
		//Credits go to https://www.youtube.com/watch?v=lYIRm4QEqro
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
		if (knockbackTimer <= 0)
		{
			float movementSpeed = inAimMode ? walkSpeed : Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

			Vector3 xMovement = Input.GetAxisRaw("Horizontal") * transform.right;
			Vector3 zMovement = Input.GetAxisRaw("Vertical") * transform.forward;
			Vector3 horVelocity = (xMovement + zMovement).normalized * movementSpeed;

			if (isGrounded)
			{
				//Need to Lerp to 0
				if (onIce && horVelocity.sqrMagnitude == 0)
				{
					//velocity = Vector3.Lerp(velocity, new Vector3(0, velocity.y, 0), 0.25f * Time.deltaTime);
					velocity -= new Vector3(velocity.x * Time.deltaTime * 0.5f, 0, velocity.z * Time.deltaTime * 0.5f);
				}
				else velocity = new Vector3(horVelocity.x, velocity.y, horVelocity.z);
			}
		}

		//Applying Gravity before moving
		velocity.y = isGrounded ? onSlope ? -slopeForce : currentGravity * Time.deltaTime : velocity.y + currentGravity * Time.deltaTime;

		//If u want player to be knocked up in the air when shooting the ground
		//if (knockbackTimer <= 0) velocity.y = isGrounded ? onSlope ? -slopeForce : currentGravity * Time.deltaTime : velocity.y + currentGravity * Time.deltaTime;
		//else velocity.y = velocity.y + currentGravity * Time.deltaTime; 

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded && knockbackTimer <= 0) velocity.y = jumpSpeed;
		controller.Move(velocity * Time.deltaTime);

		#region Rigidbody Method
		/*if (isGrounded && Input.GetKeyDown(KeyCode.Space)) //Jump.
		{
			rb.useGravity = true;
			rb.velocity = new Vector3(horVelocity.x, jumpSpeed, horVelocity.z);
		}
		else if (isGrounded && horVelocity.sqrMagnitude == 0) //If no keys are being input and Player is on the ground, no gravity acts on Player. Required so that Player does not slide down slope
		{
			rb.useGravity = false;
			rb.velocity = Vector3.zero;
		}
		else
		{
			rb.useGravity = true;
			//rb.velocity = new Vector3(horVelocity.x, rb.velocity.y, horVelocity.z);

			//Below will solve the Slope Issue by adding a Ground Clamp for the Rigidbody... But feels like a Duct Tape Method
			rb.velocity = isOnSlope ? new Vector3(horVelocity.x, -10, horVelocity.z) : new Vector3(horVelocity.x, rb.velocity.y, horVelocity.z);
		}*/
		#endregion
	}

	public void GroundCheck()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit, distFromGround, groundLayer))
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

	public void PlayerKnockback(Vector3 direction, float force, float knockBackTime = 0.5f)
	{
		velocity = direction * force;
		//knockbackVel = velocity;
		knockbackTimer = knockBackTime;
	}

	//Do not want Controller.Move to be manipulated directly by other Scripts
	public void StopPlayerMovementImmediately()
	{
		velocity = new Vector3 (0, currentGravity * Time.deltaTime, 0);
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

	void Grenade()
	{
		if (Input.GetKeyDown(key: KeyCode.G))
		{
			Rigidbody grenadeRb = Instantiate(grenade, grenadePos.position, Quaternion.identity).GetComponent<Rigidbody>();
			grenadeRb.velocity = playerCam.transform.forward * throwSpeed;
		}
	}

	void SelectGun()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) currentGun = gunInventory[0];
		else if (Input.GetKeyDown(KeyCode.Alpha2)) currentGun = gunInventory[1];
		else if (Input.GetKeyDown(KeyCode.Alpha3)) currentGun = gunInventory[2];
		else if (Input.GetKeyDown(KeyCode.Alpha4)) currentGun = gunInventory[3];
		else if (Input.GetKeyDown(KeyCode.Alpha5)) currentGun = gunInventory[4];

		if (!currentGun.gunInitialised) currentGun.InitialiseGun(this, playerCam, shootPoint, shootLayers);
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Low Gravity")
		{
			gravityScale = 1f;
			currentGravity = originalGravity * gravityScale;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Low Gravity")
		{
			gravityScale = 3f;
			currentGravity = originalGravity * gravityScale;
		}
	}

	#region Old Firing Functions
	/*public void RaycastShoot()
	{
		Vector3 bulletDir = playerCam.transform.forward;
		Vector3 spread = new Vector3(UnityEngine.Random.Range(-spreadVal,spreadVal), UnityEngine.Random.Range(-spreadVal, spreadVal), 0);
		Ray shootRay = inAimMode ? new Ray(playerCam.transform.position, playerCam.transform.forward) : new Ray(playerCam.transform.position + spread, playerCam.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(shootRay, out hit, currentGun.effectiveRange, shootLayers))
		{
			print("Hit!");
			switch (hit.collider.tag) 
			{
				case ("Enemy"): 
				{
					hit.collider.GetComponent<SimpleEnemy>().health -= currentGun.shootDmg;
				}
				break;

				case ("ExplodingBarrel"): 
				{
					hit.collider.GetComponent<ExplodingBarrel>().hitsLeft--;
				}
				break;
			}

			//hitInfo.collider.gameObject.SetActive(false);
		}
	}

	//Need instantiate bullet then feed in the values
	public void ShootProjectile()
	{
		Vector3 shootDir = playerCam.transform.forward;
		Ray shootRay = new Ray(playerCam.transform.position, playerCam.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(shootRay, out hit, currentGun.effectiveRange, shootLayers)) shootDir = (hit.point - shootPoint.position).normalized;
		else shootDir = shootRay.direction;

		Projectile bullet = Instantiate(currentGun.projectile, shootPoint.position, Quaternion.identity);
		bullet.rb.velocity = shootDir * bullet.projectileSpd;

		currentGun.ShootEffect();
	}*/
	#endregion
}