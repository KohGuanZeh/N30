using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingAI : MonoBehaviour
{
	[System.Serializable]
	public struct PatrolPoint
	{
		public Transform point;
		public bool randomiseIdle;
		public bool alwaysIdle;

		[HideInInspector] 
		public Collider col;
	}
	
	public bool patrol = true;
	public bool idleLookAround = true;
	public bool hacked = false;
	public bool disable = false;

	[HideInInspector] public bool idleRotation = false;
	[HideInInspector] public bool reachedIdle = false;
	[HideInInspector] public bool firstIdle = true;
	
	public PatrolPoint[] patrolPoints;
	public Vector3 alarmPos;

	public int currentIndex;
	public bool registered = false;
	public bool alarmed = false;	
	public bool sentBack = false;

	bool chasingPlayer = false;
	[HideInInspector] public bool findingPlayer = false;
	bool reachedLastSeen = false;
	public bool isInvincible = false;
	public bool canChase = true;

	public float minStealthPercent = 0.1f; //0.00 - 1.00
	public GameObject chaseCheckpoint;
	GameObject storedCheckpoint;
	
	[HideInInspector] public NavMeshAgent agent;

	PlayerController player;
	AI ai;

	[HideInInspector] public bool invokedDoorChaseCancel;

	void Start ()
	{
		agent = GetComponent<NavMeshAgent> ();
		ai = GetComponent<AI> ();
		player = PlayerController.inst;

		currentIndex = 0;
		registered = false;
		alarmed = false;
		sentBack = false;
		disable = false;
		chasingPlayer = false;
		findingPlayer = false;
		reachedLastSeen = false;
		isInvincible = false;
		idleRotation = false;
		reachedIdle = false;
		firstIdle = true;
		invokedDoorChaseCancel = false;

		for (int i = 0; i < patrolPoints.Length; i++)
			patrolPoints[i].col = patrolPoints[i].point.GetComponent<Collider> ();
	}

	void Update () 
	{
		if (!sentBack)
			if (!ai.isDisabled && !isInvincible)
				if (!alarmed)
					ReRoute ();
				else
					agent.SetDestination (alarmPos);
		
		if (canChase)
			PlayerChase ();

		Invincibility ();

		if (idleLookAround && !idleRotation && !patrol && reachedIdle)
			StartCoroutine (IdleLookAround (firstIdle));
	}

	void Invincibility ()
	{
		if (//player.GetPlayerCollider ().IsVisibleFrom (ai.camera) && 
			(player.detectionGauge / player.detectionThreshold) >= minStealthPercent || findingPlayer || 
			(player.GetPlayerCollider ().IsVisibleFrom (ai.camera) && ai.whiteDot.activeSelf)) //&&
			//!canChase)
			isInvincible = true;
		else
			isInvincible = false;

		ai.hackable = !isInvincible;
		ai.canWipeMemory = !isInvincible;
	}

	void ChaseAlarm ()
	{
		isInvincible = false;
		agent.SetDestination (alarmPos);
	}

	void PlayerChase ()
	{
		if (player.GetPlayerCollider ().IsVisibleFrom (ai.camera) && 
			(player.detectionGauge / player.detectionThreshold) >= minStealthPercent)
			StartPlayerChase ();
		
		DuringPlayerChase ();
	}

	void StartPlayerChase ()
	{
		if (!invokedDoorChaseCancel)
		{
			agent.SetDestination (player.transform.position);
			chasingPlayer = true;
			findingPlayer = true;
			isInvincible = true;
			reachedLastSeen = false;
			StopAllCoroutines ();
			if (storedCheckpoint != null)
				Destroy (storedCheckpoint);
			storedCheckpoint = Instantiate (chaseCheckpoint, player.transform.position, Quaternion.identity);
		}
	}

	void DuringPlayerChase ()
	{
		if (reachedLastSeen)
		{
			reachedLastSeen = false;
			StartCoroutine ("LookAround");
		}
	}

	IEnumerator IdleLookAround (bool firstTime, bool patrolIdle = false)
	{
		idleRotation = true;

		if (firstIdle) 
		{
			for (int i = 0; i < 45; i++)
			{
				transform.RotateAround (transform.position, Vector3.up, -45 * Time.deltaTime);
				yield return null;
			}
		}
		else
		{
			for (int i = 0; i < 90; i++)
			{
				transform.RotateAround (transform.position, Vector3.up, -45 * Time.deltaTime);
				yield return null;
			}
		}

		firstIdle = false;

		yield return new WaitForSeconds (3);

		for (int i = 0; i < 90; i++)
		{
			transform.RotateAround (transform.position, Vector3.up, 45 * Time.deltaTime);
			yield return null;
		}

		yield return new WaitForSeconds (3);

		idleRotation = false;

		if (patrolIdle)
		{
			IdleEnd ();
		}
	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		if (patrolPoints.Length > 1)
		{
			for (int i = 1; i < patrolPoints.Length + 1; i++)
			{
				if (i < patrolPoints.Length)
					Gizmos.DrawLine (patrolPoints[i].point.position, patrolPoints[i - 1].point.position);
				else
					Gizmos.DrawLine (patrolPoints[patrolPoints.Length - 1].point.position, patrolPoints[0].point.position);
			}
		}
		else if (patrolPoints.Length > 0)
		{
			Gizmos.DrawWireCube (patrolPoints[0].point.position, Vector3.one);
		}
	}

	IEnumerator LookAround ()
	{
		//Vector3 startRotation = transform.eulerAngles;
		//Vector3 leftRotation = transform.eulerAngles + new Vector3 (0, 45, 0);
		//Vector3 rightRotation = transform.eulerAngles - new Vector3 (0, 45, 0);

		for (int i = 0; i < 45; i++)
		{
			transform.RotateAround (transform.position, Vector3.up, -45 * Time.deltaTime);
			yield return null;
		}
		
		yield return new WaitForSeconds (1);

		for (int i = 0; i < 90; i++)
		{
			transform.RotateAround (transform.position, Vector3.up, 45 * Time.deltaTime);
			yield return null;
		}

		yield return new WaitForSeconds (1);

		for (int i = 0; i < 45; i++)
		{
			transform.RotateAround (transform.position, Vector3.up, -45 * Time.deltaTime);
			yield return null;
		}

		yield return new WaitForSeconds (3);

		EndChase ();
	}

	void EndChase ()
	{
		if (alarmed)
			ChaseAlarm ();
		else
			ReRoute ();
	}
	
	public void ReRoute () 
	{
		if (storedCheckpoint != null)
			Destroy (storedCheckpoint);

		sentBack = true;
		findingPlayer = false;
		registered = false;
		isInvincible = false;

		if (patrol)
		{
			//ai.anim.SetFloat("Speed", 1);
			Transform nearestPatrolPoint = patrolPoints[0].point;
			currentIndex = 0;
			for (int i = 1; i < patrolPoints.Length; i++)
				if ((patrolPoints[i].point.position - transform.position).magnitude < (nearestPatrolPoint.position - transform.position).magnitude) 
				{
					currentIndex = i;
					nearestPatrolPoint = patrolPoints[i].point;
				}
			agent.SetDestination (nearestPatrolPoint.position);
		}
		else
		{
			agent.SetDestination (patrolPoints[0].point.position);
		}
	}

	bool RandomBool ()
	{
		return Random.Range (0, 2) == 1 ? true : false;
	}

	void TwoSecIdle ()
	{
		agent.SetDestination (transform.position);
		Invoke ("EndTwoSecIdle", 2);
	}
	
	void EndTwoSecIdle ()
	{
		EndChase ();
		chasingPlayer = false;
		findingPlayer = false;
		reachedLastSeen = false;
		invokedDoorChaseCancel = false;
	}

	void Idle ()
	{
		bool passed = false;
		if (patrolPoints[currentIndex].randomiseIdle)
		{
			if (RandomBool ())
				passed = true;
		} 
		else
		{
			passed = patrolPoints[currentIndex].alwaysIdle;
		}

		if (passed)
		{
			agent.isStopped = true;
			StartCoroutine (IdleLookAround (true, true));
		}
	}

	void IdleEnd ()
	{
		agent.isStopped = false;
	}

	void OnTriggerStay (Collider other) 
	{
		if (other.tag == "PatrolPoint" && !hacked && !registered && !alarmed && !ai.isDisabled && !chasingPlayer)
		{
			registered = true;

			if (patrol && other == patrolPoints[currentIndex].col) 
			{
				Idle ();

				currentIndex++;	
				if (currentIndex >= patrolPoints.Length)
					currentIndex = 0;

				agent.SetDestination (patrolPoints[currentIndex].point.position);
			}
			else if (!patrol && other == patrolPoints[0].col)
			{
				agent.isStopped = true;
				agent.velocity = Vector3.zero;
				reachedIdle = true;
				transform.eulerAngles = patrolPoints[0].point.eulerAngles;
			}
		}

		if (other.tag == "ChaseCheckpoint" && !ai.isDisabled && chasingPlayer && other.GetComponent<Collider> () == storedCheckpoint.GetComponent<Collider> ())
		{
			reachedLastSeen = true;
			chasingPlayer = false;
			Destroy (storedCheckpoint);
		}

		if (other.tag == "Door")
		{
			if (!invokedDoorChaseCancel && chasingPlayer)
			{
				if (other.GetComponent<AIDoor> ().requiredColor != ai.color && !other.GetComponent<AIDoor> ().nowForeverOpened)
				{
					invokedDoorChaseCancel = true;
					Destroy (storedCheckpoint);
					TwoSecIdle ();
				}
			}
			
			if (!invokedDoorChaseCancel && alarmed && other.GetComponent<AIDoor> ().requiredColor != ai.color)
			{
				invokedDoorChaseCancel = true;
				alarmed = false;
				sentBack = false;
				TwoSecIdle ();
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "PatrolPoint" && !hacked)
			registered = false;	
	}
}