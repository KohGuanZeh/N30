using UnityEngine;
using UnityEngine.AI;

public class AIDoor : MonoBehaviour
{
	public ColorIdentifier requiredColor;
	public bool nowForeverOpened;
	[SerializeField] float intensity = 1;
	[SerializeField] Renderer[] emissiveRs;
	[SerializeField] Material[] emissiveMats;
	Animator animator;
	NavMeshObstacle obstacle;

	void Start ()
	{
		animator = GetComponent<Animator> ();
		obstacle = GetComponent<NavMeshObstacle> ();
		nowForeverOpened = false;

		//Get Materials to Change Emission
		emissiveMats = MaterialUtils.GetMaterialsFromRenderers(emissiveRs);
	}

	void Open ()
	{
		SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		float startTime = 0;
		animator.SetBool("Opened", true);
		/*if (animator.GetCurrentAnimatorStateInfo(0).IsName("Closed"))
		{
			startTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		animator.SetBool  ("Opened", true);
		animator.GetCurrentAnimatorStateInfo(0).normalizedTime;*/
	}

	void Close ()
	{
		SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
		animator.SetBool  ("Opened", false);
	}

	void ChangeEmissionColor(bool unlocked = true)
	{
		Color emissiveColor = unlocked ? new Color(0.62f, 1.28f, 0.65f) : new Color(1.5f, 0.43f, 0.43f, 1);
		MaterialUtils.ChangeMaterialsEmission(emissiveMats, emissiveColor, intensity);
		//foreach (Material emissiveMat in emissiveMats) emissiveMat.SetColor("_EmissionColor", emissiveColor);
	}

	void OnTriggerStay (Collider other)
	{
		if ((other.tag == "Hackable" && other.GetComponent<IHackable> ().color == requiredColor))
		{
			if (!nowForeverOpened) ChangeEmissionColor();
			nowForeverOpened = true;
			obstacle.enabled = false;
			Open ();
		}

		if (other.tag == "Hackable" && nowForeverOpened)
		{
			Open ();
		}

		if (other.tag == "Player" && nowForeverOpened)
		{
			Open ();
		}

		if (other.tag == "Player" && requiredColor == ColorIdentifier.none)
		{
			if (!nowForeverOpened) ChangeEmissionColor();
			Open ();
		}
	}

	void OnTriggerExit (Collider other)
	{
		if ((other.tag == "Hackable" && other.GetComponent<IHackable> ().color == requiredColor))
		{
			Close ();
		}

		if (other.tag == "Hackable" && nowForeverOpened)
		{
			Close ();
		}

		if (other.tag == "Player" && nowForeverOpened)
		{
			Close ();
		}

		if (other.tag == "Player" && requiredColor == ColorIdentifier.none)
		{
			Close ();
		}
	}
}