using System.Collections.Generic;
using UnityEngine;

//to be called using tryinteract
public class ControlPanel : IInteractable
{
	public List<IHackable> affectedItems;
	public bool activated = false;
	AudioSource audioSource;

	[Header("For Mat Change")]
	[SerializeField] Renderer[] screenRs;
	[SerializeField] Material[] screenMats;

	public override void Start ()
	{
		base.Start ();
		activated = false;
		screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
		soundManager = SoundManager.inst;
		audioSource = GetComponent<AudioSource> ();
	}

	public override void Interact ()
	{
		if (!activated) Disable();
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
		MaterialUtils.ToggleMaterialsEmission(screenMats, false);
		foreach (IHackable item in affectedItems)
			item.EnableDisable(false, color);
	}

	public void Restore() //If we adding Circuit Enablers
	{
		activated = false;
		MaterialUtils.ToggleMaterialsEmission(screenMats, true);
		foreach (IHackable item in affectedItems)
			item.EnableDisable(true, color);
	}
}