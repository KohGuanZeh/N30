using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public struct ControlsInfo
{
	public bool show;
	public bool hasError;
	public bool isLerping;
	public float lerpTime;
	public float errorLerpTime;
	public Image icon;
	public TextMeshProUGUI text;
	public Image border;
}

public class UIManager : MonoBehaviour
{
	[Header("General Properties")]
	public static UIManager inst;
	[SerializeField] PlayerController player;
	[SerializeField] Animator guiAnim; //Stand in for now... Will think about how to better integrate Animations
	[SerializeField] Vector2 baseResolution, currentResolution;

	[Header("Menus")]
	[SerializeField] RectTransform pauseScreen;
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform gameOverScreen;

	[Header("UI Templates")]
	public GameObject playerUI;
	public GameObject cctvUI;
	public GameObject controlsGrp; //Stores the Graphics for Controls and Errors

	[Header("CCTV UI Items")]
	public bool uiFadeIn;
	[SerializeField] bool uiFadeInProgress;
	[SerializeField] float uiLerpTime;
	[SerializeField] Image[] cctvCrosshair;
	[SerializeField] Image cctvUIBorder;
	[SerializeField] Image[] otherIcons;
	[SerializeField] TextMeshProUGUI date;
	[SerializeField] TextMeshProUGUI hackableName;

	[Header("Crosshair")]
	[SerializeField] bool isFocusing; //Check if a Hackable or Interactable Object has been focused on
	[SerializeField] Image[] rings; //For Rotation of Rings. 0 is Inner, 1 is Middle, 2 is Outer
	[SerializeField] float[] ringRotSpeeds; //For Rotation of Rings
	[SerializeField] float crosshairLerpTime;
	[SerializeField] bool crosshairIsLerping; //Check if the Focus Animation is Ongoing

	[Header("Objective Marker")]
	[SerializeField] Image marker;
	[SerializeField] Transform objective;
	[SerializeField] float offset; //Offset for Min Max XY
	[SerializeField] Vector2 minXY, maxXY;
	[SerializeField] TextMeshProUGUI distanceToObj;

	[Header("Stealth Gauge")]
	public RectTransform mainPointer; //Pointer to Instantiate
	public List<RectTransform> detectedPointers; //To Point to where Player is detected from. Only problem that has not been fixed is instantiating when not enough pointers... (Can be Coded in Optimisation)
	[SerializeField] Image stealthGauge;
	[SerializeField] Image detectedWarning;
	[SerializeField] float warningTime;

	[Header("Instructions and Error Msgs")]
	[SerializeField] Sprite[] controlsSprites; //Mouse Click is 0, E is 1
	[SerializeField] ControlsInfo[] controls;

	[SerializeField] bool errorFadeIn; //Checks if Error is Fading in
	[SerializeField] TextMeshProUGUI errorMsg;
	[SerializeField] float errorLerpTime;

	[Header("Game States")]
	//May want to use Enum for Game States
	public bool isGameOver;
	public bool isPaused;

	public Color disabledUIColor = new Color(0.8f, 0.8f, 0.8f, 0.75f);
	public Action action;

	[Header("White Dots")]
	public GameObject whiteDot;
	public RectTransform whiteDotHolder;

	private void Awake()
	{
		inst = this;
		baseResolution = new Vector2(1920, 1080);
		currentResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

		for (int i = 0; i < 10; i++)
		{
			if (i == 0) detectedPointers.Add(mainPointer);
			else detectedPointers.Add(Instantiate(mainPointer, mainPointer.transform.parent));

			detectedPointers[i].gameObject.SetActive(false);
		}

		//Set Min Max XY for Waypoint Pos
		minXY = new Vector2(marker.GetPixelAdjustedRect().width / 2 + offset, marker.GetPixelAdjustedRect().height / 2 + offset);
		maxXY = new Vector2(Screen.width - minXY.x, Screen.height - minXY.y);
	}

