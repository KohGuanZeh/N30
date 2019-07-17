using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IProjectile : MonoBehaviour
{
	[Header("General Properties")]
	public Rigidbody rb;
	public IGun gun;
	public LayerMask shootLayers;

	[Header ("Projectile Properties")]
	public float projectileSpd;
	public int projectileDmg;
	public float lifeTime;
	public bool usesGravity;
	public bool effectOnImpact = true; //Check if the Projectile Effect should occur on Impact
	public bool destroyOnHit = true;

	[Header("Projectile Effects")]
	public GameObject particleEffect;
	public float particleLifeTime;

    // Update is called once per frame
    protected virtual void Update()
    {
		//Or want to destroy by effective Distance
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
		{
			ProjectileEffect();
			Destroy(gameObject);
		}
    }

	public void InitialiseProjectile(IGun gun)
	{
		this.gun = gun;
		projectileDmg = gun.shootDmg;
		shootLayers = gun.shootLayers;
		rb.useGravity = usesGravity;
		//lifeTime = gun.effectiveRange / projectileSpd;
	}

	public virtual void ProjectileEffect()
	{
		
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (shootLayers == (shootLayers | (1 << collision.gameObject.layer)))
		{
			switch (collision.gameObject.tag)
			{
				case ("Enemy"):
					{
						collision.gameObject.GetComponent<SimpleEnemy>().health -= projectileDmg;
					}
					break;

				case ("ExplodingBarrel"):
					{
						collision.gameObject.GetComponent<ExplodingBarrel>().hitsLeft--;
					}
					break;
			}
		}

		if (effectOnImpact) ProjectileEffect();

		//print(collision.gameObject.name);
		if (destroyOnHit) Destroy(gameObject);
	}
}
