using UnityEngine;

public class AIDoor : MonoBehaviour
{
	public ColorIdentifier requiredColor;
	
	Animator animator;

	void Start ()
	{
		animator = GetComponent<Animator> ();
	}

	void Open ()
	{
		SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		animator.SetBool  ("Opened", true);
	}

	void Close ()
	{
		SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
		animator.SetBool  ("Opened", false);
	}

	void OnTriggerStay (Collider other)
	{
		if ((other.tag == "Hackable" && other.GetComponent<IHackable> ().color == requiredColor))
		{
			Open ();
		}

		if (other.tag == "Player" && requiredColor == ColorIdentifier.none)
		{
			Open ();
		}
	}

	void OnTriggerExit (Collider other)
	{
		if ((other.tag == "Hackable" && other.GetComponent<IHackable> ().color == requiredColor))
		{
			Close ();
		}

		if (other.tag == "Player" && requiredColor == ColorIdentifier.none)
		{
			Close ();
		}
	}
}