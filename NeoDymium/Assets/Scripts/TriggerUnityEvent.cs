using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
	public UnityEvent events;

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Player" || other.tag == "HackableInteractable")
		{
			events.Invoke ();
		}
	}
}