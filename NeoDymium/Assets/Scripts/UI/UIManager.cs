using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public struct ControlsInfo
{
	public bool show; //Check if it should be Visible
	public bool hasError; //Check if Current Action can be Executed
	public bool isLerping; //Check if Current UI Element is Lerping
	public float lerpTime; //Lerp Time of Current UI Element
	public float errorLerpTime; //Lerp Time for Showing Error of Current UI (From Full White to Disabled Color)
	public Image icon; //Icon of the Controls
	public Image backdrop; //Backdrop for Controls
	public TextMeshProUGUI text; //Text of the Controls
	public Image border; //The Line
}

public class UIManager : MonoBehaviour
{
	[Header("General Properties")]
	public static UIManager inst;
	[SerializeField] PlayerController player;
	[SerializeField] ColorIdentifier currentColor = ColorIdentifier.none;
	[SerializeField] Animator guiAnim; 
	[SerializeField] Vector2 screenSize;
	[SerializeField] Resolution baseRes, currentRes;

	[Header("Menus")]
	[SerializeField] RectTransform pauseScreen;
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform gameOverScreen;

	[Header ("For UI Lerp")]
	public bool uiFadeIn;
	[SerializeField] HackableType currentType, prevType;
	[SerializeField] bool uiFadeInProgress;
	[SerializeField] float uiLerpTime;

	[Header("UI Templates")]
	public GameObject playerUI;
	public GameObject cctvUI;
	public GameObject aiUI;
	public GameObject hackableUI; //All Common UI for Hackables
	public GameObject controlsGrp; //Stores the Graphics for Controls and Errors

	[Header("Player UI Items")]
	[SerializeField] Graphic[] playerUIElements;

	[Header("CCTV UI Items")]
	[SerializeField] Image[] cctvUIBorders;

	[Header("AI UI Items")]
	[SerializeField] Graphic[] aiUIElements;
	[SerializeField] GameObject dottedItems;

	[Header("Common UI Elements")]
	[SerializeField] Graphic[] unhackInstructions; //For now Store the Unhack Instructions as a Graphic
	[SerializeField] Color[] unhackGraphicsColor; //Store Default Colors for the Unhack Instructions 
	[SerializeField] TextMeshProUGUI hackableName;
	[SerializeField] Graphic[] aiDetectionElements;

	[Header("Crosshair")]
	[SerializeField] bool isFocusing; //Check if a Hackable or Interactable Object has been focused on
	[SerializeField] Image[] crosshairs; //Stores the Unfilled and Filled Dot
	[SerializeField] float crosshairLerpTime;
	[SerializeField] bool crosshairIsLerping; //Check if the Focus Animation is Ongoing

	[Header("Objective Marker")]
	[SerializeField] Image marker;
	public Vector3 objective;
	[SerializeField] Vector2 baseOffset = new Vector2(100, 100);
	[SerializeField] Vector2 offset;
	[SerializeField] Vector2 minXY, maxXY;
	[SerializeField] TextMeshProUGUI distanceToObj;

	[Header("Player Detection Gauge")]
	public RectTransform mainPointer; //Pointer to Instantiate
	public List<RectTransform> detectedPointers; //To Point to where Player is detected from. Only problem that has not been fixed is instantiating when not enough pointers... (Can be Coded in Optimisation)
	[SerializeField] RectTransform playerPointer; //Store the Pointer that Points to where the Player is at (When Player is being Detected from AI)
	[SerializeField] Image[] detectionGauges; //[0] is Player, [1] is for CCTV/AI
	[SerializeField] Image[] detectionGaugeBackdrops; //[0] is Player, [1] is for CCTV/AI
	[SerializeField] Image[] flashingGauges; //[0] is Player, [1] is CCTV/AI
	[SerializeField] Image[] detectedAlerts; //[0] is for Player, [1] is for CCTV/AI
	[SerializeField] RectTransform movableDetectionComp;//Parent Holder for CCTV/AI Detection Gauges
	[SerializeField] float warningTime, gaugeFlashTime;
	[SerializeField] bool showGaugeBackdrop;

	[Header("AI Focus Component")]
	[SerializeField] RectTransform aiFocusGrp;
	[SerializeField] Image[] aiFocusRings;
	[SerializeField] float[] rotationSpeeds;
	[SerializeField] bool playerOnScreen;
	[SerializeField] float aiFocusLerpTime;

	[Header("Focus Controls")]
	[SerializeField] Sprite[] controlsSprites; //Mouse Click is 0, E is 1
	[SerializeField] ControlsInfo[] controls;

	[Header("Error Pop Up")]
	[SerializeField] RectTransform errorWindow;
	[SerializeField] Graphic[] errorBorders;
	[SerializeField] TextMeshProUGUI errorHeader;
	[SerializeField] RectTransform errorTxtBox;
	[SerializeField] bool showError;
	[SerializeField] float errorLerpTime;

	[SerializeField] TextMeshProUGUI errorContent;
	[SerializeField] float errorTextLerpTime, errorShowTime;
	[SerializeField] bool errorIsShowing;

	[Header("Tutorial Pop Up")]
	[SerializeField] RectTransform tutorialWindow;
	[SerializeField] Graphic[] tutorialBorders;
	[SerializeField] TextMeshProUGUI tutorialHeader;
	[SerializeField] RectTransform tutorialTxtBox;
	[SerializeField] bool showTutorial;
	[SerializeField] float tutorialLerpTime;

	[SerializeField] TextMeshProUGUI tutorialContent;
	public GameObject movementTutorial;
	[SerializeField] float tutorialTextLerpTime;

