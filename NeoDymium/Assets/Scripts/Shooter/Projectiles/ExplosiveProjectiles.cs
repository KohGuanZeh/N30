using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectiles : IProjectile
{
	[Header ("For Projectile Effect")]
	public float explosionRadius;

	public override void ProjectileEffect()
	{
		List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(transform.position, explosionRadius));
		for (int i = colliders.Count - 1; i >= 0; i--)
		{
			switch (colliders[i].gameObject.tag)
			{
				/*case ("Player"):
					{
						PlayerShooterController.inst.currentHealth = 0;
						Destroy(PlayerShooterController.inst.gameObject);
					}
					break;*/

				case ("Enemy"):
					{
						Destroy(colliders[i].gameObject);
					}
					break;

				case ("ExplodingBarrel"):
					{
							colliders[i].GetComponent<ExplodingBarrel>().Explode();
					}
					break;
					
					//May want a case that destroys all projectiles in range as well
			}
		}

		GameObject exploision = Instantiate(particleEffect, transform.position, Quaternion.identity);
		exploision.transform.localScale = Vector3.one * explosionRadius * 2;
		Destroy(exploision, particleLifeTime);
	}
}
