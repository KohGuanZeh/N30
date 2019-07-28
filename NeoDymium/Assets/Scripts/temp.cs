using UnityEngine;

public class temp : MonoBehaviour
{
    void Update () 
	{
		if (Input.GetKeyDown (key: KeyCode.E))
		{
			RaycastHit hit;
			Physics.Raycast (transform.position, /*cam.transform.forward */transform.forward, out hit, 3);

			if (hit.collider != null) 
				if (hit.collider.tag == "ExitDoor")
					if (!hit.collider.GetComponent<ExitDoor> ().locked)
						hit.collider.GetComponent<ExitDoor> ().OpenDoor ();
		}
	}
}