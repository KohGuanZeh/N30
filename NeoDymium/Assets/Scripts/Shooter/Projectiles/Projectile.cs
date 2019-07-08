using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[Header("Projectile Properties")]
	public Vector3 projectileDir;
	public float projectileSpd;
	public int projectileDmg;
	public float lifeTime;

    // Update is called once per frame
    protected virtual void Update()
    {
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0) Destroy(gameObject);
    }

	public virtual void HitEffect()
	{
		
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		HitEffect();
		Destroy(gameObject);
	}
}
