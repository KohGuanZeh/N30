using UnityEngine;
using TMPro;

public class Pass : IInteractable
{
	public bool vip = false;
	public GameObject textHolder;
	TextMeshProUGUI text;

	public override void Start ()
	{
		base.Start ();
		text = textHolder.GetComponentInChildren<TextMeshProUGUI> ();
		text.text = vip ? "VIP Pass" : "P ASS";
		textHolder.gameObject.SetActive (false);
		col = GetComponent<Collider> ();
		foreach (IHackable hackable in FindObjectsOfType<IHackable> ())
			Physics.IgnoreCollision (col, hackable.col, true);
		Physics.IgnoreCollision (col, player.GetPlayerCollider (), true);
	}

	protected override void Update ()
	{
		base.Update();
		if (textHolder.gameObject.activeSelf)
			textHolder.transform.LookAt (2 * text.transform.position - player.CurrentViewingCamera.transform.position);
	}

	public override void TryInteract (ColorIdentifier userColor)
	{
		return;
	}

	public override void Interact ()
	{
		if (player.holdingPass)
		{	
			GameObject pass = Instantiate (VipDoor.inst.pass, player.transform.position + Vector3.up, Quaternion.identity);
			if (player.vipPass)
			{
				pass.GetComponent<Pass> ().vip = true;
				pass.name = "VIP Pass";
			}
			else
			{
				pass.GetComponent<Pass> ().vip = false;
				pass.name = "P ASS";
			}
		}
		
		player.holdingPass = true;
		player.vipPass = vip;
		Destroy (gameObject);
	}
}