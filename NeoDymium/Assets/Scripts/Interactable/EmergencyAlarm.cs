using UnityEngine;

public class EmergencyAlarm : IInteractable
{
	public float duration;
	public Transform alarmPosition;
	public PatrollingAI[] affectedAis;

	public override void Interact ()
	{
		print (gameObject.name);
		StartAlarm ();
	}

	void StartAlarm ()
	{
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = true;
			ai.agent.SetDestination (alarmPosition.position);
		}
		Invoke ("EndAlarm", duration);
	}

	public void EndAlarm () 
	{
		CancelInvoke ();
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}
}