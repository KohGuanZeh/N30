using UnityEngine;

public class Spawner : MonoBehaviour
{
	public GameObject objToSpawn;
	public float delay = 1;
	public Transform spawnPos;

	void Start () 
	{
		InvokeRepeating ("Spawn", delay, delay);
	}

	void Spawn () 
	{
		Instantiate (objToSpawn, spawnPos.position, Quaternion.identity);
	}
}