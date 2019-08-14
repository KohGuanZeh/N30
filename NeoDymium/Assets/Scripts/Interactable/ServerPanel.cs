//to be used in interact
using UnityEngine;
public class ServerPanel : IInteractable
{
	public ExitDoor linkedDoor;
	public Material serverPanelMat;

	public void Start()
	{
		serverPanelMat = transform.GetChild(0).GetComponent<Renderer>().material;
	}

	//If anything hackables try to interact, deny it. Only player can interact
	public override void TryInteract(ColorIdentifier userColor)
	{
		return; 
	}

	public override void Interact ()
	{
		linkedDoor.locked = false;
		serverPanelMat.DisableKeyword("_EMISSION");
		serverPanelMat.color = Color.grey;
	}

	//If there is even a Restore for the Server Panel
	public void OnRestore()
	{

	}
}