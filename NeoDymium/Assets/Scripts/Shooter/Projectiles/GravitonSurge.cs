using System.Collections.Generic;
using UnityEngine;

public class GravitonSurge : IProjectile
{
	[Header ("Unique")]
	public float succAmplifier;
	List<SimpleEnemy> enemies;

	void Start ()
	{
		enemies = new List<SimpleEnemy> ();
	}

	public override void ProjectileEffect()
	{
		foreach (SimpleEnemy enemy in enemies)
		{
			Vector3 direction = (transform.position - enemy.transform.position).normalized;
			enemy.transform.position = Vector3.Lerp(enemy.transform.position, transform.position, succAmplifier * Time.deltaTime);
		}
	}

	void OnTriggerEnter (Collider other) 
	{
		if (other.tag == "Enemy" && !enemies.Contains (other.GetComponent<SimpleEnemy> ()))
			enemies.Add (other.GetComponent<SimpleEnemy> ());
	}

	void OnTriggerExit (Collider other) 
	{
		if (other.tag == "Enemy")
			enemies.Remove (other.GetComponent<SimpleEnemy> ());
	}

    protected override void OnCollisionEnter (Collision other) 
	{
		base.OnCollisionEnter (other);
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
	}
}