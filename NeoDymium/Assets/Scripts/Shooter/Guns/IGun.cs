using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGun : MonoBehaviour
{
	[Header("General Properties")]
	protected PlayerShooterController player;
	protected Camera playerCam; //For Raycasting and Getting Bullet Direction
	protected Transform playerShootPoint; //Where the particles and bullets should instantiate
	protected LayerMask shootLayers;
	public bool gunInitialised;

	[Header("Ammos and Clips")]
	public int ammo;
	public int ammoPerClip;
	public bool infiniteClips;
	public int clips;

	[Header("For Shooting")]
	public bool shootsProjectiles;
	public Ray shootRay;
	public RaycastHit hit;
	public Vector3 shootDir;
	public float effectiveRange;
	public int shootDmg, effectsDmg; //If Explosion is used for HP depletion instead?
	public Projectile projectile;

	public void InitialiseGun(PlayerShooterController player, Camera playerCam, Transform playerShootPoint, LayerMask shootLayers)
	{
		this.player = player;
		this.playerCam = playerCam;
		this.playerShootPoint = playerShootPoint;
		this.shootLayers = shootLayers;

		ammo = ammoPerClip;
		gunInitialised = true;
		print("Gun Initiliased at " + Time.time);
	}

	public virtual void ChangeShootPointPosition()
	{
		//Different Guns may have different Shoot Points. Hence this function will adjust the Shoot Point Accordingly.
	}

	public virtual void Shoot(bool autoReload = true)
	{
		//For now Auto Reload
		//Unsure if I should be handling the Raycasting and Setting Projectile Direction here
		if (ammo == 0)
		{
			if(autoReload) Reload();
			return;
		}
		else ammo--;

		GetShootDirection();

		if (shootsProjectiles) ProjectileShoot();
		else RaycastShoot();

		ShootEffect();
	}

	public virtual void Reload()
	{
		if (!infiniteClips && clips == 0) return;

		ammo = ammoPerClip;
		clips--;
	}

	protected virtual void GetShootDirection()
	{
		//May need to add Spread and Deviation, whether the Player Aims or not etc.
		shootRay = new Ray(playerCam.transform.position, playerCam.transform.forward);

		if (shootsProjectiles)
		{
			if (Physics.Raycast(shootRay, out hit, effectiveRange, shootLayers)) shootDir = (hit.point - playerShootPoint.position).normalized;
			else shootDir = shootRay.direction;
		}
		else shootDir = shootRay.direction;
	}

	public virtual void ProjectileShoot()
	{
		Projectile bullet = Instantiate(projectile, playerShootPoint.position, Quaternion.identity);
		bullet.rb.velocity = shootDir * bullet.projectileSpd;
	}

	public virtual void RaycastShoot()
	{
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

	public virtual void ShootEffect()
	{
		
	}
}
