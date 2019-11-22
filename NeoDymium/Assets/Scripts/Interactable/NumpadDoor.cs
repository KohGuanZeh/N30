using System;
using UnityEngine;
using TMPro;

public class NumpadDoor : IInteractable
{
	[Header("General Variables")]
	[SerializeField] Camera playerCam; //Store the Player Camera so as to Lerp it
	[SerializeField] Collider coll; //Store the Collider of the Numpad to prevent Raycast Error
	[SerializeField] NumpadButton[] buttons; //Store all the Numpad Buttons to activate and deactivate colliders to prevent problems
	[SerializeField] GameObject exitButton; //To uninteract
	[SerializeField] bool isInteracting;
	[SerializeField] bool unlocked;

	[Header("Door Related Variables")]
	[SerializeField] GameObject numpadDoor;
	[SerializeField] Animator numpadDoorAnim;

	[Header("For Password")]
	public string password;
	[SerializeField] string input;
	[SerializeField] TextMeshProUGUI inputText;

	[Header("For Password Generation")]
	public Transform[] deskPositions;
	public DeskInfo deskInfoTemplate;
	//for randoming of the personal info, age and password will be random
	public string[] possibleNames;
	public string[] possibleJobPositions; //dont include IT guy in array

	[Header("For Emission Change")]
	[SerializeField] Renderer[] emissiveRs;
	[SerializeField] Material[] emissiveMats;
	[SerializeField] Color lockedColor, unlockedColor; //For the Light Bars on Top
	[SerializeField] float lockedIntensity, unlockedIntensity;

	[Header("For Lerping")]
	[SerializeField] Transform camPos;
	[SerializeField] Vector3 playerCamRot; //Stores Player Cam Rotation before Interaction
	[SerializeField] float lerpTime;
	[SerializeField] float lerpSpeed = 1.5f;
	[SerializeField] Action action;

	public override void Start()
	{
		base.Start();
		playerCam = player.GetPlayerCamera();
		GeneratePassword();

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll = GetComponent<Collider>();
		buttons = GetComponentsInChildren<NumpadButton>();
		coll.enabled = true;
		numpadDoorAnim = GetComponent<Animator>();
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(false);
		whiteDot.SetActive (false);

		//Get Material to Change Emission
		emissiveMats = MaterialUtils.GetMaterialsFromRenderers(emissiveRs);
	}

	protected override void Update()
	{
		if (action != null) action();
		if (!isInteracting) return;

		if (Input.GetMouseButtonDown(0)) RaycastButton();
		if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)) //Update comes after Player Controller Update. Hence need to detect if it is the same frame Player interacts with Numpad
		{
			if (action != null) return;
			Uninteract();
		} 
	}

	void GeneratePassword()
	{
		int actualPasswordIndex = UnityEngine.Random.Range(0, deskPositions.Length);
		password = UnityEngine.Random.Range(0, 1000).ToString("000");

		for (int i = 0; i < deskPositions.Length; i++)
		{
			DeskInfo info = Instantiate(deskInfoTemplate, deskPositions[i].position, Quaternion.identity, deskPositions[i]);
			info.employeeName.text = "Name: " + possibleNames[UnityEngine.Random.Range(0, possibleNames.Length)];
			info.age.text = "Age: " + UnityEngine.Random.Range(0, 100).ToString();

			if (i == actualPasswordIndex)
			{
				info.jobPosition.text = "Job: IT Personnel";
				info.password.text = "Password: " + password;
			}
			else
			{
				info.jobPosition.text = "Job: " + possibleJobPositions[UnityEngine.Random.Range(0, possibleJobPositions.Length)];
				info.password.text = "Password: " + UnityEngine.Random.Range(0, 1000).ToString("000");
			}
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

		//Hide Player UI
		UIManager.inst.ShowHideUI(false);

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll.enabled = false;
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(true);

		//Show Exit Button
		exitButton.SetActive(true);

		player.headBob = false;
		player.inSpInteraction = isInteracting = true;
		if (playerCam == null) playerCam = player.GetPlayerCamera(); //Prevent Null Ref Errors
		playerCamRot = playerCam.transform.eulerAngles;
		action += LerpCameraToNumPad;
	}

	public override void Uninteract()
	{
		if (!unlocked)
		{
			Clear();
			inputText.text = "Input Passcode_";
		}

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		//Show Player UI
		UIManager.inst.ShowHideUI(true);

		//Set Active True and False for Colliders to prevent Raycast Errors
		coll.enabled = true;
		foreach (NumpadButton button in buttons) button.EnableDisableCollider(false);

		//Hide Exit Button
		exitButton.SetActive(false);

		isInteracting = false;
		if (playerCam == null) playerCam = player.GetPlayerCamera(); //Prevent Null Ref Errors
		action += LerpCameraToNumPad;
	}

	void OnUninteract()
	{
		player.headBob = true;
		player.inSpInteraction = false;
	}

	public override string GetError(int key = 0)
	{
		if (player.inHackable) return "AI cannot interact with this Object";
		else if (unlocked) return "Door is already Unlocked";
		else return string.Empty;
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
			playerCam.transform.eulerAngles = playerCamRot; //Cannot use new Vector3 (GetPitch(), 0, 0) as there will be one frame where it snaps to the opposite direction

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

	//For Animation Event
	public void SetDoorToOpen()
	{
		numpadDoorAnim.SetBool("Open", unlocked);
	}

	public void NumPadUnlock()
	{
		//Unlock Door?
		unlocked = true;
		inputText.text = "Unlocked_";
		numpadDoorAnim.SetBool("Unlocked", unlocked);
		soundManager.PlaySound (soundManager.numpadSuccess);
		ChangeEmissionColor();
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		//May want to change to Coroutine to add a Delay
		if (isInteracting) Uninteract();//If not Loading from Checkpoint and Player unlocked the Passcode
	}

	//For Checkpoint Purposes
	public void NumPadLock()
	{
		//Lock Door?
		Clear(); //Clear any previous inputs
		unlocked = false;
		numpadDoorAnim.SetBool("Opened", unlocked);
		ChangeEmissionColor(false);
	}

	public void AddNumberToInput(int num)
	{
		if (input.Length >= 3) return; //Passcode Length
		input = string.Concat(input, num);
		soundManager.PlaySound (soundManager.numpad[num]);
		inputText.text = input;
		print(string.Format("Pressed {0}", num));
	}

	public void Backspace()
	{
		if (input.Length == 0) return;
		input = input.Remove(input.Length-1);
		inputText.text = input;
	}

	public void Clear()
	{
		input = string.Empty;
		inputText.text = "Input Passcode_";
	}

	public void CheckPasscode()
	{
		if (IsCorrectPasscode()) NumPadUnlock();
		else
		{
			soundManager.PlaySound (soundManager.numpadFail);
			inputText.text = "Wrong Passcode_";
			input = string.Empty;
		}
	}

	bool IsCorrectPasscode()
	{
		return (input == password);
	}

	void ChangeEmissionColor(bool unlocked = true)
	{
		Color emissiveColor = unlocked ? unlockedColor : lockedColor;
		float intensity = unlocked ? unlockedIntensity : lockedIntensity;
		MaterialUtils.ChangeMaterialsEmission(emissiveMats, emissiveColor, intensity, "_EmissiveColor");
	}
}