	[Header("Cutscene Items")]
	[SerializeField] TextMeshProUGUI cutsceneMsg;
	[SerializeField] bool canCloseMenu;
	[SerializeField] TextMeshProUGUI subtitles;
	[SerializeField] float subtitlesLerpTime, subtitlesDisplayTime;
	[SerializeField] bool useFade, showSubtitles;

	[Header("Game States")]
	//May want to use Enum for Game States
	public bool isGameOver;
	public bool isPaused;

	[Header("White Dots")]
	public GameObject whiteDot;
	public RectTransform whiteDotHolder;

	[Header("Tutorial Instructions")]
	public TextMeshProUGUI currentHint;

	[Header("Checkpoint System")]
	ObjectiveManager objM;
	public bool touchedCheckpoint;

	[Header ("Others")]
	public Color disabledUIColor = new Color(0.8f, 0.8f, 0.8f, 0.75f);
	public Color playerColor = Color.white;
	public Color redColor = Color.red;
	public Color blueColor = Color.blue;
	public Color yellowColor = Color.yellow;
	public Color greenColor = Color.green;
	public Action action;
	SoundManager soundManager;
	AudioSource[] controlKioskAudioSources;

	private void Awake()
	{
		inst = this;

		screenSize = new Vector2(Screen.width, Screen.height);
		baseRes = Screen.currentResolution;
		currentRes = baseRes;

		for (int i = 0; i < 10; i++)
		{
			if (i == 0) detectedPointers.Add(mainPointer);
			else detectedPointers.Add(Instantiate(mainPointer, mainPointer.transform.parent));

			detectedPointers[i].gameObject.SetActive(false);
		}

		//Set Min Max XY for Waypoint Pos. 1920x1080 is the base size that we are using for our UI Anchoring Positions
		offset = new Vector2(baseOffset.x / 1920 * screenSize.x, baseOffset.y / 1080 * screenSize.y);

		//minXY = new Vector2(marker.GetPixelAdjustedRect().width / 2 + offset.x, offset.y);
		//maxXY = new Vector2(Screen.width - minXY.x, Screen.height - (minXY.y + marker.GetPixelAdjustedRect().height));
		minXY = new Vector2(movableDetectionComp.rect.width / 2 + offset.x, offset.y);
		maxXY = new Vector2(screenSize.x - minXY.x, screenSize.y - (minXY.y + movableDetectionComp.rect.height));
	}

	private void Start()
	{
		player = PlayerController.inst;

		foreach (Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(false);
		foreach (Image flashGauge in flashingGauges) flashGauge.gameObject.SetActive(false);
		foreach (Image warning in detectedAlerts) warning.color = warning.color.ChangeAlpha(0);

		gaugeFlashTime = 0;

		//CCTV UI Start Color and Anchored Positions
		foreach (Image cctvUIBorder in cctvUIBorders)
		{
			cctvUIBorder.rectTransform.anchoredPosition = Vector2.zero;
			cctvUIBorder.rectTransform.localScale = Vector3.one  * 1.5f;
			cctvUIBorder.color = Color.clear;
		}

		hackableName.color = Color.clear;
		unhackGraphicsColor = new Color[unhackInstructions.Length];

		for (int i = 0; i < unhackInstructions.Length; i++)
		{
			unhackGraphicsColor[i] = unhackInstructions[i].color;
			unhackInstructions[i].color = Color.clear;
		}

		crosshairs[1].color = Color.clear;
		SetUIColors(ColorIdentifier.none); //Set Crosshair and Borders Color to fit player Color

		//Set Color and Border of Control Infos
		Color startColor = Color.clear;
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i].border.fillAmount = 0;
			controls[i].text.color = startColor;
			controls[i].icon.color = startColor;
			controls[i].backdrop.color = Color.clear;
		}

		//For Error
		errorWindow.anchoredPosition = new Vector2(10, 115);

		foreach (Graphic graphic in errorBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 0);
		errorHeader.color = ColorUtils.ChangeAlpha(errorHeader.color, 0);

