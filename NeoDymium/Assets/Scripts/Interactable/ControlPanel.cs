﻿using System.Collections.Generic;
using UnityEngine;

//to be called using tryinteract
public class ControlPanel : IInteractable
{
	public List<IHackable> affectedItems;
	[SerializeField] Material controlPanelMat;
	[SerializeField] Color defaultColor;

	public bool activated = false;

	void Start ()
	{
		activated = false;
		controlPanelMat = transform.GetChild(0).GetComponent<Renderer>().material; //To Access the Material and Deactivate the Colors
		defaultColor = controlPanelMat.color;
	}

	public override void Interact ()
	{
		if (!activated) Disable();
		gameObject.GetComponent<RespectiveGoals>().isCompleted = true; //Nigel
	}

	public void Disable()
	{
		activated = true;
		controlPanelMat.DisableKeyword("_EMISSION");
		controlPanelMat.color = Color.grey;
		foreach (IHackable item in affectedItems)
			item.EnableDisable(false, color);
	}

	public void Restore() //If we adding Circuit Enablers
	{
		activated = false;
		controlPanelMat.EnableKeyword("_EMISSION");
		controlPanelMat.color = defaultColor;
		foreach (IHackable item in affectedItems)
			item.EnableDisable(true, color);
	}
}