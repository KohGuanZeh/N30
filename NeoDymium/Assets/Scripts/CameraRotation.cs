using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
	public float rotateSpeed;
	public Transform player;

	void Start () 
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update () 
	{
		float mouseX = Input.GetAxis ("Mouse X") * rotateSpeed;
		float mouseY = Input.GetAxis ("Mouse Y") * rotateSpeed;

		mouseY = Mathf.Clamp (mouseY, -35, 60);

		transform.eulerAngles += new Vector3 (-mouseY, mouseX, 0);
		transform.position = player.transform.position;
	}
}