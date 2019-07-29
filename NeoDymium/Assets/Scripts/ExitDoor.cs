using UnityEngine;

public class ExitDoor : MonoBehaviour
{
	public bool locked = true;

	void Start () 
	{
		locked = true;
	}

	public void OpenDoor () 
	{
		gameObject.SetActive (false);
	}
}