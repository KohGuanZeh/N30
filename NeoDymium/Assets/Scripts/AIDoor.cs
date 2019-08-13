using UnityEngine;

public class AIDoor : MonoBehaviour
{
	// void OnTriggerEnter (Collider other)
	// {
	// 	if (other.tag == "Hackable")
	// 		gameObject.SetActive (false);
	// }

	
	public ColorIdentifier requiredColor;
	//public GameObject door;
	//public LayerMask aiLayer;
	//public bool unlocked = false;

	/* 
	void Start ()
	{
		unlocked = false;
	}
	*/

	void OnTriggerStay (Collider other)
	{
		if ((other.tag == "Hackable"/*9*/ && other.GetComponent<IHackable> ().color == requiredColor)) //|| (unlocked && other.gameObject.layer == unlockedLayer))//(other.gameObject.layer == 9 || other.gameObject.layer == 8)))
		{
			transform.GetChild (0).gameObject.SetActive (false);
			gameObject.SetActive (false);
			//transform.GetChild (1).gameObject.SetActive (false);
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