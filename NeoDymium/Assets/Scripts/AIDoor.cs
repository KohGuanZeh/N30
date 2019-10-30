using UnityEngine;

public class AIDoor : MonoBehaviour
{
	public ColorIdentifier requiredColor;
	public bool nowForeverOpened;
	Animator animator;

	void Start ()
	{
		animator = GetComponent<Animator> ();
		nowForeverOpened = false;
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

		if (other.tag == "Player" && nowForeverOpened)
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
			nowForeverOpened = true;
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