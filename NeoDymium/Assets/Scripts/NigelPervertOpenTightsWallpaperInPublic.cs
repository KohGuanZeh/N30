using UnityEngine;

public class NigelPervertOpenTightsWallpaperInPublic : MonoBehaviour
{
	public GameObject boinger;

	bool activated = false;

    void OnTriggerEnter (Collider other)
	{	
		if (other.tag == "Player" && !activated)
		{
			activated = true;
			boinger.SetActive (true);
		}
	}
}