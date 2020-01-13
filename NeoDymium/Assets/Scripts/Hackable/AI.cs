using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class AI : IHackable
{
	PatrollingAI ai;

	//movement and rotation
	public float walkSpeed = 10;
	public float groundOffset = 0.02f;

	CharacterController controller;
	[SerializeField] Transform hackCamPos, unhackCamPos;
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
	AudioSource walkAudiosource;
	AudioSource rotateAudiosource;

	Vector3 lastEuler;

	public Light lightCone;
	public DensityVolume densityVolume;

	bool secondWalk = false;

	protected override void Start ()
	{
		controller = GetComponent<CharacterController>();
		ai = GetComponent<PatrollingAI>();
		anim = GetComponentInChildren<Animator>();
		walkAudiosource = GetComponents<AudioSource> ()[0];
		rotateAudiosource = GetComponents<AudioSource> ()[1];
		distFromGround = GetComponentInChildren<Renderer>().bounds.extents.y + 0.02f;

		soundManager = SoundManager.inst;

		walkAudiosource.loop = false;
		walkAudiosource.playOnAwake = false;

		rotateAudiosource.loop = false;
		rotateAudiosource.playOnAwake = false;

		// audioSource.volume = soundManager.aiWalk.volume;
		// audioSource.pitch = soundManager.aiWalk.pitch;
		controller.enabled = false;
		ai.enabled = true;
		hackableType = HackableType.AI;

		base.Start ();

		//lightCone = transform.GetChild (1).GetChild (20).GetChild (1).GetChild (0).GetComponentInChildren<Light> ();
		//densityVolume = transform.GetChild (1).GetChild (20).GetChild (1).GetChild (0).GetComponentInChildren<DensityVolume> ();
		densityVolume.enabled = false;

		lightCone.color = GetHackableMaterialColor (color);

		camera.transform.position = unhackCamPos.position;
	}

	protected override void Update ()
	{
		base.Update ();
		GroundAndSlopeCheck ();
		if (!hacked)
		{
			if (ai.agent.velocity.magnitude > 0.1f || ai.moveAcrossNavMeshesStarted)
				anim.SetBool ("Moving", true);
			else
				anim.SetBool ("Moving", false);
			UnhackedSound ();
			AdjustIntensity ();
		}
		else
		{
			HackedSound ();
			if (controller.velocity.magnitude > 0.1f)
				anim.SetBool ("Moving", true);
			else
				anim.SetBool ("Moving", false);
		}
	}

	void AdjustIntensity ()
	{
		RaycastHit hit;
		if (Physics.Raycast (lightCone.transform.position, lightCone.transform.forward, out hit, Mathf.Infinity, player.aimingRaycastLayers))
		{
			float distance = (lightCone.transform.position - hit.point).magnitude;
			densityVolume.transform.localPosition = new Vector3 (0, (-distance / 2), 1);
			densityVolume.parameters.size = new Vector3 (distance, 5, distance);
			lightCone.intensity = distance * 100;
		}
		else
		{
			densityVolume.parameters.size = new Vector3 (3.75f, 5, 7.5f);
			lightCone.intensity = 2000;
		}
	}

	void UnhackedSound ()
	{
		if (ai.agent.velocity.sqrMagnitude > 0 && !walkAudiosource.isPlaying)
		{
			if (secondWalk)
			{
				walkAudiosource.clip = soundManager.unHackedAiWalk.clip;
			}
			else
			{
				walkAudiosource.clip = soundManager.unHackedAiWalk2.clip;
			}
			walkAudiosource.Play ();
		}
		
		if (camera.transform.eulerAngles != lastEuler && !rotateAudiosource.isPlaying)
		{
			rotateAudiosource.clip = soundManager.unHackedAiLookAround.clip;
			rotateAudiosource.Play ();
		}
		else if (camera.transform.eulerAngles == lastEuler)
		{
			rotateAudiosource.Stop ();
		}

		lastEuler = camera.transform.eulerAngles;

		// if (ai.agent.velocity.sqrMagnitude == 0)
		// 	audioSource.Stop ();
	}

	void HackedSound ()
	{
		if (controller.velocity.sqrMagnitude > 0 && isGrounded && !walkAudiosource.isPlaying)
		{
			if (secondWalk)
			{
				walkAudiosource.clip = soundManager.hackedAiWalk.clip;
			}
			else
			{
				walkAudiosource.clip = soundManager.hackedAiWalk2.clip;
			}
			walkAudiosource.Play ();
		}
		
		if (camera.transform.eulerAngles != lastEuler && !rotateAudiosource.isPlaying)
		{
			rotateAudiosource.clip = soundManager.hackedAiLookAround.clip;
			rotateAudiosource.Play ();
		}
		else if (camera.transform.eulerAngles == lastEuler)
		{
			rotateAudiosource.Stop ();
		}

		lastEuler = camera.transform.eulerAngles;
	}

	public override void OnHack ()
	{
		base.OnHack ();
		camera.transform.position = hackCamPos.position;

		ai.EndFinding ();

		//Reset Camera Rotations
		yaw = transform.eulerAngles.y;

		ai.agent.enabled = false;
		ai.hacked = true;
		ai.enabled = false;
		controller.enabled = true;
		ai.StopAllCoroutines ();
		walkAudiosource.Stop ();
		rotateAudiosource.Stop ();
		lightCone.enabled = false;
		//ai.gameObject.AddComponent<Rigidbody> ();
		//ai.GetComponent<Rigidbody> ().useGravity = false;
	}

	public override void OnUnhack ()
	{
		base.OnUnhack ();
		camera.transform.position = unhackCamPos.position;

		//Reset Cam Rotation on Unhack
		pitch = 0;
		camera.transform.localEulerAngles = new Vector3(pitch, 0, 0);

		print(camera.transform.localEulerAngles);

		ai.enabled = true;
		ai.agent.enabled = true;
		controller.enabled = false;
		ai.hacked = false;
		ai.registered = false;
		ai.sentBack = false;
		ai.firstIdle = true;
		ai.reachedIdle = false;
		ai.idleRotation = false;
		ai.moveAcrossNavMeshesStarted = false;
		ai.invokedDoorChaseCancel = false;
		ai.StopAllCoroutines ();
		walkAudiosource.Stop ();
		rotateAudiosource.Stop ();
		lightCone.enabled = true;

		ai.findingPlayer = false;
		ai.spottingPlayer = false;

		ai.EndFinding ();
		//Destroy (ai.GetComponent<Rigidbody> ());
	}

	public override Transform GetCameraRefPoint()
	{
		return hackCamPos;
	}

	protected override void ExecuteHackingFunctionaliy ()
	{
		if (!lockRotation) PlayerRotation ();
		if (!lockRotation) PlayerMovement ();
		ui.ShiftAIArrows(pitch);
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
		// if ((velocity.x != 0 || velocity.z != 0) && isGrounded && !soundManager.IsSourcePlaying (soundManager.unHackedAiWalk.sourceIndex))
		// {
		// 	soundManager.PlaySound (soundManager.unHackedAiWalk);
		// }
			

		// if (velocity.x == 0 || velocity.z == 0)
		// 	soundManager.StopSound (soundManager.aiWalk.sourceIndex);
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
			ai.agent.enabled = false;
			ai.enabled = false;
			controller.enabled = false;
			ai.sentBack = true;

			lightCone.enabled = false;
			densityVolume.enabled = false;
		}
	}
}
