using System.Collections.Generic;
using UnityEngine;

public class ShootableLight : MonoBehaviour
{
	public float rechargeDuration = 20;

	List<SimpleEnemy> enemies;
	Light lamp;

	void Start () 
	{
		lamp = GetComponent<Light> ();
		lamp.enabled = true;
		enemies = new List<SimpleEnemy> ();
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

    void OnCollisionEnter (Collision other) 
	{
		if (other.gameObject.tag == "PlayerProj" && lamp.enabled) //proj tag here
		{
			foreach (SimpleEnemy enemy in enemies)
				enemy.Blind ();
			lamp.enabled = false;
			Invoke ("EnableLamp", rechargeDuration);
		}
	}

	void EnableLamp () 
	{
		lamp.enabled = true;
	}
}