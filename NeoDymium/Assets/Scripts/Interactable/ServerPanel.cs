//to be used in interact
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerPanel : IInteractable
{
	[SerializeField] bool canCloseMenu, isDummy;
	public GameObject gateway;

	public override void Start()
	{
		base.Start();
		//screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
	}

	protected override void Update()
	{
		base.Update();
		if (canCloseMenu && Input.GetMouseButtonDown(0)) LoadingScreen.inst.AutoLoadNextScene();
	}

	//If anything hackables try to interact, deny it. Only player can interact
	public override void TryInteract(ColorIdentifier userColor)
	{
		return; 
	}

	public override void Interact()
	{
		//audioSource.Play ();
		if (isDummy)
		{
			PlayLevel2Dialog();
			col.enabled = false;
			gameObject.layer = 0;
			if (gateway.activeSelf) gateway.SetActive (false);
			RespectiveGoals goal = GetComponent<RespectiveGoals>();
			if (goal) goal.isCompleted = true;
		}
		else
		{
			ExecuteGameEnd();
			RespectiveGoals goal = GetComponent<RespectiveGoals>();
			if (goal) goal.isCompleted = true;
		}
	}

	public override string GetError(int key = 0)
	{
		if (player.inHackable) return "Invalid Permission";
		//else if (linkedDoor != null && !linkedDoor.locked) return "Server Panel has already been Disabled";
		else return string.Empty;
	}

	public void PlayLevel2Dialog()
	{
		UIManager.inst.PlayCutscene(1);
	}

	public void ExecuteGameEnd()
	{
		UIManager.inst.PlayCutscene(2);
		UIManager.inst.isGameOver = true;
	}

	/*public void Disable()
	{
		linkedDoor.locked = false;
		MaterialUtils.ChangeMaterialsEmission(screenMats, Color.black, 0, "_EmissiveColor");
		MaterialUtils.ToggleMaterialsEmission(screenMats, false);
	}

	//If there is even a Restore for the Server Panel
	public void Restore()
	{
		linkedDoor.locked = true;
		MaterialUtils.ChangeMaterialsEmission(screenMats, defaultColor, defaultIntensity, "_EmissiveColor");
		MaterialUtils.ToggleMaterialsEmission(screenMats, true);
	}*/
}