		errorBorders[0].rectTransform.anchoredPosition = new Vector2(-2, -30);
		errorBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, 55f);
		errorTxtBox.sizeDelta = new Vector2(160, 0);

		errorContent.color = ColorUtils.ChangeAlpha(errorContent.color, 0);


		//For Tutorial
		tutorialWindow.anchoredPosition = new Vector2(-10, 115);

		foreach (Graphic graphic in tutorialBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 0);
		tutorialHeader.color = ColorUtils.ChangeAlpha(tutorialHeader.color, 0);
		tutorialContent.color = ColorUtils.ChangeAlpha(tutorialContent.color, 0);

		tutorialBorders[0].rectTransform.anchoredPosition = new Vector2(-30, 12.5f);
		tutorialBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, 55f);
		tutorialTxtBox.sizeDelta = new Vector2(160, 0);

		//Adding Scripted Animations to Action Delegate
		action += LerpInstructions;
		action += LerpActionAvailability;
		action += FlashDetectedWarning;
		action += FlashGaugeOnHighAlert;
		action += AIFocus;

		action += PointToObjective;
		action += PointDetectionGaugeToPlayer;
		action += ShowHideGaugeBackdrop;

		EmergencyAlarm[] alarms = FindObjectsOfType<EmergencyAlarm>();
		controlKioskAudioSources = new AudioSource[alarms.Length * 2];
		for (int i = 0; i < alarms.Length; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				controlKioskAudioSources[i * 2 + j] = alarms[i].audioSources[j];
			}
		} 

		soundManager = SoundManager.inst;
		objM = ObjectiveManager.inst;

		if (LoadingScreen.inst.startWithBackdrop) PlayCutscene(0);
	}

	void Update()
	{
		if (Screen.width != screenSize.x || Screen.height != screenSize.y) OnScreenSizeChange();

		foreach (Image detectionGauge in detectionGauges) detectionGauge.fillAmount = (player.detectionGauge / player.detectionThreshold);

		if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && !LoadingScreen.inst.isLoading) PausePlay();

		if (isGameOver && canCloseMenu && Input.GetMouseButtonDown(0)) LoadingScreen.inst.AutoLoadNextScene();

		//if (Input.GetKeyDown(KeyCode.K)) StartCoroutine(ShowHideSubtitles(new string[] { "I need to build my statikk shiv", "Ded ass my n word" }));

		if (errorIsShowing)
		{
			errorShowTime -= Time.deltaTime;
			if (errorShowTime <= 0) HideError();
		}

		if (action != null) action();
	}

	public void GameOver()
	{
		//PlayerPrefs.SetInt ("Last Objective Saved", objM.currentGoalNumber);

		isGameOver = true;
		Time.timeScale = 0;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		//Closes all other Screens
		pauseScreen.gameObject.SetActive(false);
		optionsScreen.gameObject.SetActive(false);
		gameOverScreen.gameObject.SetActive(true);

		for (int i = 0; i < controlKioskAudioSources.Length; i++)
		{
			if (isGameOver)
				controlKioskAudioSources[i].Pause();
			else
				controlKioskAudioSources[i].UnPause();
		}
	}

	public void Focus(bool playerIsFocusing)
	{
		if (playerIsFocusing && !isFocusing) isFocusing = true;
		else if (!playerIsFocusing && isFocusing) isFocusing = false;
		else return;

		if (crosshairIsLerping) return;

		crosshairIsLerping = true;
		action += LerpFocusFeedback;
	}

	public void DisplayInstructionsAndErrors(bool isHackableObj, bool[] hasErrors, bool haveSecondary = false)
	{
		if (isHackableObj)
		{
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

	public void SetUIColors(ColorIdentifier color = ColorIdentifier.none)
	{
		currentColor = color;
		Color uiColor = GetCurrentColor(currentColor);

		for (int i = 0; i < controls.Length; i++) controls[i].border.color = uiColor;

		foreach (Image crosshair in crosshairs)
		{
			uiColor.a = crosshair.color.a;
			crosshair.color = uiColor;
		}

		//Only for AI Detection Items
		if (color != ColorIdentifier.none) foreach (Graphic aiDetectionItem in aiDetectionElements) aiDetectionItem.color = uiColor.ChangeAlpha(aiDetectionItem.color.a);
	}

	public void ResetInstructionsDisplayOnHack()
	{
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i].show = false;
			controls[i].icon.color = Color.clear;
			controls[i].text.color = Color.clear;
			controls[i].border.fillAmount = 0;
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

	public void SwitchUI(HackableType type, HackableType prevType)
	{
		currentType = type;
		this.prevType = prevType;

		if (currentType == this.prevType) return;

		switch (type)
		{
			case HackableType.CCTV:
				hackableUI.SetActive(true);
				cctvUI.SetActive(true);
				guiAnim.SetBool("In Hackable", true);

				aiUI.SetActive(false);
				playerUI.SetActive(false);
				break;

			case HackableType.AI:
				hackableUI.SetActive(true);
				aiUI.SetActive(true);
				guiAnim.SetBool("In Hackable", true);

				cctvUI.SetActive(false);
				playerUI.SetActive(false);
				break;

			default:
				playerUI.SetActive(true);
				guiAnim.SetBool("In Hackable", false);

				playerPointer.gameObject.SetActive(false); //Set Pointer To Inactive whenever Unhack
				hackableUI.SetActive(false);
				cctvUI.SetActive(false);
				aiUI.SetActive(false);
				break;
		}
	}

	public void StartUILerp(bool fadeIn)
	{
		action -= UITemplatesFadeAnim; //Remove to prevent Errors
		if (fadeIn) uiLerpTime = 0;

		uiFadeIn = fadeIn;
		uiFadeInProgress = true;
		//Only Change Name if in Hackable when UI is Fading In
		dottedItems.SetActive(false); //Always Set the Dotted Items in AI UI to false whenever the Start of a Transition
		if (player.inHackable && fadeIn) ChangeHackableDisplayName(player.hackedObj.roomName, player.hackedObj.hackableName);
		action += UITemplatesFadeAnim;
	}

	public void ChangeHackableDisplayName(string roomName, string hackableName)
	{
		this.hackableName.text = string.Format("{0}\n{1}", roomName, hackableName);
	}

	//Display Error Upon Button Press
	public void DisplayError(string error = "")
	{
		if (error == string.Empty) return;

		errorContent.text = error;
		errorShowTime = 3;
		errorTextLerpTime = 0;
		action += ErrorTextFade;

		if (!errorIsShowing)
		{
			if (showError) return;

			action -= ErrorPopInPopOut;

			showError = true;
			action += ErrorPopInPopOut;
		}
	}

	public void HideError()
	{
		errorIsShowing = false;

		showError = false;
		action += ErrorPopInPopOut;
	}

	public void ShowHideTutorial(bool show, string text = "", bool isMovement = false)
	{
		showTutorial = show;
		if (showTutorial)
		{
			if (isMovement)
			{
				movementTutorial.SetActive(true);
				tutorialContent.text = "";
			}
			else tutorialContent.text = text;
			
			tutorialTextLerpTime = 0;
			action += TutorialTextFade;
		}
		else if (isMovement) movementTutorial.SetActive(false);

		action += TutorialPopInPopOut;
	}

	//Hide or Show All UI, used in Special Interactions like the Numpad
	public void ShowHideUI(bool show)
	{
		if (player.inHackable) cctvUI.gameObject.SetActive(show);
		//else playerUI.gameObject.SetActive(show); Currently Player UI only have Detection Gauge
		foreach (Image crosshair in crosshairs) crosshair.gameObject.SetActive(show);
		marker.gameObject.SetActive(show); //Show Hide Obj Marker
		controlsGrp.SetActive(show); //Show Hide Controls
		whiteDotHolder.gameObject.SetActive(show); //Show Hide White Dots
	}

	public void ShowSavedAfterCheckpoints ()
	{
		if (touchedCheckpoint)
		{
			StartCoroutine("ShowSavedIcon");
		}
		touchedCheckpoint = !touchedCheckpoint;
	}

	IEnumerator ShowSavedIcon ()
	{	
		guiAnim.SetBool ("Checkpoint", true);
		yield return new WaitForSeconds (3.0f);
		guiAnim.SetBool ("Checkpoint", false);
	}

	#region Objective Marker Functions
	void PointToObjective()
	{
		if (objective == Vector3.zero) return;

		Vector2 objScreenPos = player.CurrentViewingCamera.WorldToScreenPoint(objective);

		//Distance from Player to Objective
		int dist = Mathf.RoundToInt((player.CurrentViewingCamera.transform.position - objective).magnitude);

		distanceToObj.text = dist.ToString() + "m";

		Vector3 dirToObj = (objective - player.CurrentViewingCamera.transform.position).normalized;

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

	public void SetNewObjective (Vector3 newObjective, bool firstTime = false)
	{
		objective = newObjective;
		if (!firstTime)
			soundManager.PlaySound (soundManager.nextObjective);
		ShowMarker();
		PlayerPrefs.SetInt ("Last Objective Saved", objM.currentGoalNumber);
	}
	#endregion

	#region Detection Gauge Functions
	void ShowHideGaugeBackdrop()
	{
		if (player.detectionGauge > 0 && !showGaugeBackdrop)
		{
			showGaugeBackdrop = true;
			foreach(Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(true);
		}
		else if (player.detectionGauge <= 0 && showGaugeBackdrop)
		{
			showGaugeBackdrop = false;
			foreach (Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(false);
		}
	}

	void FlashDetectedWarning()
	{
		if (warningTime == 0 && !player.isDetected) return;

		float alpha = 0;
		if (player.isDetected)
		{
			warningTime += Time.deltaTime * 2f;
			alpha = Mathf.PingPong(warningTime, 1);
		}
		else warningTime = 0;

		foreach (Image alert in detectedAlerts) alert.color = alert.color.ChangeAlpha(alpha);
	}

	void FlashGaugeOnHighAlert()
	{
		bool showFlash = detectionGauges[0].fillAmount >= 0.5f;
		if (!showFlash && !flashingGauges[0].gameObject.activeSelf) return;

		if (showFlash) gaugeFlashTime = Mathf.Min(gaugeFlashTime + Time.deltaTime * 3, 1);
		else gaugeFlashTime = 0;

		float alpha = Mathf.Lerp(0.75f, 0, gaugeFlashTime);

		foreach (Image flashGauge in flashingGauges)
		{
			flashGauge.gameObject.SetActive(showFlash);
			flashGauge.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, gaugeFlashTime);
			flashGauge.color = flashGauge.color.ChangeAlpha(alpha);
		}

		if (gaugeFlashTime >= 1) gaugeFlashTime = 0;
	}

	void AIFocus()
	{
		if (aiFocusGrp.gameObject.activeInHierarchy) for (int i = 0; i < aiFocusRings.Length; i++) aiFocusRings[i].rectTransform.Rotate(new Vector3(0, 0, rotationSpeeds[i] * Time.deltaTime));

		if ((playerOnScreen && aiFocusLerpTime >= 1) || (!playerOnScreen && aiFocusLerpTime <= 0)) return;
		aiFocusLerpTime = playerOnScreen ? Mathf.Min(aiFocusLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(aiFocusLerpTime - Time.deltaTime * 5, 0);

		foreach (Image ring in aiFocusRings)
		{
			ring.color = ring.color.ChangeAlpha(aiFocusLerpTime);
			ring.rectTransform.localScale = Vector3.Lerp(Vector3.one * 3f, Vector3.one, aiFocusLerpTime);
		}
	}

	//May need to Account for Vertical Direction as well
	//Sometimes doesnt work. Not sure how to replicate the bug
	void PointDetectionGaugeToPlayer()
	{
		if (!detectionGauges[1].gameObject.activeInHierarchy || detectionGauges[1].fillAmount == 0)
		{
			if (playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(false);
			if (playerOnScreen) playerOnScreen = false;
			return;
		}

		Vector3 playerPos = player.transform.position + new Vector3(0, player.GetPlayerHeight() + 0.1f, 0); //0.1f is the Offset
		Vector3 screenPos = player.CurrentViewingCamera.WorldToScreenPoint(playerPos);

		if (screenPos.z <= 0 || screenPos.x < minXY.x || screenPos.x > maxXY.x || screenPos.y < minXY.y || screenPos.y > maxXY.y)
		{
			playerOnScreen = false;

			//Multiply by -1 if the Object is behind
			if (screenPos.z < 0) screenPos *= -1;

			//Get Center of the Screen.
			//Meant for Translation such that Calculations are done whereby the Center is the Pivot
			Vector3 center = new Vector3(Screen.width, Screen.height, 0) / 2;
			screenPos -= center;

			//Find Angle from Target Screen Pos to Center of Screen
			float angle = Mathf.Atan2(screenPos.y, screenPos.x); //
			angle -= 90 * Mathf.Deg2Rad;
			float cos = Mathf.Cos(angle);
			float sin = -Mathf.Sin(angle);

			screenPos = center + new Vector3(sin * 150, cos * 150, 0);

			//y = mx + b format
			float m = cos / sin;

			//Check up and down first
			if (cos > 0) screenPos = new Vector3(center.y / m, center.y, 0);
			else screenPos = new Vector3(-center.y / m, -center.y, 0);

			//If out of Bounds, Get Pointer on Correct Side
			if (screenPos.x > center.x) screenPos = new Vector3(center.x, center.x * m, 0); //Out of Bounds Right
			else if (screenPos.x < -center.x) screenPos = new Vector3(-center.x, -center.x * m, 0); //Out of Bounds Left

			screenPos += center;

			if (player.isDetected)
			{
				if (!playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(true);
				playerPointer.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg); //Set Rotation of Player Pointer to Point at Player
			}
			else if (playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(false);
		}
		else
		{
			playerOnScreen = true;
			if (playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(false);
		} 

		//Clamping to Edges of Screen
		screenPos.x = Mathf.Clamp(screenPos.x, minXY.x, maxXY.x);
		screenPos.y = Mathf.Clamp(screenPos.y, minXY.y, maxXY.y);

		movableDetectionComp.transform.position = screenPos;

		if (playerOnScreen)
		{
			Vector3 focusPos = player.transform.position + new Vector3(0, player.GetPlayerHeight()/2 + 0.1f, 0);
			Vector3 screenFocusPos = player.CurrentViewingCamera.WorldToScreenPoint(focusPos);
			aiFocusGrp.transform.position = screenFocusPos;
		}
	}
	#endregion

	#region GUI Animations
	void LerpFocusFeedback()
	{
		crosshairLerpTime = isFocusing ? Mathf.Min(crosshairLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(crosshairLerpTime - Time.deltaTime * 5, 0);

		Color targetColor = GetCurrentColor(currentColor);
		crosshairs[0].color = Color.Lerp(targetColor, Color.clear, crosshairLerpTime); //May not even need 0. Just Lerp the Filled Dot Color
		crosshairs[1].color = Color.Lerp(Color.clear, targetColor, crosshairLerpTime);

		if (crosshairLerpTime >= 1 && isFocusing)
		{
			crosshairs[0].color = Color.clear;
			crosshairs[1].color = targetColor;

			crosshairIsLerping = false;
			action -= LerpFocusFeedback;
		}
		else if (crosshairLerpTime <= 0 && !isFocusing)
		{
			crosshairs[0].color = targetColor;
			crosshairs[1].color = Color.clear;

			crosshairIsLerping = false;
			action -= LerpFocusFeedback;
		}
	}

	void ErrorTextFade()
	{
		errorTextLerpTime = Mathf.Min(errorTextLerpTime + Time.deltaTime * 5, 1);

		errorContent.color = ColorUtils.ChangeAlpha(errorContent.color, errorTextLerpTime);
		if (errorTextLerpTime >= 1) action -= ErrorTextFade;
	}

	void ErrorPopInPopOut()
	{
		errorLerpTime = showError ? Mathf.Min(errorLerpTime + Time.deltaTime * 2f, 1) : Mathf.Max(errorLerpTime - Time.deltaTime * 2f, 0);

		float earlyLerpTime = Mathf.Clamp(errorLerpTime / 0.45f, 0, 1);

		errorWindow.anchoredPosition = new Vector2(Mathf.Lerp(10, -250, earlyLerpTime), 115);

		foreach (Graphic graphic in errorBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, earlyLerpTime);
		errorHeader.color = ColorUtils.ChangeAlpha(errorHeader.color, earlyLerpTime);

		float lateLerpTime = Mathf.Clamp((errorLerpTime - 0.5f) / 0.5f, 0, 1);

		errorBorders[0].rectTransform.anchoredPosition = new Vector2(-2, Mathf.Lerp(-30, 12.5f, lateLerpTime));
		errorBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, Mathf.Lerp(55.5f, -8.5f, lateLerpTime));
		errorTxtBox.sizeDelta = new Vector2(160, Mathf.Lerp(0, 120, lateLerpTime));

		if (showError && errorLerpTime >= 1)
		{
			errorWindow.anchoredPosition = new Vector2(-250, 115);

			foreach (Graphic graphic in errorBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 1);
			errorHeader.color = ColorUtils.ChangeAlpha(errorHeader.color, 1);

			errorBorders[0].rectTransform.anchoredPosition = new Vector2(-2, 12.5f);
			errorBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, -8.5f);
			errorTxtBox.sizeDelta = new Vector2(160, 120);

			errorIsShowing = true;

			action -= ErrorPopInPopOut;
		}
		else if (!showError && errorLerpTime <= 0)
		{
			errorWindow.anchoredPosition = new Vector2(10, 115);

			foreach (Graphic graphic in errorBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 0);
			errorHeader.color = ColorUtils.ChangeAlpha(errorHeader.color, 0);

			errorBorders[0].rectTransform.anchoredPosition = new Vector2(-2, -30);
			errorBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, 55f);
			errorTxtBox.sizeDelta = new Vector2(160, 0);

			action -= ErrorPopInPopOut;
		}
	}

	void TutorialTextFade()
	{
		tutorialTextLerpTime = Mathf.Min(tutorialTextLerpTime + Time.deltaTime * 5, 1);

		tutorialContent.color = ColorUtils.ChangeAlpha(tutorialContent.color, tutorialTextLerpTime);
		if (tutorialTextLerpTime >= 1) action -= TutorialTextFade;
	}

	void TutorialPopInPopOut()
	{
		tutorialLerpTime = showTutorial ? Mathf.Min(tutorialLerpTime + Time.deltaTime * 2f, 1) : Mathf.Max(tutorialLerpTime - Time.deltaTime * 2f, 0);

		float earlyLerpTIme = Mathf.Clamp(tutorialLerpTime / 0.45f, 0, 1);

		tutorialWindow.anchoredPosition = new Vector2(Mathf.Lerp(-10, 242.5f, earlyLerpTIme), 115);

		foreach (Graphic graphic in tutorialBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, earlyLerpTIme);
		tutorialHeader.color = ColorUtils.ChangeAlpha(tutorialHeader.color, earlyLerpTIme);

		float lateLerpTime = Mathf.Clamp((tutorialLerpTime - 0.5f) / 0.5f, 0, 1);

		tutorialBorders[0].rectTransform.anchoredPosition = new Vector2(-2, Mathf.Lerp(-30, 12.5f, lateLerpTime));
		tutorialBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, Mathf.Lerp(55f, -8.5f, lateLerpTime));
		tutorialTxtBox.sizeDelta = new Vector2(160, Mathf.Lerp(0, 120, lateLerpTime));

		if (showTutorial && tutorialLerpTime >= 1)
		{
			tutorialWindow.anchoredPosition = new Vector2(242.5f, 115);

			foreach (Graphic graphic in tutorialBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 1);
			tutorialHeader.color = ColorUtils.ChangeAlpha(tutorialHeader.color, 1);

			tutorialBorders[0].rectTransform.anchoredPosition = new Vector2(-2, 12.5f);
			tutorialBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, -8.5f);
			tutorialTxtBox.sizeDelta = new Vector2(160, 120);

			action -= TutorialPopInPopOut;
		}
		else if (!showTutorial && tutorialLerpTime <= 0)
		{
			tutorialWindow.anchoredPosition = new Vector2(-10, 115);

			foreach (Graphic graphic in tutorialBorders) graphic.color = ColorUtils.ChangeAlpha(graphic.color, 0);
			tutorialHeader.color = ColorUtils.ChangeAlpha(tutorialHeader.color, 0);

			tutorialBorders[0].rectTransform.anchoredPosition = new Vector2(-2, -30);
			tutorialBorders[1].rectTransform.anchoredPosition = new Vector2(1.2f, 55f);
			tutorialTxtBox.sizeDelta = new Vector2(160, 0);

			action -= TutorialPopInPopOut;
		}
	}

	void UITemplatesFadeAnim()
	{
		uiLerpTime = uiFadeIn ? Mathf.Min(uiLerpTime + Time.deltaTime * 2.5f, 1) : Mathf.Max(uiLerpTime - Time.deltaTime * 4.5f, 0);
		HackableType type = uiFadeIn ? currentType : prevType;
		Color targetColor = GetCurrentColor(currentColor);

		bool isPlayer = false;

		switch (type)
		{
			case HackableType.CCTV:
				for (int i = 0; i < cctvUIBorders.Length; i++)
				{
					int xMult = cctvUIBorders[i].rectTransform.pivot.x == 0 ? 1 : -1;
					int yMult = cctvUIBorders[i].rectTransform.pivot.y == 0 ? 1 : -1;

					cctvUIBorders[i].rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(23.5f * xMult, 26.5f * yMult), Mathf.Clamp(uiLerpTime / 0.75f, 0, 1));
					cctvUIBorders[i].rectTransform.localScale = Vector3.Lerp(new Vector3(1.25f, 1.25f, 1.25f), Vector3.one, Mathf.Clamp(uiLerpTime / 0.75f, 0, 1));
					cctvUIBorders[i].color = Color.Lerp(Color.clear, targetColor, uiLerpTime);
				}
				break;

			case HackableType.AI:

				Color targetColorA = targetColor;
				targetColorA = Color.Lerp(Color.clear, targetColorA, uiLerpTime);

				foreach (Graphic aiUIElement in aiUIElements) aiUIElement.color = targetColorA;

				break;

			default:

				isPlayer = true;
				foreach (Graphic playerUI in playerUIElements) playerUI.color = Color.Lerp(Color.clear, targetColor, uiLerpTime);

				break;
		}

		//Lerping of Common UI Elements
		float lerpTimeLate = Mathf.Clamp((uiLerpTime - 0.5f) / 0.5f, 0, 1);

		if (!isPlayer)
		{
			hackableName.color = Color.Lerp(Color.clear, targetColor, lerpTimeLate);
			for (int i = 0; i < unhackInstructions.Length; i++) unhackInstructions[i].color = Color.Lerp(Color.clear, unhackGraphicsColor[i], lerpTimeLate);
		}

		if (uiLerpTime >= 1 && uiFadeIn)
		{
			if (isPlayer) foreach (Graphic playerUI in playerUIElements) playerUI.color = targetColor;
			else
			{
				switch (type)
				{
					case HackableType.CCTV:
						for (int i = 0; i < cctvUIBorders.Length; i++)
						{
							int xMult = cctvUIBorders[i].rectTransform.pivot.x == 0 ? 1 : -1;
							int yMult = cctvUIBorders[i].rectTransform.pivot.y == 0 ? 1 : -1;

							cctvUIBorders[i].rectTransform.anchoredPosition = new Vector2(23.5f * xMult, 26.5f * yMult);
							cctvUIBorders[i].rectTransform.localScale = Vector3.one;
							cctvUIBorders[i].color = targetColor;
						}
						break;

					case HackableType.AI:

						foreach (Graphic aiUIElement in aiUIElements) aiUIElement.color = targetColor;
						dottedItems.SetActive(true);

						break;
				}

				hackableName.color = targetColor;
				for (int i = 0; i < unhackInstructions.Length; i++) unhackInstructions[i].color = unhackGraphicsColor[i];
			}

			uiFadeInProgress = false;
			action -= UITemplatesFadeAnim;
		}
		else if (uiLerpTime <= 0 && !uiFadeIn)
		{
			if (isPlayer) foreach (Graphic playerUI in playerUIElements) playerUI.color = targetColor;
			else
			{
				switch (type)
				{
					case HackableType.CCTV:
						foreach (Image cctvUIBorder in cctvUIBorders)
						{
							cctvUIBorder.rectTransform.anchoredPosition = Vector2.zero;
							cctvUIBorder.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
							cctvUIBorder.color = Color.clear;
						}
						break;

					case HackableType.AI:

						foreach (Graphic aiUIElement in aiUIElements) aiUIElement.color = Color.clear;
						break;
				}

				hackableName.color = Color.clear;
				foreach (Graphic graphic in unhackInstructions) graphic.color = Color.clear;
			}

			uiFadeIn = true;
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
				if ((controls[i].lerpTime >= 1 && controls[i].show && isFocusing) || (controls[i].lerpTime <= 0 && !controls[i].show)) ignoreLerp = true;
				if ((controls[i].errorLerpTime >= 1 && controls[i].hasError && controls[i].show) || (controls[i].errorLerpTime <= 0 && !controls[i].hasError)) ignoreErrorLerp = true;
			}

			if (!ignoreLerp)
			{
				controls[i].isLerping = true;
				if (uiFadeInProgress || !isFocusing) controls[i].show = false;
				controls[i].lerpTime = controls[i].show ? Mathf.Min(controls[i].lerpTime + Time.deltaTime * 4.5f, 1) : Mathf.Max(controls[i].lerpTime - Time.deltaTime * 4.5f, 0);

				controls[i].border.fillAmount = Mathf.Clamp(controls[i].lerpTime / 0.5f, 0, 1);

				Color controlsColor = Color.Lerp(Color.clear, controls[i].hasError ? disabledUIColor : Color.white, Mathf.Clamp((controls[i].lerpTime - 0.25f) / 0.75f, 0, 1));
				controls[i].text.color = controlsColor;
				controls[i].icon.color = controlsColor;

				controls[i].backdrop.color = Color.Lerp(Color.clear, new Color(0, 0, 0, 0.75f), Mathf.Clamp((controls[i].lerpTime - 0.25f) / 0.75f, 0, 1));

				if (controls[i].lerpTime >= 1 && controls[i].show)
				{
					controlsColor = controls[i].hasError ? disabledUIColor : Color.white;
					controls[i].text.color = controlsColor;
					controls[i].icon.color = controlsColor;
					controls[i].backdrop.color = new Color(0, 0, 0, 0.75f);
					controls[i].border.fillAmount = 1;
					controls[i].isLerping = false;
				}
				else if (controls[i].lerpTime <= 0 && !controls[i].show)
				{
					controlsColor = Color.clear;
					controls[i].text.color = controlsColor;
					controls[i].icon.color = controlsColor;
					controls[i].backdrop.color = controlsColor;
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
		player.Unhack(true);
		player.ForcedUnhackAnimEvent();
	}
	#endregion

	#region Button Functions
	public void PausePlay ()
	{
		isPaused = !isPaused;
		LoadingScreen.inst.PausePlayWatch(isPaused);
		optionsScreen.gameObject.SetActive(false); //If Players Press the Esc Key when in the Options Menu. Unless you want to disable use of Shortcut Keys when in Pause
		pauseScreen.gameObject.SetActive(isPaused);
		Cursor.visible = isPaused ? true : player.inSpInteraction ? true : isPaused;
		Cursor.lockState = isPaused ?  CursorLockMode.None : player.inSpInteraction ? CursorLockMode.None : CursorLockMode.Locked;
		Time.timeScale = isPaused ? 0 : 1;
		soundManager.PlaySound (soundManager.click);

		for (int i = 0; i < controlKioskAudioSources.Length; i++)
		{
			if (isPaused)
				controlKioskAudioSources[i].Pause();
			else
				controlKioskAudioSources[i].UnPause();
		}
	}

	public void Options()
	{
		optionsScreen.gameObject.SetActive(!optionsScreen.gameObject.activeSelf);
		soundManager.PlaySound (soundManager.click);
	}

	public void MainMenu ()
	{
		Time.timeScale = 1;
		LoadingScreen.inst.LoadScene("Main Menu");
		soundManager.PlaySound (soundManager.click);
		//SceneManager.LoadScene ("Main Menu", LoadSceneMode.Single);
	}

	public void LoadCheckpoint()
	{
		Time.timeScale = 1;
		soundManager.PlaySound (soundManager.click);
		LoadingScreen.inst.LoadScene(SceneManager.GetActiveScene().name);
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}

	public void Restart()
	{
		//Need a Proper Respawn
		Time.timeScale = 1;
		soundManager.PlaySound (soundManager.click);
		LoadingScreen.inst.LoadScene(SceneManager.GetActiveScene().name);
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}
	#endregion

	#region Other UI Utils
	public void SetControlsFeedback(List<string> instructions)
	{
		for (int i = 0; i < instructions.Count; i++)
		{
			//If you want the Elements to FULLY Fade out before it changes, Have to Store in a String or Sprite instead of using text.text and icon.sprite
			controls[i].text.text = instructions[i];
			controls[i].icon.sprite = GetSpriteFromStr(instructions[i]);
			controls[i].show = true;
		}

		for (int i = instructions.Count; i < controls.Length; i++)
		{
			controls[i].text.text = instructions[i];
			controls[i].icon.sprite = GetSpriteFromStr(instructions[i]);
			controls[i].show = false;
		}
	}

	Sprite GetSpriteFromStr(string str)
	{
		switch (str)
		{
			case "Hack":
				return controlsSprites[0];
			case "Interact":
				return controlsSprites[1];
			case "Wipe Memory":
				return controlsSprites[1];
			case "Unhack":
				return controlsSprites[0];
			default:
				return null;
		}
	}

	Color GetCurrentColor(ColorIdentifier color)
	{
		switch (color)
		{
			case ColorIdentifier.red:
				return redColor;
			case ColorIdentifier.blue:
				return blueColor;
			case ColorIdentifier.yellow:
				return yellowColor;
			case ColorIdentifier.green:
				return greenColor;
			default:
				return playerColor;
		}
	}

	//Meant to be Temporary so as for the Area Manager to work
	public TextMeshProUGUI GetRoomAndHackableName()
	{
		return hackableName;
	}

	//Recalculate if Screen Window Changes
	public void OnScreenSizeChange()
	{
		screenSize = new Vector2(Screen.width, Screen.height);

		//Set Min Max XY for Waypoint Pos
		offset = new Vector2(baseOffset.x / 1920 * screenSize.x, baseOffset.y / 1080 * screenSize.y);

		//minXY = new Vector2(marker.GetPixelAdjustedRect().width / 2 + offset.x, offset.y);
		//maxXY = new Vector2(Screen.width - minXY.x, Screen.height - (minXY.y + marker.GetPixelAdjustedRect().height));
		minXY = new Vector2(movableDetectionComp.rect.width / 2 + offset.x, offset.y);
		maxXY = new Vector2(screenSize.x - minXY.x, screenSize.y - (minXY.y + movableDetectionComp.rect.height));
	}
	#endregion

	#region Cutsceene Items
	public void PlayCutscene(int index)
	{
		switch (index)
		{
			case 0:
				guiAnim.SetBool("Play Intro", true);
				player.LockPlayerMovement(true);
				player.LockPlayerRotation(true);
				player.LockPlayerAction(true);
				break;
			case 1:
				guiAnim.SetBool("Play Mid", true);
				break;
			case 2:
				guiAnim.SetBool("Play End", true);
				player.LockPlayerMovement(true);
				player.LockPlayerAction(true);
				break;
		}
	}

	public void EndCutscene()
	{
		LoadingScreen.inst.startWithBackdrop = false;
		player.LockPlayerMovement(false);
		player.LockPlayerRotation(false);
		player.LockPlayerAction(false);
	}

	void OnBackdropFadeOut()
	{
		player.LockPlayerRotation(false);
	}

	void PlaySound(int soundIndex)
	{
		switch (soundIndex)
		{
			case 0:
				if (soundManager.ventSfx.clip) soundManager.PlaySound(soundManager.ventSfx);
				break;
			case 1:
				if (soundManager.dropSfx.clip) soundManager.PlaySound(soundManager.dropSfx);
				break;
		}
	}

	void ChangeMsgText(string msg)
	{
		cutsceneMsg.text = msg;	
	}

	void CanCloseMenu()
	{
		canCloseMenu = true;
	}

	public IEnumerator ShowHideSubtitles(string[] text)
	{
		subtitles.text = string.Empty;
		subtitles.gameObject.SetActive(true);

		if (useFade)
		{
			subtitles.color = subtitles.color.ChangeAlpha(0);
			for (int i = 0; i < text.Length; i++)
			{
				subtitles.text = text[i];
				while (subtitlesLerpTime < 1)
				{
					subtitlesLerpTime = Mathf.Min(subtitlesLerpTime + Time.deltaTime * 3.5f, 1);
					subtitles.color = subtitles.color.ChangeAlpha(subtitlesLerpTime);
					yield return new WaitForEndOfFrame();
				}
				
				yield return new WaitForSeconds(subtitlesDisplayTime);
				
				while (subtitlesLerpTime > 0)
				{
					subtitlesLerpTime = Mathf.Max(subtitlesLerpTime - Time.deltaTime * 3.5f, 0);
					subtitles.color = subtitles.color.ChangeAlpha(subtitlesLerpTime);
					yield return new WaitForEndOfFrame();
				}

				if (i != text.Length - 1) yield return new WaitForSeconds(0.25f); //Add a Slight Pause before next text plays
			}

			subtitles.gameObject.SetActive(false);
		}
		else
		{
			for (int i = 0; i < text.Length; i++)
			{
				subtitles.text = string.Empty;
				char[] chars = text[i].ToCharArray();

				for (int j = 0; j < chars.Length; j++)
				{
					subtitles.text += chars[j];
					yield return new WaitForSeconds(0.02f);
				}

				yield return new WaitForSeconds(subtitlesDisplayTime);
			}

			subtitles.gameObject.SetActive(false);
		}
	}
	#endregion
}
