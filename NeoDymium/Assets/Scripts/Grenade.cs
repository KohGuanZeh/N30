using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	void Start () 
	{
		Invoke ("Explode", PlayerShooterController.inst.explosionTime);
	}

    void Explode ()
	{
		List<Collider> colliders = new List<Collider> (Physics.OverlapSphere (transform.position, PlayerShooterController.inst.explosionRadius));
		for (int i = colliders.Count - 1; i >= 0; i--)
		{
			switch (colliders[i].gameObject.tag) 
			{
				case ("Player"):
				{
					PlayerShooterController.inst.currentHealth = 0;
					Destroy (PlayerShooterController.inst.gameObject);
				}
				break;

				case ("Enemy"):
				{
					Destroy (colliders[i].gameObject);
				}
				break;
				
				case ("ExplodingBarrel"):
				{
					if (colliders[i].gameObject != gameObject)
						colliders[i].GetComponent<ExplodingBarrel> ().Explode ();
				}
				break;
			}
		}

		Destroy (gameObject);
	}
}