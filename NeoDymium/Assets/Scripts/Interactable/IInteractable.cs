using UnityEngine;

public class IInteractable : MonoBehaviour
{
	[Header ("Common Interactable Properties")]
	public ColorIdentifier color;
	public bool allowPlayerInteraction = false;
	[SerializeField] bool requireColor;

	protected virtual void Start()
	{

	}

	//For Componenets that Require Color for Interaction
	//Called in Player Script when in Hackable
	public virtual void TryInteract (ColorIdentifier userColor)
	{
		if (!requireColor || color == ColorIdentifier.none) Interact();
		else if (color == userColor) Interact();
	}

	//To be Called in Player Controller Script when Press E if Player is not in Hackable
	public virtual void Interact()
	{

	}

	public virtual void Uninteract()
	{

	}
}