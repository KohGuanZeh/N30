using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
	public float rotateSpeed;
	public bool clockwise;

	void Update ()
	{
		transform.eulerAngles += Vector3.forward * rotateSpeed * Time.deltaTime * (clockwise ? 1 : -1);
	}
}