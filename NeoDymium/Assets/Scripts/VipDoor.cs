using UnityEngine;
using System.Collections.Generic;

public class VipDoor : IInteractable
{
	public static VipDoor inst;

	public Transform[] possiblePositions; //will spawn 7 passes, so at least 7 transforms
	public GameObject pass;

	void Awake ()
	{
		inst = this;
	}

	public override void Start ()
	{
		base.Start ();

		List<Transform> possiblePos = new List<Transform> (possiblePositions);
		int vipPassIndex = Random.Range (0, 7);
		
		for (int i = 0; i < 7; i++)
		{
			int selectedIndex = Random.Range (0, possiblePos.Count);

			GameObject passs = Instantiate (pass, possiblePos[selectedIndex].position, Quaternion.identity, transform);
			if (i == vipPassIndex)
			{
				passs.GetComponent<Pass> ().vip = true;
				passs.name = "VIP Pass";
			}
			else
			{
				passs.name = "P ASS";
			}

			possiblePos.RemoveAt (selectedIndex);
		}
	}

	public override void TryInteract (ColorIdentifier userColor)
	{
		return;
	}

	public override void Interact ()
	{
		if (player.vipPass)
			gameObject.SetActive (false);
	}
}