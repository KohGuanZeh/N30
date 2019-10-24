using UnityEngine;

public class AIDoor : MonoBehaviour
{
	public ColorIdentifier requiredColor;
	
	void OnTriggerStay (Collider other)
	{
		if ((other.tag == "Hackable" && other.GetComponent<IHackable> ().color == requiredColor))
		{
			transform.GetChild (0).gameObject.SetActive (false);
			SoundManager.inst.PlaySound (SoundManager.inst.slidingDoor);
			gameObject.SetActive (false);
		}
	}
}