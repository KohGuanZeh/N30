using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGun : MonoBehaviour
{
	[Header("Gun Properties")]
	public int ammo;
	public int ammoPerClip;
	public int clips;
	public bool isProjectileBased;
	public Projectile projectile;
	//May want to include Shoot type... Whether its trajectory based or etc etc

	public virtual void Shoot()
	{
		
	}

	public virtual void ShootEffect()
	{

	}
}
