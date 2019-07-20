using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunProjectile : IProjectile
{
	[Header("Shotgun Projectile Properties")]
	public float velocityThreshold;

	protected override void OnCollisionEnter(Collision collision)
	{
		if (rb.velocity.magnitude < velocityThreshold) return;
		base.OnCollisionEnter(collision);
	}
}
