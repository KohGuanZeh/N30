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
	[SerializeField] Transform checkPoint; //Position where the Player spawns
	[SerializeField] ControlPanel[] controlPanels; //List of Disabled Control Panels up to Checkpoint
	[SerializeField] ServerPanel[] serverPanels; //List of Disabled Server Panels up to Checkpoint

    // Start is called before the first frame update
    void Start()
    {
		player = PlayerController.inst;
		coll = GetComponent<Collider>();
    }

	public void LoadCheckPoint(bool setPlayerPos = true)
	{
		coll.enabled = false;
		foreach (ControlPanel controlPanel in controlPanels) controlPanel.Disable();
		foreach (ServerPanel serverPanel in serverPanels) serverPanel.Disable();

		if (setPlayerPos)
		{
			player.transform.position = transform.position;
			//Need to Set Rotation for Player and Player Canera as well
		}
	}

	public void ResetStateBeforeCheckpoint()
	{
		foreach (ControlPanel controlPanel in controlPanels) controlPanel.Restore();
		foreach (ServerPanel serverPanel in serverPanels) serverPanel.Restore();
	}

	private void OnTriggerEnter(Collider other)
	{
		//If detected Player, Considered Check point Registered
		if (playerLayer == (playerLayer | (1 << other.gameObject.layer)))
		{
			PlayerPrefs.SetInt("Checkpoint", ++player.checkPointsPassed);
			coll.enabled = false;
		}
	}
}
