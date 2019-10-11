//to be used in interact
using UnityEngine;
public class ServerPanel : IInteractable
{
	public ExitDoor linkedDoor;
	public Material serverPanelMat;
	public Color defaultColor;

	public override void Start()
	{
		base.Start();
		serverPanelMat = transform.GetChild(0).GetComponent<Renderer>().material;
		defaultColor = serverPanelMat.color;
	}

	//If anything hackables try to interact, deny it. Only player can interact
	public override void TryInteract(ColorIdentifier userColor)
	{
		return; 
	}

	public override void Interact ()
	{
		Disable();
		gameObject.GetComponent<RespectiveGoals>().isCompleted = true; //Nigel
	}

	public override string GetError(int key = 0)
	{
		if (player.inHackable) return "AI cannot interact with this Object";
		else if (!linkedDoor.locked) return "Server Panel has already been Disabled";
		else return string.Empty;
	}

	public void Disable()
	{
		linkedDoor.locked = false;
		serverPanelMat.DisableKeyword("_EMISSION");
		serverPanelMat.color = Color.grey;
	}

	//If there is even a Restore for the Server Panel
	public void Restore()
	{
		linkedDoor.locked = true;
		serverPanelMat.EnableKeyword("_EMISSION");
		serverPanelMat.color = defaultColor;
	}
}