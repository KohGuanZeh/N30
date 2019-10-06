using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NumpadButton : MonoBehaviour
{
	[SerializeField] UnityEvent ButtonPress;
	[SerializeField] Collider coll;

	public void EnableDisableCollider(bool active)
	{
		coll.enabled = active;
	}

	public void InvokeButtonPress()
	{
		ButtonPress.Invoke();
	}
}
