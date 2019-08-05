using UnityEngine;

public class EmergencyAlarm : IHackable
{
	public float duration;
	public Transform alarmPosition;

	PatrollingAI[] ais;

	float horLookSpeed = 1, vertLookSpeed = 1;
	float yaw, pitch; //Determines Camera and Player Rotation

	bool disabled = false;

	public override void OnHack ()
	{
		base.OnHack ();
		disabled = true;
		yaw = camera.transform.eulerAngles.y;
	}

	protected override void Start ()
	{
		base.Start ();
		ais = FindObjectsOfType<PatrollingAI> ();
	}

	protected override void Update ()
	{
		if (ui.isPaused || ui.isGameOver) return;
		if (!disabled) CatchPlayer();
		if (hacked) ExecuteHackingFunctionaliy();
	}

	/*public override void Disable ()
	{
		gameObject.layer = 0;
		camera.enabled = false;
		disabled = true;
	}*/

	protected override void ExecuteHackingFunctionaliy ()
	{
		PlayerRotation ();

		if (Input.GetKeyDown (key: KeyCode.E)) 
		{
			CancelInvoke ();
			StartAlarm ();
			Invoke ("EndAlarm", duration);
		}
	}

	void StartAlarm () 
	{
		foreach (PatrollingAI ai in ais)
		{
			ai.alarmed = true;
			ai.agent.SetDestination (alarmPosition.position);
		}
	}

	void EndAlarm () 
	{
		foreach (PatrollingAI ai in ais)
		{
			ai.alarmed = false;
			ai.sentBack = false;
		}
	}

	void PlayerRotation()
	{
		//Camera and Player Rotation
		yaw += horLookSpeed * Input.GetAxis("Mouse X");
		pitch -= vertLookSpeed * Input.GetAxis("Mouse Y"); //-Since 0-1 = 359 and 359 is rotation upwards;
		pitch = Mathf.Clamp(pitch, -90, 90); //Setting Angle Limits

		camera.transform.localEulerAngles = new Vector3(pitch, yaw, 0);
	}
}