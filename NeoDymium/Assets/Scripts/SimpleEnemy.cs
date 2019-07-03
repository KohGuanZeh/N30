using System.Collections;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
	public float shootCooldown = 1;
	public GameObject bullet;
	public float bulletSpeed = 2;
	[Space (10)]
	public int health = 100;
	public float invulnTime = 0.5f;
	[Space (10)]
	[Header ("Wandering Behaviour")]
	public float rotationTime = 0.7f;
	public float rotationDelay = 0.5f;
	public float walkTime = 0.7f;
	public float walkDelay = 0.5f;
	public float walkSpeed = 2;
	public float rotatingSpeed = 100;

	bool invuln;

	bool walking;
	bool rotating;
	bool wandering;
	bool rotateClockwise;

	Transform bulletSpawnPos;

    SimpleEnemyRing innerRing;
	SimpleEnemyRing outerRing;
	Player player;
	Rigidbody rb;

	void Start () 
	{
		player = Player.inst;

		innerRing = transform.GetChild (0).GetComponent<SimpleEnemyRing> ();
		outerRing = transform.GetChild (1).GetComponent<SimpleEnemyRing> ();
		bulletSpawnPos = transform.GetChild (2);
		rb = GetComponent<Rigidbody> ();

		walking = false;
		rotating = false;
		wandering = false;

		invuln = false;
	}

	void Update () 
	{
		if (outerRing.inside) 
		{
			if (wandering) 
			{
				StopCoroutine ("Wander");
				walking = false;
				rotating = false;
				wandering = false;
			}

			if (!IsInvoking ("Shoot"))
				InvokeRepeating ("Shoot", 0, shootCooldown);
			
			transform.rotation = Quaternion.LookRotation (player.transform.position - transform.position, transform.up);
			transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
		}
		else 
		{
			if (!wandering)
				StartCoroutine ("Wander");
			CancelInvoke ("Shoot");

			if (walking) 
				rb.velocity = transform.forward * walkSpeed;
			else
				rb.velocity = Vector3.zero;

			if (rotating) 
			{
				if (rotateClockwise)
					transform.eulerAngles += new Vector3 (0, rotatingSpeed * Time.deltaTime, 0);
				else
					transform.eulerAngles -= new Vector3 (0, rotatingSpeed * Time.deltaTime, 0);
			}
		}

		if (health < 0) 
			Destroy (gameObject);
	}

	void Shoot () 
	{
		bool shoot = Random.Range (1, 3) == 1 ? true : false;
		if (shoot) 
		{
			Rigidbody objRb = Instantiate (bullet, bulletSpawnPos.position, Quaternion.LookRotation (transform.forward, transform.up)).GetComponent<Rigidbody> ();
			objRb.velocity = transform.forward * bulletSpeed;
		}
	}

	void OnCollisionEnter (Collision other) 
	{
		if (other.gameObject.tag == "PlayerAttack" && !invuln) 
		{
			health -= player.CalculateDamage ();
			invuln = true;
			Invoke ("RefreshInvuln", invulnTime);
		}
	}

	void RefreshInvuln () 
	{
		invuln = false;
	}

	IEnumerator Wander ()
	{
		wandering = true;

		float rotTime = Random.Range (0.50f, rotationTime + 1);
		float rotDelay = Random.Range (0.50f, rotationDelay + 1);
		float wlkTime = Random.Range (0.50f, walkTime + 1);
		float wlkDelay = Random.Range (0.50f, walkDelay + 1);
		
		rotateClockwise = Random.Range (1, 3) == 1 ? true : false;

		walking = true;
		yield return new WaitForSeconds (wlkDelay);
		walking = false;
		yield return new WaitForSeconds (wlkDelay);
		rotating = true;
		yield return new WaitForSeconds (rotTime);
		rotating = false;
		yield return new WaitForSeconds (rotDelay);
		
		wandering = false;
	}
}