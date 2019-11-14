using UnityEngine;

public class EmergencyAlarm : IInteractable
{
	public float duration;
	public Transform alarmPosition;
	public PatrollingAI[] affectedAis;
	AudioSource[] audioSources;
	UIManager uIManager;
	bool tutHasFinished;

	[Header ("For Mat Change")]
	[SerializeField] Renderer[] screenRs;
	[SerializeField] Material[] screenMats;

	bool alarmed;

	public override void Start ()
	{
		base.Start ();
		alarmed = false;
		audioSources = GetComponents<AudioSource> ();
		uIManager = UIManager.inst;
		screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
	}

	public override void Interact ()
	{
		AlarmStartup ();
	}

	void AlarmStartup ()
	{
		if (alarmed || affectedAis[0].alarmed) //cheap check
			return;
		alarmed = true;
		audioSources[1].Play ();
		Invoke ("StartAlarm", audioSources[1].clip.length);
	}

	void StartAlarm ()
	{
		audioSources[0].Play ();
		MaterialUtils.ChangeMaterialsEmission(screenMats, new Color(2.118f, 0.519f, 0.476f), 1.9f);
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
		audioSources[0].Stop ();
		CancelInvoke ();
		MaterialUtils.ChangeMaterialsEmission(screenMats, new Color(0.47f, 2.00f, 2.11f), 1f);
		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}
}