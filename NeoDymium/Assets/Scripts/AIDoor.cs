using UnityEngine;

public class AIDoor : MonoBehaviour
{
	// void OnTriggerEnter (Collider other)
	// {
	// 	if (other.tag == "Hackable")
	// 		gameObject.SetActive (false);
	// }

	
	public ColorIdentifier color;
	public GameObject door;
	public bool unlocked = false;

	void Start ()
	{
		unlocked = false;
	}

	void OnTriggerStay (Collider other)
	{
		if ((other.gameObject.layer == 9 && other.GetComponent<IHackable> ().color == color) || (unlocked && (other.gameObject.layer == 9 || other.gameObject.layer == 8)))
		{
			if (!unlocked) unlocked = true;
			door.SetActive (false);
		}	

		/*if ((other.gameObject.layer == 9 && other.GetComponent<IHackable> ().color == color) || (unlocked && (other.gameObject.layer == 9 || other.gameObject.layer == 8)))
			door.SetActive (false);*/
	}

	/*void OnTriggerExit (Collider other)
	{
		if ((other.gameObject.layer == 9 && other.GetComponent<IHackable> ().color == color) || (unlocked && (other.gameObject.layer == 9 || other.gameObject.layer == 8)))
			door.SetActive (true);
	}*/
	
}