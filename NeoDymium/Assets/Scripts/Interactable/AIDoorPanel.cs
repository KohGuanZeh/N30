public class AIDoorPanel : IInteractable
{
	
    public override void Interact ()
	{
		GetComponentInParent<AIDoor> ().unlocked = true;
	}
	
}