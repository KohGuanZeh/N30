using UnityEngine;

public class NigelPervertOpenTightsInPublic : MonoBehaviour
{
	public float rotationSpeed = 4;
	public float boingSpeed = 1;
	public float boingHeight = 1;

	Vector3 startingPos;

	void Start ()
	{
		startingPos = transform.localPosition;
	}

    void Update ()
	{
		transform.localPosition = startingPos + boingHeight * Vector3.up * Mathf.Sin (boingSpeed * Time.time);
		transform.eulerAngles += Vector3.up * Time.deltaTime * rotationSpeed;
	}
}