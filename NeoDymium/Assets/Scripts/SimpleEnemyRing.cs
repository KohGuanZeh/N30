using UnityEngine;

public class SimpleEnemyRing : MonoBehaviour
{
    public bool inside = false;

	void OnTriggerEnter (Collider other) 
	{
		if (other.tag == "Player")
			inside = true;
	}

	void OnTriggerExit (Collider other) 
	{
		if (other.tag == "Player")
			inside = false;
	}
}