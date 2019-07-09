using UnityEngine;
using System.Collections.Generic;

public class ExplodingBarrel : MonoBehaviour
{
	public int hitsLeft = 2;
	public float radius = 5;

	void Update () 
	{
		if (hitsLeft <= 0)
			Explode ();
	}

	public void Explode ()
	{
		List<Collider> colliders = new List<Collider> (Physics.OverlapSphere (transform.position, radius));
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