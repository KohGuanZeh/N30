using UnityEngine;

public class IInteractable : MonoBehaviour
{
	[Header ("Common Interactable Properties")]
	public ColorIdentifier color;
	public bool allowPlayerInteraction;
	[SerializeField] bool requireColor;

	//For Componenets that Require Color for Interaction
	public virtual void TryInteract (ColorIdentifier userColor)
	{
		if (!requireColor || color == ColorIdentifier.none) Interact();
		else if (color == userColor) Interact();
	}

	//To be Called in Player Controller Script when Press E
	public virtual void Interact()
	{

	}

	public virtual void OnUninteract()
	{

	}
}