using UnityEngine;

public class EmergencyAlarm : IHackable
{
	public float duration;
	public Transform alarmPosition;

	PatrollingAI[] ais;

	float horLookSpeed = 1, vertLookSpeed = 1;
	float yaw, pitch; //Determines Camera and Player Rotation

	protected override void Start ()
	{
		base.Start ();
		ais = FindObjectsOfType<PatrollingAI> ();
	}

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