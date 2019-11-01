using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NumpadButton : MonoBehaviour
{
	[SerializeField] UnityEvent ButtonPress;
	[SerializeField] Collider coll;

	private void Awake()
	{
		coll = GetComponent<Collider>();
	}

	public void EnableDisableCollider(bool active)
	{
		coll.enabled = active;
	}

	public void InvokeButtonPress()
	{
		ButtonPress.Invoke();
	}
}
