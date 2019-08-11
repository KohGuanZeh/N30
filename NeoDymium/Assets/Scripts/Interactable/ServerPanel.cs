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