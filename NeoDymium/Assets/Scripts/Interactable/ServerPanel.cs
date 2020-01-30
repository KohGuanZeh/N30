//to be used in interact
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerPanel : IInteractable
{
	[SerializeField] GameObject tempCanvas;
	[SerializeField] Animator tempCanvasAnim;
	[SerializeField] bool canCloseMenu;

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

	public override void Interact ()
	{
		//audioSource.Play ();
		ExecuteGameEnd();
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
	}

	public override string GetError(int key = 0)
	{
		if (player.inHackable) return "AI cannot interact with this Object";
		//else if (linkedDoor != null && !linkedDoor.locked) return "Server Panel has already been Disabled";
		else return string.Empty;
	}

	public void ExecuteGameEnd()
	{
		tempCanvas.gameObject.SetActive(true);
		tempCanvasAnim.SetBool("Ended", true);
	}

	public void AllowCloseMenu()
	{
		canCloseMenu = true;
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