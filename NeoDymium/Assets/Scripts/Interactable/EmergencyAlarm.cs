using UnityEngine;
using TMPro;

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
	[SerializeField] Color defaultColor, alertColor;
	[SerializeField] float defaultIntensity, alertIntensity;
	[SerializeField] GameObject defaultHud, errorHud, passcodeUI;
	[SerializeField] Animator anim;
	[SerializeField] TextMeshProUGUI passcode;

	bool alarmed;
	public bool active;

	EmergencyAlarm[] alarms;

	public override void Start ()
	{
		base.Start ();
		alarmed = false;
		active = false;
		alarms = FindObjectsOfType<EmergencyAlarm> ();
		audioSources = GetComponents<AudioSource> ();
		uIManager = UIManager.inst;

		//screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
	}

	public override void Interact ()
	{
		AlarmStartup ();
	}

	void AlarmStartup ()
	{
		bool passed = true;
		
		foreach (EmergencyAlarm alarm in alarms)
			if (alarm.active)
				passed = false;

		if (active)
			return;

		active = true;

		if (!passed)
		{
			active = false;
			return;
		}
			
		// if (alarmed || affectedAis[0].alarmed) //cheap check
		// 	return;

		alarmed = true;
		audioSources[1].Play ();
		passcodeUI.SetActive(true);
		anim.SetTrigger("Interact");
		Invoke ("StartAlarm", audioSources[1].clip.length);
	}

	void StartAlarm ()
	{
		audioSources[0].Play ();

		//MaterialUtils.ChangeMaterialsEmission(screenMats, alertColor, alertIntensity, "_EmissiveColor");
		defaultHud.SetActive(false);
		errorHud.SetActive(true);
		anim.SetBool("Error", true);

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
			{
				ai.agent.SetDestination (alarmPosition.position);
				ai.ResetHeadRotation ();
				ai.idleRotation = true;
				ai.StopCoroutine (ai.IdleLookAround ());
			}
				
		}
		alarmed = true;
		Invoke ("EndAlarm", duration);
	}

	public void EndAlarm ()
	{
		alarmed = false;
		active = false;
		audioSources[0].Stop ();
		CancelInvoke ();

		//MaterialUtils.ChangeMaterialsEmission(screenMats, defaultColor, defaultIntensity, "_EmissiveColor");

		defaultHud.SetActive(true);
		errorHud.SetActive(false);
		passcodeUI.SetActive(false);
		passcode.text = string.Empty;
		anim.SetBool("Error", false);

		foreach (PatrollingAI ai in affectedAis)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}

	void AddAsterisk()
	{
		passcode.text += "*";
	}

	void FlashError()
	{

	}

	public override string GetError(int key = 0)
	{
		for (int i = 0; i < alarms.Length; i++) if (alarms[i].active) return "Error. An Alarm is still going off.";
		return string.Empty;
	}
}
