//used with interact
public class ExitDoor : IInteractable
{
	public bool locked = true;

	void Start ()
	{
		locked = true;
	}

	public override void Interact ()
	{
		OpenDoor ();
	}

	public void OpenDoor () 
	{
		if (!locked)
			gameObject.SetActive (false);
	}
}