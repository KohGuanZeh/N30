using UnityEngine;

public class IInteractable : MonoBehaviour
{
	[Header ("Common Interactable Properties")]
	public ColorIdentifier color;
	public bool allowPlayerInteraction = false;
	[SerializeField] bool requireColor;

	public PlayerController player;
	public Collider col;
	GameObject whiteDot;

	public virtual void Start()
	{
		player = PlayerController.inst;
		whiteDot = Instantiate (UIManager.inst.whiteDot, Vector3.zero, Quaternion.identity, UIManager.inst.whiteDotHolder);
	}

	void FixedUpdate ()
	{
		WhiteDot ();
	}

	void WhiteDot ()
	{
		/*
		if (col.IsVisibleFrom (player.CurrentViewingCamera))
		{
			whiteDot.SetActive (true);
			whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
		}
		else
			whiteDot.SetActive (false);
		*/

		whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
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