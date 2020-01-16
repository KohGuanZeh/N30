using UnityEngine;
using UnityEngine.Events;

public class TriggerUnityEvent : MonoBehaviour
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