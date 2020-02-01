using UnityEngine;
using TMPro;

public class EmergencyAlarm : IInteractable
{
	public float duration;
	public Transform alarmPosition;
	public PatrollingAI[] affectedAis;
	public AudioSource[] audioSources;
	UIManager uIManager;
	bool tutHasFinished;

	[Header ("For Mat Change")]
	[SerializeField] Renderer[] screenRs;
	[SerializeField] Material[] screenMats;
	[SerializeField] Color defaultColor, alertColor;
	[SerializeField] float defaultIntensity, alertIntensity;
	[SerializeField] GameObject defaultHud, errorHud, passcodeUI;
	[SerializeField] Animator anim;
	[SerializeField] TextMeshProUGUI passcode, time;
	[SerializeField] double timer;

	[Header ("Tutorial")]
	public bool tutorialFirst = false;
	bool tutorialFirstDone = false;

	[Space (10)]

	bool alarmed;
	public bool active;

	EmergencyAlarm[] alarms;

	void Awake ()
	{
		audioSources = GetComponents<AudioSource> ();
	}

	public override void Start ()
	{
		base.Start ();
		alarmed = false;
		active = false;
		alarms = FindObjectsOfType<EmergencyAlarm> ();
		uIManager = UIManager.inst;

		timer = 13 + PlayerPrefs.GetFloat("Minutes Elapsed", 0);
		time.text = string.Format("{0}:{1}", (Mathf.FloorToInt((float)timer/60)).ToString("00"), Mathf.FloorToInt((float)timer % 60));

		//screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
	}

	protected override void Update()
	{
		base.Update();
		double totalTime = timer + (LoadingScreen.inst.GetTimeElapsed()/60);
		time.text = string.Format("{0}:{1}", (Mathf.FloorToInt((float) totalTime/60)).ToString("00"), Mathf.FloorToInt((float)totalTime % 60));
	}

	public override void Interact ()
	{
		AlarmStartup ();
	}

	void AlarmStartup ()
	{
		//bool passed = true;

		/*foreach (EmergencyAlarm alarm in alarms)
			if (alarm.active)
				passed = false;

		if (active)
			return;

		active = true;

		if (!passed)
		{
			active = false;
			return;
		}*/

		// if (alarmed || affectedAis[0].alarmed) //cheap check
		// 	return;

		active = true;

		alarmed = true;
		audioSources[1].Play ();
		passcodeUI.SetActive(true);
		anim.SetTrigger("Interact");
		Invoke ("StartAlarm", audioSources[1].clip.length);
	}

	void StartAlarm ()
	{
		if (tutorialFirst && !tutorialFirstDone)
		{
			tutorialFirstDone = true;
			NewTutorial.inst.TutorialEnd (1);
		}

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
				ai.reachedIdle = false;
				ai.firstIdle = true;
				//ai.StopCoroutine (ai.IdleLookAround ());
				ai.StopAllCoroutines();
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

	public override string GetError(int key = 0)
	{
		if (active) return "Error. Alarm is currently active. Please wait.";
		//for (int i = 0; i < alarms.Length; i++) if (alarms[i].active) return "Error. Alarm is Currently Active. Please Wait";
		return string.Empty;
	}
}
