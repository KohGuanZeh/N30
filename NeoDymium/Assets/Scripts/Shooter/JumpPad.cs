using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
	[Header ("Jump Pad Properties")]
	[SerializeField] Transform jumpDestination;

	public void LaunchPlayer(Transform player)
	{
		//Do an Arc Jump
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player") LaunchPlayer(collision.transform);
	}
}
