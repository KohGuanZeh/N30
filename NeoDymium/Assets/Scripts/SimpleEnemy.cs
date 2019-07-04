using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
	public float attackCooldown = 1;
	public int health = 100;

	PlayerController player;
	NavMeshAgent agent;

	void Start () 
	{
		player = PlayerController.inst;
		agent = GetComponent<NavMeshAgent> ();
	}

	void Update () 
	{
		agent.SetDestination (player.transform.position);
		if (health < 0)
			Destroy (gameObject);
	}

	void OnCollisionStay (Collision other) 
	{
		if (other.gameObject.tag == "Player") 
			{}//player.currentHealth -= 10;
	}
}