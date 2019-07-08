using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[Header("Projectile Properties")]
	public Vector3 bulletDir;
	public float bulletSpeed;
	public float lifeTime;
	public int bulletDmg;

    // Update is called once per frame
    void Update()
    {
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0) Destroy(gameObject);
    }

	public virtual void ProjectileHitEffect()
	{
		Destroy(gameObject);
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		ProjectileHitEffect();
	}
}
