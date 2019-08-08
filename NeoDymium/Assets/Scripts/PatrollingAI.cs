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
	}
	
	public bool patrol = true;
	public bool hacked = false;
	public bool disable = false;
	public float idleDuration = 1;
	
	public PatrolPoint[] patrolPoints;

	public int currentIndex;
	public bool registered = false;
	public bool alarmed = false;
	public bool sentBack = false;
	
	[HideInInspector] public NavMeshAgent agent;
	List<Collider> colliders;
	AI ai;

	void Start ()
	{
		colliders = new List<Collider> ();
		agent = GetComponent<NavMeshAgent> ();
		ai = GetComponent<AI> ();

		currentIndex = 0;
		registered = false;
		alarmed = false;
		sentBack = false;
		disable = false;

		foreach (PatrolPoint point in patrolPoints)
			colliders.Add (point.point.GetComponent<Collider> ());
	}

	void Update () 
	{
		if (!alarmed && !sentBack && !ai.isDisabled)
			ReRoute ();
	}
	
	public void ReRoute () 
	{
		sentBack = true;
		if (patrol)
		{
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
			Invoke ("IdleEnd", idleDuration);
		}
	}

	void IdleEnd ()
	{
		agent.isStopped = false;
	}

	void OnTriggerStay (Collider other) 
	{
		if (other.tag == "PatrolPoint" && !hacked && !registered && !alarmed && !ai.isDisabled)
		{
			registered = true;

			if (patrol && colliders.Contains (other)) 
			{
				Idle ();
				if (currentIndex + 1 >= patrolPoints.Length)
					currentIndex = 0;
				else 
					currentIndex++;
				agent.SetDestination (patrolPoints[currentIndex].point.position);
			}
			else if (!patrol)
			{
				agent.SetDestination (transform.position);
				transform.eulerAngles = patrolPoints[0].point.eulerAngles;
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "PatrolPoint" && !hacked)
			registered = false;	
	}
}