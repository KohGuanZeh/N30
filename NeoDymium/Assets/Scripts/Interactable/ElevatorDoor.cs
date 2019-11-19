using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : IInteractable
{
	[SerializeField] Animator doorAnim;

	public override void Start()
	{
		base.Start();
		doorAnim = GetComponentInChildren<Animator>();
	}

	//Prevent AI Interaction
	public override void TryInteract(ColorIdentifier userColor)
	{
		return;
	}

	public override void Interact()
	{
		soundManager.PlaySound(soundManager.elevatorBell);
		OpenCloseElevatorDoor();
	}

	public void TransitToNextLevel()
	{
		OpenCloseElevatorDoor(false);
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
