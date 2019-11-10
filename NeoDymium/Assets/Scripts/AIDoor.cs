using UnityEngine;
using UnityEngine.UI;
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

		nowForeverOpened = requiredColor == ColorIdentifier.none? true : false;
		if (nowForeverOpened)
		{
			SetDoorToUnlocked();
			animator.Play("Unlock", 1, 1);
		} 

		//Get Materials to Change Emission
		emissiveMats = MaterialUtils.GetMaterialsFromRenderers(emissiveRs);
	}

	void Open ()
	{
		SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		animator.SetBool("Opened", true);
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

	public void SetDoorToUnlocked()
	{
		ChangeEmissionColor();
		obstacle.enabled = false;
		nowForeverOpened = true;
		animator.SetBool("Unlocked", true);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Hackable" && other.GetComponent<IHackable>().color == requiredColor && !nowForeverOpened)
		{
			if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Unlock")) animator.SetTrigger("Unlock");
		}
	}

	void OnTriggerStay (Collider other)
	{
		if (nowForeverOpened && (other.tag == "Hackable" || other.tag == "Player")) Open();
	}

	void OnTriggerExit (Collider other)
	{
		if (nowForeverOpened && (other.tag == "Hackable" || other.tag == "Player")) Close();
	}
}