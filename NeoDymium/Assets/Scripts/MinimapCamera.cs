using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
	public static MinimapCamera inst;
    public Transform target;

	void Awake () 
	{
		inst = this;
	}

	void Update () 
	{
		if (target != null) 
			transform.position = new Vector3 (target.transform.position.x, transform.position.y, target.transform.position.z);
	}

	public void ChangeTarget (Transform newTarget)
	{
		target = newTarget;
	}
}