using UnityEngine;

public class IInteractable : MonoBehaviour
{
	[Header ("Common Interactable Properties")]
	public ColorIdentifier color;
	public bool allowPlayerInteraction = false;
	//May want to have a Bool to check if AI interaction is allowed
	[SerializeField] bool requireColor;

	public PlayerController player;
	public Collider col;
	GameObject whiteDot;
	public float whiteDotRaycastHeightOffset = 0.5f;

	public virtual void Start()
	{
		player = PlayerController.inst;
		whiteDot = Instantiate (UIManager.inst.whiteDot, Vector3.zero, Quaternion.identity, UIManager.inst.whiteDotHolder);
	}

	protected virtual void Update ()
	{
		WhiteDot ();
	}

	void WhiteDot ()
	{
		Vector3 whiteDotPos = transform.position + Vector3.up * whiteDotRaycastHeightOffset;
        Ray r = new Ray (whiteDotPos, (player.CurrentViewingCamera.transform.position - whiteDotPos).normalized);
		RaycastHit[] hits = Physics.RaycastAll (r, (player.CurrentViewingCamera.transform.position - whiteDotPos).magnitude, player.aimingRaycastLayers);

		bool passed = true;
		foreach (RaycastHit hit in hits)
		{
			if (hit.collider != col)
			{
				if (!player.inHackable)
				{
					if (hit.collider != player.GetPlayerCollider ())
					{
						passed = false;
					}
				}
				else
				{
					if (hit.collider != player.hackedObj.col)
					{
						passed = false;
					}
				}
			}
		}

		if (!col.IsVisibleFrom (player.CurrentViewingCamera))
			passed = false;

		if (passed)
		{
			whiteDot.gameObject.SetActive (true);
        	whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (whiteDotPos);
		}
		else
		{
			whiteDot.gameObject.SetActive (false);
		}

		/*
		Ray ray = new Ray (player.CurrentViewingCamera.transform.position, (transform.position - player.CurrentViewingCamera.transform.position).normalized);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, player.aimingRaycastLayers))
		{
			if (col == hit.collider)
			{
				whiteDot.gameObject.SetActive(true);
				whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint(transform.position);
			}
			else
			{
				whiteDot.gameObject.SetActive(false);
			}
		}
		else 
		{
			whiteDot.gameObject.SetActive(false);
		}
		*/
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

	/// <summary>
	/// Get the Error Message Corresponding to the Action
	/// </summary>
	/// <param name="key"> 0 is Main Interaction Button. 1 is Secondary Interaction Button </param>
	/// <returns></returns>
	public virtual string GetError(int key = 0)
	{
		return "";
	}
}