	private void Start()
	{
		player = PlayerController.inst;

		//Set Rings to Correct Local Scale First Before Game Start
		rings[0].rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		rings[2].rectTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		rings[0].gameObject.SetActive(false);
		rings[2].gameObject.SetActive(false);
		action += RotateRings;

		action += FlashDetectedWarning;
		detectedWarning.color = new Color(detectedWarning.color.r, detectedWarning.color.g, detectedWarning.color.b, 0);

		//CCTV UI Start Color and Anchored Positions
		cctvUIBorder.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		cctvUIBorder.color = Color.clear;
		foreach (Image icon in otherIcons)
		{
			icon.rectTransform.anchoredPosition = new Vector2(icon.rectTransform.anchoredPosition.x, -260);
			icon.color = Color.clear;
		}
		foreach (Image crosshair in cctvCrosshair) crosshair.color = Color.clear;
		date.rectTransform.anchoredPosition = new Vector2(date.rectTransform.anchoredPosition.x, -260);
		date.color = Color.clear;

		hackableName.rectTransform.anchoredPosition = new Vector2(hackableName.rectTransform.anchoredPosition.x, 260);
		hackableName.color = Color.clear;

		//Set Color and Border of Control Infos
		Color startColor = Color.clear;
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i].border.fillAmount = 0;
			controls[i].text.color = startColor;
			controls[i].icon.color = startColor;
		}

		errorMsg.color = Color.clear;

		action += LerpInstructions;
		//action += LerpActionAvailability;
	}

	void Update()
	{
		PointToObjective();
		stealthGauge.fillAmount = (player.stealthGauge / player.stealthThreshold);

		if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver) PausePlay();

		if (action != null) action();
	}

	public void GameOver()
	{
		isGameOver = true;
		Time.timeScale = 0;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		//Closes all other Screens
		pauseScreen.gameObject.SetActive(false);
		optionsScreen.gameObject.SetActive(false);

		gameOverScreen.gameObject.SetActive(true);
	}

	public void Focus(bool playerIsFocusing)
	{
		if (playerIsFocusing && !isFocusing) isFocusing = true;
		else if (!playerIsFocusing && isFocusing)
		{
			isFocusing = false;
			for (int i = 0; i < controls.Length; i++) controls[i].show = false;
		}
		else return;

		if (crosshairIsLerping) return;

		rings[0].gameObject.SetActive(true);
		rings[2].gameObject.SetActive(true);
		crosshairIsLerping = true;
		action += LerpFocusFeedback;
	}

	public void DisplayInstructionsAndErrors(bool isHackableObj, bool[] hasErrors, bool haveSecondary = false)
	{
		if (isHackableObj)
		{
			//print(string.Format("Show Secondary: {0}", haveSecondary));

			controls[0].show = true;
			controls[0].hasError = hasErrors[0];
			controls[0].icon.sprite = controlsSprites[0];
			controls[0].text.text = "Hack";

			controls[1].show = haveSecondary;
			if (haveSecondary)
			{
				controls[1].hasError = hasErrors[1];
				controls[1].icon.sprite = controlsSprites[1];
				controls[1].text.text = "Wipe Memory";
			}
		}
		else
		{
			controls[0].show = true;
			controls[0].hasError = hasErrors[0];
			controls[0].icon.sprite = controlsSprites[1];
			controls[0].text.text = "Interact";

			controls[1].show = false;
		}
	}

	public void LocateHackable(IHackable hackable, RectTransform pointer)
	{
		//Rotation is Correct
		Vector3 toPosition = hackable.transform.position;
		Vector3 fromPosition = player.transform.position;

		Vector3 horDir = (new Vector3(toPosition.x, 0, toPosition.z) - new Vector3(fromPosition.x, 0, fromPosition.z)).normalized;
		Vector3 forward = player.GetPlayerCamera().transform.forward;

		float horAngle = Vector3.SignedAngle(new Vector3(forward.x, 0, forward.z).normalized, horDir, Vector3.up); //Not Sure why this Works
		pointer.eulerAngles = new Vector3(0, 0, -horAngle); //Set Rotation of Player Pointer to Point at Player
	}

	public void StartUILerp(bool fadeIn)
	{
		action -= LerpUITemplate; //Remove to prevent Errors

		uiFadeIn = fadeIn;
		uiFadeInProgress = true;
		action += LerpUITemplate;
	}

	//Display Error Upon Button Press
	public void DisplayError(string error = "")
	{
		if (error == string.Empty) return;

		action -= ErrorFadeInFadeOut;
		errorMsg.color = Color.clear;
		errorMsg.text = error;
		errorFadeIn = true;
		errorLerpTime = 0;
		action += ErrorFadeInFadeOut;
	}

	public void ShowHideUI(bool show)
	{
		playerUI.gameObject.SetActive(show);
		cctvUI.gameObject.SetActive(show);
		marker.gameObject.SetActive(show); //Show Hide Obj Marker
		controlsGrp.SetActive(show); //Show Hide Controls
		whiteDotHolder.gameObject.SetActive(show); //Show Hide White Dots
	}

	#region Objective Marker Functions
	void PointToObjective()
	{
		if (!objective) return;

		Vector3 objPos = objective.position;
		Vector2 objScreenPos = player.CurrentViewingCamera.WorldToScreenPoint(objPos);

		//Distance from Player to Objective
		int dist = Mathf.RoundToInt((player.transform.position - objPos).magnitude);

		distanceToObj.text = dist.ToString() + "m";
		
		// //Check if Objective is in front or behind of Player (any body that the Player is in)
		// Vector3 dirToObj = (objPos - player.CurrentViewingCamera.transform.position).normalized;
		// //If Objective is behind of where Player (any body that the Player is in) is at
		// if (Vector3.Dot(player.CurrentViewingCamera.transform.forward, dirToObj) < 0)
		// {
		// 	//If Object is on the Right side of the Player, Clamp it to the LEFT (Since Player is facing behind) and vice versa
		// 	if (objScreenPos.x > Screen.width / 2) objScreenPos.x = minXY.x;
		// 	else objScreenPos.x = maxXY.x;
		// }

		// //Clamp to prevent Marker from going Offscreen
		// objScreenPos.x = Mathf.Clamp(objScreenPos.x, minXY.x, maxXY.x);
		// objScreenPos.y = Mathf.Clamp(objScreenPos.y, minXY.y, maxXY.y);

		Vector3 dirToObj = (objPos - player.CurrentViewingCamera.transform.position).normalized;

		if (Vector3.Dot(player.CurrentViewingCamera.transform.forward, dirToObj) < 0)
		{
			marker.gameObject.SetActive (false);
			distanceToObj.gameObject.SetActive (false);
		}
		else 
		{
			marker.gameObject.SetActive (true);
			distanceToObj.gameObject.SetActive (true);
		}

		marker.transform.position = objScreenPos;
		distanceToObj.transform.position = objScreenPos;
	}

	void ShowMarker()
	{
		marker.gameObject.SetActive(true);
	}

	void HideMarker()
	{
		marker.gameObject.SetActive(false);
	}

	void ClearObjective()
	{
		objective = null;
	}

	// I put this to public - Nigel
	public void SetNewObjective(Transform newObjective)
	{
		objective = newObjective;
		ShowMarker();
	}
	#endregion

	#region GUI Animations
	void RotateRings()
	{
		rings[1].rectTransform.Rotate(0, 0, ringRotSpeeds[1] * Time.deltaTime);

		if (!rings[0].gameObject.activeInHierarchy) return;

		rings[0].rectTransform.Rotate(0, 0, ringRotSpeeds[0] * Time.deltaTime);
		rings[2].rectTransform.Rotate(0, 0, ringRotSpeeds[2] * Time.deltaTime);
	}

	void LerpFocusFeedback()
	{
		crosshairLerpTime = isFocusing ? Mathf.Min(crosshairLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(crosshairLerpTime - Time.deltaTime * 5, 0);
		rings[0].rectTransform.localScale = Vector3.Lerp(new Vector3(1.25f, 1.25f, 1.25f), Vector3.one, crosshairLerpTime);
		rings[2].rectTransform.localScale = Vector3.Lerp(new Vector3(0.75f, 0.75f, 0.75f), Vector3.one, crosshairLerpTime);

		if (crosshairLerpTime >= 1 && isFocusing)
		{
			crosshairLerpTime = 1;
			rings[0].rectTransform.localScale = Vector3.one;
			rings[2].rectTransform.localScale = Vector3.one;

			crosshairIsLerping = false;
			action -= LerpFocusFeedback;
		}
		else if (crosshairLerpTime <= 0 && !isFocusing)
		{
			crosshairLerpTime = 0;
			rings[0].rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
			rings[2].rectTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

			rings[0].gameObject.SetActive(false);
			rings[2].gameObject.SetActive(false);

			crosshairIsLerping = false;
			action -= LerpFocusFeedback;
		}
	}

	void FlashDetectedWarning()
	{
		if (warningTime == 0 && !player.isDetected) return;

		float alpha = 0;
		Color newColor = detectedWarning.color;

		if (player.isDetected)
		{
			warningTime += Time.deltaTime * 3;
			alpha = Mathf.PingPong(warningTime, 1);
		}
		else warningTime = 0;

		newColor.a = alpha;
		detectedWarning.color = newColor;
	}

	void ErrorFadeInFadeOut()
	{
		errorLerpTime = errorFadeIn ? Mathf.Min(errorLerpTime + Time.deltaTime * 1.5f, 1) : Mathf.Max(errorLerpTime - Time.deltaTime * 1.5f, 0);

		//Appear within the first quarter and start to disappear on the last quarter of the lerp
		errorMsg.color = Color.Lerp(Color.clear, Color.red, Mathf.Clamp(errorLerpTime/0.25f, 0, 1));

		if (errorLerpTime >= 1 && errorFadeIn)
		{
			errorMsg.color = Color.red;
			errorFadeIn = false;
		}
		else if (errorLerpTime <= 0 && !errorFadeIn)
		{
			errorMsg.color = Color.clear;
			action -= ErrorFadeInFadeOut;
		}
	}

	void LerpUITemplate()
	{
		uiLerpTime = uiFadeIn ? Mathf.Min(uiLerpTime + Time.deltaTime * 3f, 1) : Mathf.Max(uiLerpTime - Time.deltaTime * 3f, 0);

		cctvUIBorder.rectTransform.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1.5f), Vector3.one, Mathf.Clamp(uiLerpTime/0.5f, 0, 1));
		cctvUIBorder.color = Color.Lerp(Color.clear, Color.white, uiLerpTime);

		foreach (Image icon in otherIcons)
		{
			icon.rectTransform.anchoredPosition = new Vector2(icon.rectTransform.anchoredPosition.x, Mathf.Lerp(-260, -238, Mathf.Clamp((uiLerpTime - 0.25f)/0.75f, 0, 1)));
			icon.color = Color.Lerp(Color.clear, Color.white, uiLerpTime);
		}

		foreach (Image crosshair in cctvCrosshair) crosshair.color = Color.Lerp(Color.clear, Color.white, Mathf.Clamp((uiLerpTime - 0.25f) / 0.75f, 0, 1));

		date.rectTransform.anchoredPosition = new Vector2(date.rectTransform.anchoredPosition.x, Mathf.Lerp(-260, -237, Mathf.Clamp((uiLerpTime - 0.25f)/0.75f, 0, 1)));
		date.color = Color.Lerp(Color.clear, Color.white, uiLerpTime);

		hackableName.rectTransform.anchoredPosition = new Vector2(hackableName.rectTransform.anchoredPosition.x, Mathf.Lerp(260, 237, Mathf.Clamp((uiLerpTime - 0.25f)/0.75f, 0, 1)));
		hackableName.color = Color.Lerp(Color.clear, Color.white, uiLerpTime);

		if (uiLerpTime >= 1 && uiFadeIn)
		{
			cctvUIBorder.rectTransform.localScale = Vector3.one;
			cctvUIBorder.color = Color.white;

			foreach (Image icon in otherIcons)
			{
				icon.rectTransform.anchoredPosition = new Vector2(icon.rectTransform.anchoredPosition.x, -238);
				icon.color = Color.white;
			}
			foreach (Image crosshair in cctvCrosshair) crosshair.color = Color.white;

			date.rectTransform.anchoredPosition = new Vector2(date.rectTransform.anchoredPosition.x, -237);
			date.color = Color.white;

			hackableName.rectTransform.anchoredPosition = new Vector2(hackableName.rectTransform.anchoredPosition.x, 237);
			hackableName.color = Color.white;

			uiFadeInProgress = false;
			action -= LerpUITemplate;
		}
		else if (uiLerpTime <= 0 && !uiFadeIn)
		{
			cctvUIBorder.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			cctvUIBorder.color = Color.clear;

			foreach (Image icon in otherIcons)
			{
				icon.rectTransform.anchoredPosition = new Vector2(icon.rectTransform.anchoredPosition.x, -260);
				icon.color = Color.clear;
			}

			foreach (Image crosshair in cctvCrosshair) crosshair.color = Color.clear;

			date.rectTransform.anchoredPosition = new Vector2(date.rectTransform.anchoredPosition.x, -260);
			date.color = Color.clear;

			hackableName.rectTransform.anchoredPosition = new Vector2(hackableName.rectTransform.anchoredPosition.x, 260);
			hackableName.color = Color.clear;

			uiFadeInProgress = false;
			action -= LerpUITemplate;
		}
	}

	void LerpInstructions()
	{
		for (int i = 0; i < controls.Length; i++)
		{
			//Hence UI must be fading OR controls are still lerping
			//Conditions. UI is not fading AND [Controls lerp time is >=1 when show is true OR Controls Lerp time is <= 0 when show is false]
			//Condition. UI is not fading AND (Error lerp time is >= 1 when error is true and when show is true OR error lerp time <= 0 when error is false)
			bool ignoreLerp = false;
			bool ignoreErrorLerp = false;

			if (!uiFadeInProgress)
			{
				if ((controls[i].lerpTime >= 1 && controls[i].show) || (controls[i].lerpTime <= 0 && !controls[i].show)) ignoreLerp = true;
				if ((controls[i].errorLerpTime >= 1 && controls[i].hasError && controls[i].show) || (controls[i].errorLerpTime <= 0 && !controls[i].hasError)) ignoreErrorLerp = true;
			}	

			if (!ignoreLerp)
			{
				controls[i].isLerping = true;
				if (uiFadeInProgress) controls[i].show = false;
				controls[i].lerpTime = controls[i].show ? Mathf.Min(controls[i].lerpTime + Time.deltaTime * 4.5f, 1) : Mathf.Max(controls[i].lerpTime - Time.deltaTime * 4.5f, 0);

				controls[i].border.fillAmount = Mathf.Clamp(controls[i].lerpTime / 0.5f, 0, 1);

				Color controlsColor = Color.Lerp(Color.clear, controls[i].hasError ? disabledUIColor : Color.white, Mathf.Clamp((controls[i].lerpTime - 0.25f) / 0.75f, 0, 1));
				controls[i].text.color = controlsColor;
				controls[i].icon.color = controlsColor;

				if (controls[i].lerpTime >= 1 && controls[i].show)
				{
					controlsColor = controls[i].hasError ? disabledUIColor : Color.white;
					controls[i].text.color = controlsColor;
					controls[i].icon.color = controlsColor;
					controls[i].border.fillAmount = 1;
					controls[i].isLerping = false;
				}
				else if (controls[i].lerpTime <= 0 && !controls[i].show)
				{
					controlsColor = Color.clear;
					controls[i].text.color = controlsColor;
					controls[i].icon.color = controlsColor;
					controls[i].border.fillAmount = 0;
					controls[i].isLerping = false;
				}
			}

			if (!ignoreErrorLerp)
			{
				if ((!controls[i].show && controls[i].hasError)) controls[i].hasError = false; //Turn Error back into false if Focus is lost
				controls[i].errorLerpTime = controls[i].hasError ? Mathf.Min(controls[i].errorLerpTime + Time.deltaTime * 4.5f, 1) : Mathf.Max(controls[i].errorLerpTime - Time.deltaTime * 4.5f, 0);

				if (!controls[i].isLerping && controls[i].show) //Only change Color when Controls are not Lerping and when they are being shown
				{
					Color controlsColor = Color.Lerp(Color.white, disabledUIColor, controls[i].errorLerpTime);
					controls[i].text.color = controlsColor;
					controls[i].icon.color = controlsColor;
				}

				if (controls[i].errorLerpTime >= 1 && controls[i].hasError)
				{
					if (!controls[i].isLerping)
					{
						controls[i].text.color = disabledUIColor;
						controls[i].icon.color = disabledUIColor;
					}

				}
				else if (controls[i].errorLerpTime <= 0 && !controls[i].hasError)
				{
					if (!controls[i].isLerping && controls[i].show)
					{
						controls[i].text.color = Color.white;
						controls[i].icon.color = Color.white;
					}
				}
			}
		}
	}

	//Separate from Lerp Instructions. Old Error Lerp. May be used instead of combining them for clarity sake
	void LerpActionAvailability()
	{
		for (int i = 0; i < controls.Length; i++)
		{
			if (!uiFadeInProgress)
				if ((controls[i].errorLerpTime >= 1 && controls[i].hasError && controls[i].show) || (controls[i].errorLerpTime <= 0 && !controls[i].hasError)) continue;

			if ((!controls[i].show && controls[i].hasError)) controls[i].hasError = false;
			controls[i].errorLerpTime = controls[i].hasError ? Mathf.Min(controls[i].errorLerpTime + Time.deltaTime * 4.5f, 1) : Mathf.Max(controls[i].errorLerpTime - Time.deltaTime * 4.5f, 0);

			if (!controls[i].isLerping && controls[i].show)
			{
				Color controlsColor = Color.Lerp(Color.white, disabledUIColor, controls[i].errorLerpTime);
				controls[i].text.color = controlsColor;
				controls[i].icon.color = controlsColor;
			}

			if (controls[i].errorLerpTime >= 1 && controls[i].hasError)
			{
				if (!controls[i].isLerping)
				{
					controls[i].text.color = disabledUIColor;
					controls[i].icon.color = disabledUIColor;
				}

			}
			else if (controls[i].errorLerpTime <= 0 && !controls[i].hasError)
			{
				if (!controls[i].isLerping && controls[i].show)
				{
					controls[i].text.color = Color.white;
					controls[i].icon.color = Color.white;
				}
			}
		}
	}
	#endregion

	#region Animation Events
	public void ShowStaticScreen()
	{
		guiAnim.SetTrigger("Static");
	}

	public void StaticScreenAnimEvent()
	{
		player.ForcedUnhackAnimEvent();
	}
	#endregion

	#region Button Functions
	public void PausePlay ()
	{
		isPaused = !isPaused;
		optionsScreen.gameObject.SetActive(false); //If Players Press the Esc Key when in the Options Menu. Unless you want to disable use of Shortcut Keys when in Pause
		pauseScreen.gameObject.SetActive(isPaused);
		Cursor.visible = isPaused;
		Cursor.lockState = isPaused ?  CursorLockMode.None : CursorLockMode.Locked;
		Time.timeScale = isPaused ? 0 : 1;
	}

	public void Options()
	{
		optionsScreen.gameObject.SetActive(!optionsScreen.gameObject.activeSelf);
	}

	public void MainMenu ()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene ("Main Menu", LoadSceneMode.Single);
	}

	public void LoadCheckpoint()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}

	public void Restart()
	{
		//Need a Proper Respawn
		Time.timeScale = 1;
		PlayerPrefs.DeleteKey("Checkpoint");
		SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}
	#endregion
}
