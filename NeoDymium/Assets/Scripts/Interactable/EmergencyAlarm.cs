using UnityEngine;

public class EmergencyAlarm : IInteractable
{
	public float duration;
	public Transform alarmPosition;
	public PatrollingAI[] affectedAis;
	AudioSource audioSource;

	bool alarmed;

	public override void Start ()
	{
		base.Start ();
		alarmed = false;
		audioSource = GetComponent<AudioSource> ();
	}

	public override void Interact ()
	{
		StartAlarm ();
	}

	void StartAlarm ()
	{
		if (alarmed || affectedAis[0].alarmed) //cheap check
			return;
		audioSource.Play ();
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = true;
			ai.agent.isStopped = false;
			ai.alarmPos = alarmPosition.position;
			if (!ai.isInvincible)
				ai.agent.SetDestination (alarmPosition.position);
		}
		alarmed = true;
		Invoke ("EndAlarm", duration);
	}

	public void EndAlarm () 
	{
		alarmed = false;
		audioSource.Stop ();
		CancelInvoke ();
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}
}