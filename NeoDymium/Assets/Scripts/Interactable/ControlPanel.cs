using System.Collections.Generic;
using UnityEngine;

//to be called using tryinteract
public class ControlPanel : IInteractable
{
	public List<IHackable> affectedItems;
	[SerializeField] Material controlPanelMat;
	[SerializeField] Color defaultColor;

	public bool activated = false;

	AudioSource audioSource;

	public override void Start ()
	{
		base.Start ();
		activated = false;
		controlPanelMat = transform.GetChild(0).GetComponent<Renderer>().material; //To Access the Material and Deactivate the Colors
		defaultColor = controlPanelMat.color;
		soundManager = SoundManager.inst;
		audioSource = GetComponent<AudioSource> ();
	}

	public override void Interact ()
	{
		if (!activated) 
			Disable();
		audioSource.Play ();
		//Get Component and Check if Component Exist to prevent error
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		//gameObject.GetComponent<RespectiveGoals>().isCompleted = true; //Nigel
	}

	public override string GetError(int key = 0)
	{
		if (activated) return "Control Panel has already been Disabled";
		else return string.Empty;
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