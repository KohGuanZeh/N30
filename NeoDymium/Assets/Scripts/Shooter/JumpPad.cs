using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
	[Header ("Jump Pad Properties")]
	[SerializeField] Transform jumpDestination;

	public void LaunchPlayer(Transform player)
	{
		//Now only need to do the Jump Arc
		player.transform.position = jumpDestination.position + new Vector3(0, PlayerShooterController.inst.distFromGround, 0);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerShooterController>()) LaunchPlayer(other.transform);
	}
}
