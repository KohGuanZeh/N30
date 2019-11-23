using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorDoor : IInteractable
{
	[SerializeField] Animator doorAnim;

	public override void Start()
	{
		base.Start();
		col = GetComponentInChildren<Collider>();
		//doorAnim = GetComponentInChildren<Animator>();
	}

	//Prevent AI Interaction
	public override void TryInteract(ColorIdentifier userColor)
	{
		return;
	}

	public override void Interact()
	{
		TransitToNextLevel();
		//soundManager.PlaySound(soundManager.elevatorBell);
		//OpenCloseElevatorDoor();
	}

	public void TransitToNextLevel()
	{
		//OpenCloseElevatorDoor(false);
		PlayerPrefs.DeleteKey ("Last Objective Saved");
		PlayerPrefs.DeleteKey (SceneManager.GetActiveScene().name + " Checkpoint");
		LoadingScreen.inst.AutoLoadNextScene();
	}

	public void OpenCloseElevatorDoor(bool open = true)
	{
		doorAnim.SetBool("Open", open);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") TransitToNextLevel();
	}
}
