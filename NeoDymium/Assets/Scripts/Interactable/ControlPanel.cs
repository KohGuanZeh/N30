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
	[SerializeField] Color defaultColor;
	[SerializeField] float defaultIntensity;

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
		MaterialUtils.ChangeMaterialsEmission(screenMats, Color.black, 0, "_EmissiveColor");
		//MaterialUtils.ChangeMaterialsEmissionHDRP(screenMats, Color.black);
		//MaterialUtils.ChangeMaterialsIntensityHDRP(screenMats, 0);
		foreach (IHackable item in affectedItems)
			item.EnableDisable(false, color);
	}

	public void Restore() //If we adding Circuit Enablers
	{
		activated = false;
		MaterialUtils.ChangeMaterialsEmission(screenMats, defaultColor, defaultIntensity, "_EmissiveColor");
		//MaterialUtils.ChangeMaterialsEmissionHDRP(screenMats, new Color(0, 0.2549019f, 0.4509804f));
		//MaterialUtils.ChangeMaterialsIntensityHDRP(screenMats, 45);
		foreach (IHackable item in affectedItems)
			item.EnableDisable(true, color);
	}
}