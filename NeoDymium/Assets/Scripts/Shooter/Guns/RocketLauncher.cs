using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : IGun
{
	[Header("For Shoot Effects")]
	public float knockbackForce;
	public bool affectYAxis;

	public override void ShootEffect()
	{
		Vector3 knockBackDir = affectYAxis ? -1 * shootDir : new Vector3(shootDir.x, 0, shootDir.z).normalized * -1;
		player.PlayerKnockback(knockBackDir, knockbackForce);
	}
}
