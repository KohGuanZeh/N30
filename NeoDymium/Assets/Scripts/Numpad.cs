using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Numpad : IInteractable
{
	[Header ("General Variables")]
	[SerializeField] PlayerController player;
	[SerializeField] Camera playerCam; //Store the Player Camera so as to Lerp it
	[SerializeField] Collider coll; //Store the Collider of the Numpad to prevent Raycast Error
	[SerializeField] NumpadButton[] buttons; //Store all the Numpad Buttons to activate and deactivate colliders to prevent problems
	[SerializeField] bool isInteracting;
	[SerializeField] bool unlocked;

	[Header("For Passcode")]
	[SerializeField] string passcode;
	[SerializeField] string input;
	[SerializeField] TextMeshPro inputText;

	[Header ("For Lerping")]
	[SerializeField] Transform camPos;
	[SerializeField] Vector3 playerCamRot; //Stores Player Cam Rotation before Interaction
	[SerializeField] float lerpTime;
	[SerializeField] float lerpSpeed = 1.5f;
	[SerializeField] Action action;

	protected override void Start()
	{
		base.Start();
		player = PlayerController.inst;
		playerCam = player.GetPlayerCamera();

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll = GetComponent<Collider>();
		buttons = GetComponentsInChildren<NumpadButton>();
		coll.enabled = true;
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(false);
	}

	private void Update()
	{
		if (action != null) action();
		if (!isInteracting) return;

		if (Input.GetMouseButtonDown(0)) RaycastButton();
		if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1))
		{
			if (action != null) return;
			Uninteract();
		}
	}

	public override void TryInteract(ColorIdentifier userColor)
	{
		//Disallow Interaction with Numpad when in AI
		return;
	}

	public override void Interact()
	{
		if (unlocked) return;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll.enabled = false;
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(true);

		player.headBob = false;
		player.inSpInteraction = isInteracting = true;
		playerCamRot = playerCam.transform.eulerAngles;
		action += LerpCameraToNumPad;
	}

	public override void Uninteract()
	{
		if (!unlocked) inputText.text = "Enter Passcode";

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll.enabled = true;
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(false);

		isInteracting = false;
		action += LerpCameraToNumPad;
	}

	void OnUninteract()
	{
		player.headBob = true;
		player.inSpInteraction = false;
	}

	void LerpCameraToNumPad()
	{
		lerpTime = isInteracting ? Mathf.Min(lerpTime + Time.deltaTime * lerpSpeed, 1) : Mathf.Max(lerpTime - Time.deltaTime * lerpSpeed, 0);

		//May need to adjust this depending on the smoothness of the Lerp
		playerCam.transform.position = Vector3.Lerp(player.GetHeadRefTransform().position, camPos.position, lerpTime);
		playerCam.transform.eulerAngles = new Vector3(Mathf.LerpAngle(playerCamRot.x, camPos.eulerAngles.x, lerpTime),
									Mathf.LerpAngle(playerCamRot.y, camPos.eulerAngles.y, lerpTime),
									Mathf.LerpAngle(playerCamRot.z, camPos.eulerAngles.z, lerpTime));

		if (isInteracting && lerpTime >= 1)
		{
			playerCam.transform.position = camPos.position;
			playerCam.transform.eulerAngles = camPos.eulerAngles;

			action -= LerpCameraToNumPad;
		}
		else if (!isInteracting && lerpTime <= 0)
		{
			playerCam.transform.position = player.GetHeadRefTransform().position;
			playerCam.transform.eulerAngles = new Vector3(player.GetPitch(), 0, 0);

			OnUninteract();
			action -= LerpCameraToNumPad;
		}
	}

	void RaycastButton()
	{
		RaycastHit hit;
		Ray mouseRay = playerCam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(mouseRay, out hit, 10))
		{
			if (hit.collider != null)
			{
				//May want to Check by Tag instead...
				if (hit.collider.GetComponent<NumpadButton>()) hit.collider.GetComponent<NumpadButton>().InvokeButtonPress();
			}
		}
	}

	void NumPadUnlock()
	{
		//Unlock Door?
		unlocked = true;
		inputText.text = "Unlocked";
		//May want to change to Coroutine to add a Delay
		if (isInteracting) Uninteract();//If not Loading from Checkpoint and Player unlocked the Passcode
	}

	//For Checkpoint Purposes
	void NumPadLock()
	{
		unlocked = false;
		//Lock Door?
	}

	public void AddNumberToInput(int num)
	{
		if (input.Length > 5) return; //Passcode Length
		string.Concat(input, num);
		inputText.text = input;
	}

	public void Backspace()
	{
		if (input.Length == 0) return;
		input.Remove(input.Length - 1);
	}

	public void Clear()
	{
		input = string.Empty;
		inputText.text = "Enter Passcode";
	}

	public void CheckPasscode()
	{
		if (IsCorrectPasscode()) NumPadUnlock();
		else
		{
			inputText.text = "Wrong Passcode";
			input = string.Empty;
		}
	}

	bool IsCorrectPasscode()
	{
		return (input == passcode);
	}
}
