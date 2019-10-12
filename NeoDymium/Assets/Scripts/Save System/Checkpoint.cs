using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	//May have a System whereby upon Player/UI Manager keeps track of which Checkpoint to spawn, Loop through the list of the Checkpoints up to the Player Spawn and call their Load Function
	//to Disable the Control Panels that are meant to be disabled
	[SerializeField] PlayerController player;
	[SerializeField] LayerMask playerLayer;
	[SerializeField] Collider coll;
	[SerializeField] GameObject wall;
	[SerializeField] List<IHackable> hackables;

    // Start is called before the first frame update
    void Awake()
    {
		if (wall) wall.SetActive(false);
		player = FindObjectOfType<PlayerController>(); //Need to Find Object of Type since this has to be initialised together with Player
		coll = GetComponent<Collider>();
    }

	public void LoadCheckPoint()
	{
		if (wall) wall.SetActive(true);
		coll.enabled = false;

		//Yaw and Pitch of Players are Set in Respawn function
		player.transform.position = transform.position;
		player.transform.eulerAngles = transform.eulerAngles;
	}

	public void SetHackableMemory(int cpIndex)
	{
		for (int i = 0; i < hackables.Count; i++) hackables[i].GetSetPlayerMemory(cpIndex, i, false);
	}

	public void GetHackableMemory(int cpIndex)
	{
		for (int i = 0; i < hackables.Count; i++) hackables[i].GetSetPlayerMemory(cpIndex, i);
	}

	private void OnTriggerEnter(Collider other)
	{
		//If detected Player, Considered Check point Registered
		if (playerLayer == (playerLayer | (1 << other.gameObject.layer)))
		{
			PlayerPrefs.SetInt("Checkpoint", ++player.checkPointsPassed);
			SetHackableMemory(player.checkPointsPassed);
			if (wall) wall.SetActive(true);
			coll.enabled = false;
		}
	}
}
