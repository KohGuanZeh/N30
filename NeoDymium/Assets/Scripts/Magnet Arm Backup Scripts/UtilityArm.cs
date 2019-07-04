using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmState { attached, shooting, detached, returning };

public class UtilityArm : MonoBehaviour
{
	[Header("General Properties")]
	[SerializeField] PlayerController player;

	[Header("Arm Properties")]
	public Rigidbody rb;
	public bool attached = true;
	public bool isReturning;
	public float shootSpeed = 10;
	public float shootRange = 100;

	[Header("For Shooting and Returning")]
	public LayerMask interactableLayers;
	public Vector3 targetPoint; //May want to Change to Transform
	public bool reachedLimit;

	// Start is called before the first frame update
	void Start()
	{
		player = PlayerController.inst;
		attached = true;
		player.utilityArm = this;

		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!attached)
		{
			Vector3 dir = isReturning ? (player.armAttachPoint.position - transform.position).normalized : (transform.position - targetPoint).normalized;
			rb.velocity = dir * shootSpeed;

			if (Vector3.Distance(transform.position, targetPoint) < 0.02f)
			{
				transform.position = targetPoint;

				if (!isReturning) isReturning = true;
				else
				{
					attached = true;
					isReturning = false;
				}
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//If the Collided Object is in the expected Layer assigned by the Arm and the Arm is not yet returning, latch the arm to the Object
		if (!attached && !isReturning && interactableLayers == (interactableLayers | (1 << collision.gameObject.layer)))
		{
			print("Yeet");
			isReturning = true;
		}
	}
}