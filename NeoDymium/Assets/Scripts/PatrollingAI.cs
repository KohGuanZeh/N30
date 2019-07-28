using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingAI : MonoBehaviour
{
	public bool manager = false;
	public bool patrol = true;
	public bool hacked = false;
	public Transform[] patrolPoints;

	public int currentIndex;
	
	[HideInInspector] public NavMeshAgent agent;
	List<Collider> colliders;

	void Start ()
	{
		colliders = new List<Collider> ();
		agent = GetComponent<NavMeshAgent> ();

		currentIndex = 0;
		
		ReRoute ();

		if (manager)
			AIWall.inst.IgnoreManager (GetComponent<Collider> ());
		else
			AIWall.inst.IgnoreAI (GetComponent<Collider> ());

		foreach (Transform trans in patrolPoints)
			colliders.Add (trans.GetComponent<Collider> ());
	}

	public void ReRoute () 
	{
		if (patrol)
		{
			Transform nearestPatrolPoint = patrolPoints[0];
			for (int i = 0; i < patrolPoints.Length; i++)
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

	void OnTriggerEnter (Collider other) 
	{
		if (other.tag == "PatrolPoint" && !hacked)
		{
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
}