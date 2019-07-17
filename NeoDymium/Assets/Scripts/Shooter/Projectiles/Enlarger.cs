using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enlarger : IProjectile
{
	[Header("Enlargement Properties")]
	public float enlargementScale;
	public List<int> indexesToRemove;
	public List<Transform> targets;
	public List<Vector3> desiredScale;
	public List<float> lerpTimes;
	public float lerpSpeed;

	public override void InitialiseProjectile(IGun gun)
	{
		base.InitialiseProjectile(gun);
		indexesToRemove = new List<int>();
		targets = new List<Transform>();
		desiredScale = new List<Vector3>();
		lerpTimes = new List<float>();
	}

	public override void ProjectileEffect()
	{
		lifeTime = 5; //Always Set the Lifetime to 5 so that it does not destroy itself while lerping other peeps

		//Problem now is that it can be enlarged many times... And the removal system is not perfect
		//Also no clear Projectile System for this
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i])
			{
				lerpTimes[i] = Mathf.Min(lerpTimes[i] + Time.deltaTime * lerpSpeed, 1);
				targets[i].localScale = Vector3.Lerp(targets[i].localScale, desiredScale[i], lerpTimes[i]);

				if (lerpTimes[i] >= 1)
				{
					targets[i].localScale = desiredScale[i];
					indexesToRemove.Add(i);
				}
			}
			else indexesToRemove.Add(i);
		}

		foreach (int i in indexesToRemove)
		{
			targets.RemoveAt(i);
			desiredScale.RemoveAt(i);
			lerpTimes.RemoveAt(i);
		}

		indexesToRemove.Clear();

		if (targets.Count == 0) Destroy(gameObject);
	}

	protected void HideMeshAndCollider()
	{
		GetComponent<MeshRenderer>().enabled = false;
		GetComponent<Collider>().enabled = false;
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		if (shootLayers == (shootLayers | (1 << collision.gameObject.layer)))
		{
			targets.Add(collision.transform);
			desiredScale.Add(collision.transform.localScale * enlargementScale);
			lerpTimes.Add(0);
		}

		if (effectOnImpact)
		{
			ProjectileEffect();
			activateEffect = true;
		}

		if (destroyOnHit)
		{
			if (targets.Count == 0) Destroy(gameObject);
			else HideMeshAndCollider();
		}
	}
}
