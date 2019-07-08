using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[Header("Projectile Properties")]
	public Rigidbody rb;
	public LayerMask shootableLayers;
	public Vector3 projectileDir;
	public float projectileSpd;
	public int projectileDmg;
	public float lifeTime;
	
	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

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
		if (shootableLayers == (shootableLayers | (1 << collision.gameObject.layer)))
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

		print(collision.gameObject.name);
		Destroy(gameObject);
	}
}
