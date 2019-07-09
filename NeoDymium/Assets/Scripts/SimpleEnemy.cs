using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
	public float attackCooldown = 1;
	public int health = 100;
	public int damage = 10;
	public float blindDuration = 3;

	bool blind;
	bool canHit;

	PlayerShooterController player;
	NavMeshAgent agent;

	void Start () 
	{
		canHit = true;
		blind = false;

		player = PlayerShooterController.inst;
		agent = GetComponent<NavMeshAgent> ();
	}

	void Update () 
	{
		if (player != null && !blind)
			agent.SetDestination (player.transform.position);
		if (health <= 0)
			Destroy (gameObject);
	}

	void OnCollisionStay (Collision other) 
	{
		if (other.gameObject.tag == "Player" && canHit) 
		{
			canHit = false;
			player.currentHealth -= damage;
			Invoke ("RefreshHit", attackCooldown);
		}
	}

	void RefreshHit () 
	{
		canHit = true;
	}

	public void Blind () 
	{
		blind = true;
		Invoke ("RefreshBlind", blindDuration);
	}

	void RefreshBlind () 
	{
		blind = false;
	}
}