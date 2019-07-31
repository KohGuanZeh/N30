using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingAI : MonoBehaviour
{
	public bool manager = false;
	public bool patrol = true;
	public bool hacked = false;
	public bool disable = false;
	
	public Transform[] patrolPoints;

	public int currentIndex;
	public bool registered = false;
	public bool alarmed = false;
	public bool sentBack = false;
	
	[HideInInspector] public NavMeshAgent agent;
	List<Collider> colliders;

	void Start ()
	{
		colliders = new List<Collider> ();
		agent = GetComponent<NavMeshAgent> ();

		currentIndex = 0;
		registered = false;
		alarmed = false;
		sentBack = false;
		disable = false;

		foreach (Transform trans in patrolPoints)
			colliders.Add (trans.GetComponent<Collider> ());
	}

	void Update () 
	{
		if (!alarmed && !sentBack)
			ReRoute ();
	}
	
	public void ReRoute () 
	{
		sentBack = true;
		if (patrol)
		{
			Transform nearestPatrolPoint = patrolPoints[0];
			currentIndex = 0;
			for (int i = 1; i < patrolPoints.Length; i++)
				if ((patrolPoints[i].position - transform.position).magnitude < (nearestPatrolPoint.position - transform.position).magnitude) 
				{
					currentIndex = i;
					nearestPatrolPoint = patrolPoints[i];
				}
			agent.SetDestination (nearestPatrolPoint.position);
		}
		else
		{
			agent.SetDestination (patrolPoints[0].position);
		}
	}

	void OnTriggerStay (Collider other) 
	{
		if (other.tag == "PatrolPoint" && !hacked && !registered && !alarmed)
		{
			registered = true;

			if (patrol && colliders.Contains (other)) 
			{
				if (currentIndex + 1 >= patrolPoints.Length)
					currentIndex = 0;
				else 
					currentIndex++;
				agent.SetDestination (patrolPoints[currentIndex].position);
			}
			else if (!patrol)
			{
				agent.SetDestination (transform.position);
				transform.eulerAngles = patrolPoints[0].eulerAngles;
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.tag == "PatrolPoint" && !hacked)
			registered = false;	
	}
}