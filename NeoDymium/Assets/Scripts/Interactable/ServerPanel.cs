//to be used in interact
public class ServerPanel : IInteractable
{
	public ExitDoor linkedDoor;

	public override void Interact ()
	{
		linkedDoor.locked = false;
	}
}