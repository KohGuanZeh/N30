using System.Collections.Generic;
using UnityEngine;

public class GravitonSurge : IProjectile
{
	[Header ("Unique")]
	public float succAmplifier;

	bool activated;
	List<SimpleEnemy> enemies;

	void Start ()
	{
		enemies = new List<SimpleEnemy> ();
		activated = false;
	}

	public override void Update () 
	{
		base.Update ();
		if (activated) 
			foreach (SimpleEnemy enemy in enemies)
			{
				Vector3 direction = (transform.position - enemy.transform.position).normalized;
				enemy.transform.position = Vector3.Lerp (enemy.transform.position, transform.position, succAmplifier * Time.deltaTime);
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

    public override void OnCollisionEnter (Collision other) 
	{
		base.OnCollisionEnter (other);
		activated = true;
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
	}
}