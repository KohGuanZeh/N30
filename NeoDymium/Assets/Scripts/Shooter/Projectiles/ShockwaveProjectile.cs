using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveProjectile : IProjectile
{
	[Header("For Projectile Effect")]
	public float shockRadius;
	public float knockbackForce;
	public bool affectYAxis;

	public override void ProjectileEffect()
	{
		List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(transform.position, shockRadius));
		for (int i = colliders.Count - 1; i >= 0; i--)
		{
			switch (colliders[i].gameObject.tag)
			{
				case ("Player"):
					{
						Vector3 knockBackDir = (colliders[i].transform.position - transform.position).normalized;
						if (!affectYAxis) knockBackDir = new Vector3(knockBackDir.x, 0, knockBackDir.z).normalized;
						PlayerShooterController.inst.PlayerKnockback(knockBackDir, knockbackForce);
					}
					break;

				case ("Enemy"):
					{
						//Cause Knockback and maybe deal a little damage?
						Vector3 knockBackDir = (transform.position - colliders[i].transform.position).normalized;
						if (!affectYAxis) knockBackDir = new Vector3(knockBackDir.x, 0, knockBackDir.z).normalized;
					}
					break;

				case ("ExplodingBarrel"):
					{
						colliders[i].GetComponent<ExplodingBarrel>().Explode();
					}
					break;
			}
		}

		GameObject shockWave = Instantiate(particleEffect, transform.position, Quaternion.identity);
		shockWave.transform.localScale = Vector3.one * shockRadius * 2;
		Destroy(shockWave, particleLifeTime);
	}
}
