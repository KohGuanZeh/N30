using UnityEngine;

public class EmergencyAlarm : IInteractable
{
	public float duration;
	public Transform alarmPosition;
	public PatrollingAI[] affectedAis;
	AudioSource audioSource;
	UIManager uIManager;
	bool tutHasFinished;

	[Header ("For Mat Change")]
	[SerializeField] Renderer screenR;
	[SerializeField] Material screenMat;

	bool alarmed;

	public override void Start ()
	{
		base.Start ();
		alarmed = false;
		audioSource = GetComponent<AudioSource> ();
		uIManager = UIManager.inst;
		screenMat = screenR.material;
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
		MaterialUtils.ChangeMaterialEmission(screenMat, new Color(0.61f, 0.12f, 0.15f), 1);
		if (!tutHasFinished && uIManager.currentHint.gameObject.activeInHierarchy)
		{
			uIManager.currentHint.text = string.Empty;
			tutHasFinished = true;
		}
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
		MaterialUtils.ChangeMaterialEmission(screenMat, new Color(0.12f, 0.58f, 0.61f), 1);
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}
}