using System.Collections;
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

	[HideInInspector] public bool idleRotation = false;
	[HideInInspector] public bool reachedIdle = false;
	[HideInInspector] public bool firstIdle = true;
	
	public PatrolPoint[] patrolPoints;
	public Vector3 alarmPos;

	public int currentIndex;
	public bool registered = false;
	public bool alarmed = false;	
	public bool sentBack = false;

	public bool isInvincible = false;

	public float minStealthPercent = 0.2f; //0.00 - 1.00

	[HideInInspector] public bool moveAcrossNavMeshesStarted = false;
	
	[HideInInspector] public NavMeshAgent agent;

	PlayerController player;
	AI ai;
	UIManager ui;

	[HideInInspector] public bool invokedDoorChaseCancel;

	public Transform head;
	Vector3 headStartingRotation;

	[HideInInspector] public bool spottingPlayer = false;
	[HideInInspector] public bool findingPlayer = false;

	public float headRotateSpeed = 45;
	public float headRotateInterval = 3;
	public float headRotateThreshold = 45;

	bool idleReRoute;

	void Start ()
	{
		agent = GetComponent<NavMeshAgent> ();
		ai = GetComponent<AI> ();
		player = PlayerController.inst;
		ui = UIManager.inst;

		//head = transform.GetChild (1).GetChild (2).GetChild (2).GetChild (0).GetChild (0).GetChild (1).GetChild (0);
		headStartingRotation = head.localEulerAngles;

		currentIndex = 0;
		registered = false;
		alarmed = false;
		sentBack = false;
		isInvincible = false;
		idleRotation = false;
		reachedIdle = false;
		spottingPlayer = false;
		findingPlayer = false;
		moveAcrossNavMeshesStarted = false;
		firstIdle = true;
		invokedDoorChaseCancel = false;
		idleReRoute = false;

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
		
		if (!ai.isDisabled)
		{
			if (reachedIdle)
			{
				Quaternion targetRotation = Quaternion.LookRotation (patrolPoints[0].point.forward, Vector3.up);
				transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, 10 * Time.deltaTime);
				//transform.eulerAngles = patrolPoints[0].point.eulerAngles;
			}
				

			if (!moveAcrossNavMeshesStarted)
				SpotPlayer ();

			Invincibility ();
			OffMeshLinkCheck ();

			if (idleLookAround && !idleRotation && !patrol && reachedIdle && !alarmed)
			{
				StopAllCoroutines ();
				//firstIdle = true;
				StartCoroutine (IdleLookAround ());
			}
		}
	}

	void Invincibility ()
	{
		if ((player.detectionGauge / player.detectionThreshold) >= minStealthPercent)
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

	void SpotPlayer ()
	{
		if ((player.detectionGauge / player.detectionThreshold) >= minStealthPercent && !invokedDoorChaseCancel && player.GetPlayerCollider ().IsVisibleFrom (ai.camera))
		{
			CancelInvoke ("EndTwoSecIdle");	
			StopAllCoroutines ();
			idleRotation = false;
			LookAtPlayer ();
		}
		else if (findingPlayer && !idleRotation)
		{
			StopAllCoroutines ();
			StartCoroutine (IdleLookAround ());
		}
	}

	void LookAtPlayer ()
	{
		agent.SetDestination (transform.position);
		spottingPlayer = true;
		findingPlayer = true;
		firstIdle = true;
		ai.camera.transform.rotation = Quaternion.LookRotation (player.transform.position - ai.camera.transform.position + Vector3.up * 0.75f);
		head.rotation = Quaternion.LookRotation (player.transform.position - head.position + Vector3.up * 0.75f);
	}

	public IEnumerator IdleLookAround ()
	{
		idleRotation = true;

		float rotatedAmt = 0;
		Vector3 expectedAngle = Vector3.zero;
		Vector3 expectedAngleCam = Vector3.zero;

		if (firstIdle) 
		{
			rotatedAmt = 0;
			expectedAngle = new Vector3 (0, head.eulerAngles.y - headRotateThreshold, 0);
			expectedAngleCam = new Vector3 (0, ai.camera.transform.eulerAngles.y - headRotateThreshold, 0);
			while (rotatedAmt < headRotateThreshold)
			{
				head.RotateAround (head.position, Vector3.up, -headRotateSpeed * Time.deltaTime);
				ai.camera.transform.RotateAround (ai.camera.transform.position, Vector3.up, -headRotateSpeed * Time.deltaTime);
				head.transform.localPosition = Vector3.zero;
				rotatedAmt += headRotateSpeed * Time.deltaTime;
				yield return null;
			}
			head.eulerAngles = expectedAngle;
			ai.camera.transform.eulerAngles = expectedAngleCam;
			head.transform.localPosition = Vector3.zero;
		}
		else
		{
			rotatedAmt = 0;
			expectedAngle = new Vector3 (0, head.eulerAngles.y - (headRotateThreshold * 2), 0);
			expectedAngleCam = new Vector3 (0, ai.camera.transform.eulerAngles.y - (headRotateThreshold * 2), 0);
			while (rotatedAmt < headRotateThreshold * 2)
			{
				head.RotateAround (head.position, Vector3.up, -headRotateSpeed * Time.deltaTime);
				ai.camera.transform.RotateAround (ai.camera.transform.position, Vector3.up, -headRotateSpeed * Time.deltaTime);
				head.transform.localPosition = Vector3.zero;
				rotatedAmt += headRotateSpeed * Time.deltaTime;
				yield return null;
			}
			head.eulerAngles = expectedAngle;
			ai.camera.transform.eulerAngles = expectedAngleCam;
			head.transform.localPosition = Vector3.zero;
		}

		firstIdle = false;

		yield return new WaitForSeconds (headRotateInterval);

		rotatedAmt = 0;
		expectedAngle = new Vector3 (0, head.eulerAngles.y + (headRotateThreshold * 2), 0);
		expectedAngleCam = new Vector3 (0, ai.camera.transform.eulerAngles.y + (headRotateThreshold * 2), 0);
		while (rotatedAmt < headRotateThreshold * 2)
		{
			head.RotateAround (head.position, Vector3.up, headRotateSpeed * Time.deltaTime);
			ai.camera.transform.RotateAround (ai.camera.transform.position, Vector3.up, headRotateSpeed * Time.deltaTime);
			head.transform.localPosition = Vector3.zero;
			rotatedAmt += headRotateSpeed * Time.deltaTime;
			yield return null;
		}

		head.eulerAngles = expectedAngle;
		ai.camera.transform.eulerAngles = expectedAngleCam;
		head.transform.localPosition = Vector3.zero;

		yield return new WaitForSeconds (headRotateInterval);

		idleRotation = false;

		if (patrol)
			IdleEnd ();
		if (findingPlayer)
			EndFinding ();
	}
	
	public void EndFinding ()
	{
		ResetHeadRotation ();
		firstIdle = true;
		findingPlayer = false;
		if (alarmed)
			ChaseAlarm ();
		else
			ReRoute ();
	}

	void OnDrawGizmos ()
	{
		if (ai != null && ui != null)
		{
			switch (ai.color)
			{
				case ColorIdentifier.red:
				{
					Gizmos.color = ui.redColor;
					break;
				}

				case ColorIdentifier.blue:
				{
					Gizmos.color = ui.blueColor;
					break;
				}

				case ColorIdentifier.yellow:
				{
					Gizmos.color = ui.yellowColor;
					break;
				}

				case ColorIdentifier.green:
				{
					Gizmos.color =  ui.greenColor;
					break;
				}

				default:
				{
					Gizmos.color = Color.white;
					break;
				}
			}
		}
		else
		{
			if (GetComponent<AI> ())
				ai = GetComponent<AI> ();
			if (FindObjectOfType<UIManager> ())
				ui = FindObjectOfType<UIManager> ();
		}
		
		if (patrolPoints.Length > 1)
		{
			for (int i = 1; i < patrolPoints.Length + 1; i++)
			{
				Gizmos.DrawWireCube (patrolPoints[i - 1].point.position, Vector3.one * 0.5f);
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

	public void ReRoute () 
	{
		sentBack = true;
		if (!idleReRoute)
			idleReRoute = true;
		else
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
		sentBack = false;
		EndFinding ();
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
			agent.SetDestination (transform.position);
			StartCoroutine (IdleLookAround ());
		}
	}

	void IdleEnd ()
	{
		agent.SetDestination (patrolPoints[currentIndex].point.position);
	}

	void OffMeshLinkCheck ()
	{
		if (agent.isOnOffMeshLink && !moveAcrossNavMeshesStarted)
		{
   			StartCoroutine (MoveAcrossNavMeshLink ());
   			moveAcrossNavMeshesStarted = true;
		}
	}

	public void ResetHeadRotation ()
	{
		head.localEulerAngles = headStartingRotation;
		ai.camera.transform.localEulerAngles = Vector3.zero;
	}

	IEnumerator MoveAcrossNavMeshLink ()
	{
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        //agent.updateRotation = false;
 
		bool onStart = true;

		if ((data.endPos - transform.position).sqrMagnitude > (data.startPos - transform.position).sqrMagnitude)
		{
			onStart = false;
		}

        Vector3 endPos = onStart ? agent.transform.position : data.endPos + Vector3.up * agent.baseOffset;
        Vector3 startPos = onStart ? data.endPos + Vector3.up * agent.baseOffset : agent.transform.position;
        float duration = (endPos - startPos).magnitude / agent.velocity.magnitude;
        float t = 0.0f;
        float tStep = 1.0f / duration;
        while (t < 1.0f)
		{
			Quaternion endRotation = Quaternion.LookRotation (endPos - startPos, Vector3.up);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, endRotation, 10);
            transform.position = Vector3.Lerp (startPos, endPos, t);
            agent.destination = transform.position;
            t += tStep * Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        //agent.updateRotation = true;
        agent.CompleteOffMeshLink ();
		EndFinding ();
        moveAcrossNavMeshesStarted = false;
	}

	void OnTriggerStay (Collider other) 
	{	
		if (other.tag == "PatrolPoint" && !hacked && !registered && !alarmed && !ai.isDisabled && !findingPlayer)
		{
			registered = true;

			if (patrol && other == patrolPoints[currentIndex].col) 
			{
				currentIndex++;	
				if (currentIndex >= patrolPoints.Length)
					currentIndex = 0;

				agent.SetDestination (patrolPoints[currentIndex].point.position);
				idleRotation = false;
				Idle ();
			}
			else if (!patrol && other == patrolPoints[0].col)
			{
				//agent.isStopped = true;
				agent.SetDestination (transform.position);
				agent.velocity = Vector3.zero;
				reachedIdle = true;
				idleRotation = false;
				//transform.eulerAngles = patrolPoints[0].point.eulerAngles;
			}
		}

		if (other.tag == "Door")
		{
			AIDoor tempDoor = other.GetComponent<AIDoor> ();
			if (!invokedDoorChaseCancel)
			{
				if ((tempDoor.requiredColor != ai.color && tempDoor.requiredColor != ColorIdentifier.none) && !tempDoor.nowForeverOpened)
				{
					invokedDoorChaseCancel = true;
					if (alarmed)
						alarmed = false;
					TwoSecIdle ();
				}
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "PatrolPoint" && !hacked)
			registered = false;	
		if (other.tag == "Door")
			invokedDoorChaseCancel = false;
		if (other.tag == "PatrolPoint" && other == patrolPoints[0].col)
			reachedIdle = false;
	}
}