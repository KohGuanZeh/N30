using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : IGun
{
	[Header("Shotgun Properties")]
	public int totalBullets;
	public float spreadAngle;

	public override void ProjectileShoot()
	{
		IProjectile bullet;
		for (int i = 0; i < totalBullets; i++)
		{
			Quaternion rotation = Quaternion.LookRotation(shootDir);
			rotation = Quaternion.RotateTowards(rotation, Random.rotation, spreadAngle);
			bullet = Instantiate(projectile, playerShootPoint.position, rotation);
			bullet.InitialiseProjectile(this);
			bullet.rb.velocity = bullet.transform.forward * bullet.projectileSpd;
		}
	}

	public override void RaycastShoot()
	{
		for (int i = 0; i < totalBullets; i++)
		{
			Vector3 rotation = new Vector3(Random.value, Random.value, Random.value);
			rotation = Vector3.RotateTowards(shootDir, rotation, spreadAngle * Mathf.Deg2Rad, 1);
			shootRay.direction = rotation.normalized;
			Debug.DrawLine(shootRay.origin, shootRay.origin + shootRay.direction * effectiveRange, Color.red, 5);
			if (Physics.Raycast(shootRay, out hit, effectiveRange, shootLayers))
			{
				switch (hit.collider.tag)
				{
					case ("Enemy"):
						{
							hit.collider.GetComponent<SimpleEnemy>().health -= shootDmg;
						}
						break;

					case ("ExplodingBarrel"):
						{
							hit.collider.GetComponent<ExplodingBarrel>().hitsLeft--;
						}
						break;
				}
			}
		}
	}
}
