using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
	//May have a System whereby upon Player/UI Manager keeps track of which Checkpoint to spawn, Loop through the list of the Checkpoints up to the Player Spawn and call their Load Function
	//to Disable the Control Panels that are meant to be disabled
	[SerializeField] PlayerController player;
	[SerializeField] Collider coll;
	// [SerializeField] GameObject wall;
	[SerializeField] List<IHackable> hackables;
	UIManager uIManager;

    // Start is called before the first frame update
    void Awake ()
    {
		// if (wall) wall.SetActive(false);
		player = FindObjectOfType<PlayerController>(); //Need to Find Object of Type since this has to be initialised together with Player
		coll = GetComponent<Collider>();
    }
	
	void Start ()
	{
		uIManager = UIManager.inst;
	}

	private void Update()
	{
		/*if (Input.GetKeyDown(KeyCode.P))
		{
			PlayerPrefs.DeleteKey("Last Objective Saved");
			LoadingScreen.inst.LoadScene(SceneManager.GetActiveScene().name);
		}*/
	}

	public void LoadCheckPoint()
	{
		// if (wall) wall.SetActive(true);
		coll.enabled = false;

		//Yaw and Pitch of Players are Set in Respawn function
		player.transform.position = transform.position;
		player.transform.eulerAngles = transform.eulerAngles;
	}

	private void OnTriggerEnter(Collider other)
	{
		//If detected Player, Considered Check point Registered
		if (other.gameObject.tag == "Player")
		{
			uIManager.touchedCheckpoint = true;
			uIManager.ShowSavedAfterCheckpoints();
			player.checkPointsPassed++;
			PlayerPrefs.SetInt("Scene Index", SceneManager.GetActiveScene().buildIndex);
			PlayerPrefs.SetInt("Last Objective Saved", player.checkPointsPassed);
			// if (wall) wall.SetActive(true);
			coll.enabled = false;
		}
	}
}
