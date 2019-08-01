using UnityEngine;

public class temp : MonoBehaviour
{
	void Update ()
	{
		if (Input.GetKeyDown (key: KeyCode.F))
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}
}