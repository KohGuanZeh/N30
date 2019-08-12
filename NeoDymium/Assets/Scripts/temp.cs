using UnityEngine;

public class temp : MonoBehaviour
{
	public GameObject yesB0ss;

	void Start ()
	{
		yesB0ss.SetActive (false);
	}

	void Update ()
	{
		if (Input.GetKeyDown (key: KeyCode.F))
			Cursor.lockState = CursorLockMode.None;

		if (Input.GetKeyDown (key: KeyCode.T))
			yesB0ss.SetActive (!yesB0ss.activeSelf);
	}
}