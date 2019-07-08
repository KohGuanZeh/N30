using UnityEngine;

public class temp : MonoBehaviour
{
	public Transform grenadePos;
    public float throwSpeed;
	public GameObject grenade;

	void Update () 
	{
		if (Input.GetKeyDown (key: KeyCode.G)) 
		{
			Rigidbody grenadeRb = Instantiate (grenade, grenadePos.position, Quaternion.identity).GetComponent<Rigidbody> ();
			//grenadeRb.velocity = [player.transform.forward] + new Vector3 (0, [player.transform.forward] * (throwSpeed - 1), 0);
		}
	}
}