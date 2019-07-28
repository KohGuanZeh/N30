using UnityEngine;

public class AIWall : MonoBehaviour
{
    public static AIWall inst;

	public Collider[] managerColliders;
	public Collider[] aiColliders;

	void Awake () 
	{
		inst = this;
	}

	public void IgnoreManager (Collider col) 
	{
		IgnoreAI (col);
		foreach (Collider coll in managerColliders)
			Physics.IgnoreCollision (col, coll, true);
	}

	public void IgnoreAI (Collider col) 
	{
		foreach (Collider coll in aiColliders)
			Physics.IgnoreCollision (col, coll, true);
	}
}