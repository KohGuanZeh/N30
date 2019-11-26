using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorDoor : IInteractable
{
	[Header("General Elevator Properties")]
	[SerializeField] Animator anim;
	bool elevatorTriggered = false; //In case we want to allow Player to move int Elevator
	[SerializeField] bool isSpawnPoint;

	[Header("For Emissive Mat Change")]
	[SerializeField] Renderer r;
	[SerializeField] Material emissiveMat;
	[SerializeField] Color arrivedColor;
	[SerializeField] float arrivedIntensity;

	public override void Start()
	{
		base.Start();
		col = GetComponentInChildren<Collider>();
		anim = GetComponentInChildren<Animator>();
		emissiveMat = r.material;

		if (isSpawnPoint)
		{
			//To Prevent Interaction
			elevatorTriggered = true;
			gameObject.layer = 0; //Set Layer to Default
			gameObject.tag = "Untagged";

			soundManager.PlaySound(soundManager.elevatorTravel);
			StartCoroutine(OpenElevatorDoor());
		}
	}

	//Prevent AI Interaction
	public override void TryInteract(ColorIdentifier userColor)
	{
		return;
	}

	//Use a Coroutine if you want to add a Delay between Door Bell and Elevator Door Opening
	public override void Interact()
	{
		StartCoroutine(OpenElevatorDoor());
	}

	IEnumerator OpenElevatorDoor()
	{
		if (isSpawnPoint) yield return new WaitForSeconds(1);

		SetElevatorDoorAsArrived();

		//yield return new WaitUntil(() => soundPlayFinish);
		yield return new WaitForSeconds(0.5f);

		OpenCloseElevatorDoor();
	}

	IEnumerator TransitToNextLevel()
	{
		player.LockPlayerMovement(true); //Prevent Player from Moving upon hitting Elevator Trigger
		player.LockPlayerAction(true); //Prevent UI from showing up on what is Interactable etc.
		soundManager.PlaySound(soundManager.elevatorBell);

		yield return new WaitForSeconds(0.5f);

		OpenCloseElevatorDoor(false);

		//yield return new WaitUntil(() => soundPlayFinish);
		yield return new WaitForSeconds(1f);

		soundManager.PlaySound(soundManager.elevatorTravel);

		yield return new WaitForSeconds(1.5f);

		PlayerPrefs.DeleteKey ("Last Objective Saved");
		PlayerPrefs.DeleteKey (SceneManager.GetActiveScene().name + " Checkpoint");

		//LoadingScreen.inst.AutoLoadNextScene();
		OpenCloseElevatorDoor(true);
		player.LockPlayerMovement(false);
		player.LockPlayerAction(false);
	}

	void SetElevatorDoorAsArrived()
	{
		MaterialUtils.ChangeMaterialEmission(emissiveMat, arrivedColor, arrivedIntensity, "_EmissiveColor");
		soundManager.PlaySound(soundManager.elevatorBell);
	}

	void OpenCloseElevatorDoor(bool open = true)
	{
		anim.SetBool("Open", open);
		soundManager.PlaySound(soundManager.slidingDoor);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (elevatorTriggered) return;

		if (other.tag == "Player")
		{
			print("Working");
			elevatorTriggered = true;
			StartCoroutine(TransitToNextLevel());
		}
	}
}
