using UnityEngine;

public class temp : MonoBehaviour
{
	public PatrollingAI[] ais;

    void EndAlarm () 
	{
		foreach (PatrollingAI ai in ais)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}
}