using System;
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
	public TextMeshProUGUI text; //Text of the Controls
	public Image border; //The Line
}

public class UIManager : MonoBehaviour
{
	[Header("General Properties")]
	public static UIManager inst;
	[SerializeField] PlayerController player;
	[SerializeField] ColorIdentifier currentColor = ColorIdentifier.none;
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
	[SerializeField] Graphic[] unhackInstructions; //For now Store the Unhack Instructions as a Graphic
	[SerializeField] Color[] defaultGraphicsColor;
	[SerializeField] Image[] cctvUIBorders;
	[SerializeField] TextMeshProUGUI hackableName;

	[Header("Crosshair")]
	[SerializeField] bool isFocusing; //Check if a Hackable or Interactable Object has been focused on
	[SerializeField] Image[] crosshairs; //Stores the Unfilled and Filled Dot
	[SerializeField] float crosshairLerpTime;
	[SerializeField] bool crosshairIsLerping; //Check if the Focus Animation is Ongoing

	[Header("Objective Marker")]
	[SerializeField] Image marker;
	public Vector3 objective;
	[SerializeField] float offset; //Offset for Min Max XY
	[SerializeField] Vector2 minXY, maxXY;
	[SerializeField] TextMeshProUGUI distanceToObj;

	[Header("Stealth Gauge")]
	public RectTransform mainPointer; //Pointer to Instantiate
	public List<RectTransform> detectedPointers; //To Point to where Player is detected from. Only problem that has not been fixed is instantiating when not enough pointers... (Can be Coded in Optimisation)
	[SerializeField] RectTransform playerPointer;
	[SerializeField] Image[] detectionGauges; //[0] is Player, [1] is for CCTV/AI
	[SerializeField] Image[] detectionGaugeBackdrops; //[0] is Player, [1] is for CCTV/AI
	[SerializeField] Image[] detectedAlerts; //[0] is for Player, [1] is for CCTV/AI
	[SerializeField] float warningTime;
	[SerializeField] bool showGaugeBackdrop;

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

	[Header("White Dots")]
	public GameObject whiteDot;
	public RectTransform whiteDotHolder;

	[Header("Tutorial Instructions")]
	public TextMeshProUGUI currentHint;

	[Header ("Others")]
	public Color disabledUIColor = new Color(0.8f, 0.8f, 0.8f, 0.75f);
	public Color playerColor = Color.white;
	public Color redColor = Color.red;
	public Color blueColor = Color.blue;
	public Color yellowColor = Color.yellow;
	public Color greenColor = Color.green;
	public Action action;
	SoundManager soundManager;

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
		offset = offset / 1920 * Screen.width;
		minXY = new Vector2(marker.GetPixelAdjustedRect().width / 2 + offset, marker.GetPixelAdjustedRect().height / 2 + offset);
		maxXY = new Vector2(Screen.width - minXY.x, Screen.height - minXY.y);
	}

	private void Start()
	{
		player = PlayerController.inst;

		foreach (Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(false);
		foreach (Image warning in detectedAlerts) warning.color = new Color(warning.color.r, warning.color.g, warning.color.b, 0);

		//CCTV UI Start Color and Anchored Positions
		foreach (Image cctvUIBorder in cctvUIBorders)
		{
			cctvUIBorder.rectTransform.anchoredPosition = Vector2.zero;
			cctvUIBorder.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			cctvUIBorder.color = Color.clear;
		}

		hackableName.color = Color.clear;

		crosshairs[1].color = Color.clear;
		SetUIColors(ColorIdentifier.none); //Set Crosshair and Borders Color to fit player Color

		defaultGraphicsColor = new Color[unhackInstructions.Length];

		for (int i = 0; i < unhackInstructions.Length; i++)
		{
			defaultGraphicsColor[i] = unhackInstructions[i].color;
			unhackInstructions[i].color = Color.clear;
		}

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
		action += LerpActionAvailability;
		action += FlashDetectedWarning;

		action += PointToObjective;
		action += PointToPlayer;
		action += ShowHideGaugeBackdrop;

		soundManager = SoundManager.inst;
	}

	void Update()
	{
		foreach (Image detectionGauge in detectionGauges) detectionGauge.fillAmount = (player.detectionGauge / player.detectionThreshold);

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

	public void SwitchUI(HackableType type = HackableType.none)
	{
		switch (type)
		{
			case HackableType.CCTV:
				cctvUI.SetActive(true);
				playerUI.SetActive(false);
				break;
			case HackableType.AI:
				cctvUI.SetActive(true);
				playerUI.SetActive(false);
				break;
			default:
				playerUI.SetActive(true);
				cctvUI.SetActive(false);
				break;
		}
	}

	public void StartUILerp(bool fadeIn)
	{
		action -= LerpUITemplate; //Remove to prevent Errors

		uiFadeIn = fadeIn;
		uiFadeInProgress = true;
		//Only Change Name if in Hackable when UI is Fading In
		if (player.inHackable && fadeIn) ChangeHackableDisplayName(player.hackedObj.roomName, player.hackedObj.hackableName);
		action += LerpUITemplate;
	}

	public void ChangeHackableDisplayName(string roomName, string hackableName)
	{
		this.hackableName.text = string.Format("{0}\n{1}", roomName, hackableName);
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

	//Hide or Show All UI, used in Special Interactions like the Numpad
	public void ShowHideUI(bool show)
	{
		if (player.inHackable) cctvUI.gameObject.SetActive(show);
		else playerUI.gameObject.SetActive(show);
		foreach (Image crosshair in crosshairs) crosshair.gameObject.SetActive(show);
		marker.gameObject.SetActive(show); //Show Hide Obj Marker
		controlsGrp.SetActive(show); //Show Hide Controls
		whiteDotHolder.gameObject.SetActive(show); //Show Hide White Dots
	}

	#region Objective Marker Functions
	void PointToObjective()
	{
		if (objective == Vector3.zero) return;

		Vector2 objScreenPos = player.CurrentViewingCamera.WorldToScreenPoint(objective);

		//Distance from Player to Objective
		int dist = Mathf.RoundToInt((player.CurrentViewingCamera.transform.position - objective).magnitude);

		distanceToObj.text = dist.ToString() + "m";

		#region Directional Tracking
		/*//Check if Objective is in front or behind of Player (any body that the Player is in)
		Vector3 dirToObj = (objPos - player.CurrentViewingCamera.transform.position).normalized;
		//If Objective is behind of where Player (any body that the Player is in) is at
		if (Vector3.Dot(player.CurrentViewingCamera.transform.forward, dirToObj) < 0)
		{
			//If Object is on the Right side of the Player, Clamp it to the LEFT (Since Player is facing behind) and vice versa
			if (objScreenPos.x > Screen.width / 2) objScreenPos.x = minXY.x;
			else objScreenPos.x = maxXY.x;
		}

		//Clamp to prevent Marker from going Offscreen
		objScreenPos.x = Mathf.Clamp(objScreenPos.x, minXY.x, maxXY.x);
		objScreenPos.y = Mathf.Clamp(objScreenPos.y, minXY.y, maxXY.y);*/
		#endregion

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

	// void ClearObjective()
	// {
	// 	objective = null;
	// }

	// I put this to public - Nigel
	public void SetNewObjective (Vector3 newObjective, bool firstTime = false)
	{
		objective = newObjective;
		if (!firstTime)
			soundManager.PlaySound (soundManager.nextObjective);
		ShowMarker();
	}
	#endregion

	#region Detection Gauge Functions
	public void ShowHideDetectionGauges(int i = 0, bool show = true)
	{
		detectionGauges[i].gameObject.SetActive(show);
	}

	void ShowHideGaugeBackdrop()
	{
		if (player.detectionGauge > 0 && !showGaugeBackdrop)
		{
			showGaugeBackdrop = true;
			foreach (Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(true);
		}
		else if (player.detectionGauge <= 0 && showGaugeBackdrop)
		{
			showGaugeBackdrop = false;
			foreach (Image backdrop in detectionGaugeBackdrops) backdrop.gameObject.SetActive(false);
		}
	}

	void PointToPlayer()
	{
		if (!detectionGauges[1].gameObject.activeInHierarchy) return;

		Vector3 playerPos = player.transform.position + new Vector3(0, player.DistFromGround, 0);
		Vector3 currentCamPos = player.CurrentViewingCamera.transform.position;
		Vector2 playerScreenPos = player.CurrentViewingCamera.WorldToScreenPoint(playerPos);
		Vector3 dirToPlayer = (playerPos - currentCamPos).normalized;

		if (Vector3.Dot(player.CurrentViewingCamera.transform.forward, dirToPlayer) < 0)
		{
			//If Player Body is on the Right side of the Current Cam, Clamp it to the LEFT (Since Player is facing behind) and vice versa
			if (playerScreenPos.x > Screen.width / 2) playerScreenPos.x = minXY.x;
			else playerScreenPos.x = maxXY.x;
		}

		if (player.isDetected)
		{
			if (playerScreenPos.x < minXY.x || playerScreenPos.x > maxXY.x || playerScreenPos.y < minXY.y || playerScreenPos.y > maxXY.y)
			{
				if (!playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(true);
				print("Player is Off Screen");

				Vector3 horDir = (new Vector3(playerPos.x, 0, playerPos.z) - new Vector3(currentCamPos.x, 0, currentCamPos.z)).normalized;
				Vector3 forward = player.CurrentViewingCamera.transform.forward;

				float horAngle = Vector3.SignedAngle(new Vector3(forward.x, 0, forward.z).normalized, horDir, Vector3.up); //Not Sure why this Works
				playerPointer.eulerAngles = new Vector3(0, 0, -horAngle); //Set Rotation of Player Pointer to Point at Player
			}
			else if (playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(false);
		}
		else if (playerPointer.gameObject.activeInHierarchy) playerPointer.gameObject.SetActive(false);

		//Clamping to Edges of Screen
		playerScreenPos.x = Mathf.Clamp(playerScreenPos.x, minXY.x, maxXY.x);
		playerScreenPos.y = Mathf.Clamp(playerScreenPos.y, minXY.y, maxXY.y);

		detectionGauges[1].transform.position = playerScreenPos;
		detectionGaugeBackdrops[1].transform.position = playerScreenPos;
	}
	#endregion

	#region GUI Animations
	void LerpFocusFeedback()
	{
		crosshairLerpTime = isFocusing ? Mathf.Min(crosshairLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(crosshairLerpTime - Time.deltaTime * 5, 0);

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

	void FlashDetectedWarning()
	{
		if (warningTime == 0 && !player.isDetected) return;

		float alpha = 0;

		Color[] alertColors = new Color[2];

		if (player.isDetected)
		{
			warningTime += Time.deltaTime * 3;
			alpha = Mathf.PingPong(warningTime, 1);
		}
		else warningTime = 0;

		for (int i = 0; i < detectedAlerts.Length; i++)
		{
			alertColors[i] = detectedAlerts[i].color;
			alertColors[i].a = alpha;
			detectedAlerts[i].color = alertColors[i];
		}
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
		uiLerpTime = uiFadeIn ? Mathf.Min(uiLerpTime + Time.deltaTime * 2.5f, 1) : Mathf.Max(uiLerpTime - Time.deltaTime * 4.5f, 0);

		Color targetColor = GetCurrentColor(currentColor);

		for (int i = 0; i < cctvUIBorders.Length; i++)
		{
			int xMult = cctvUIBorders[i].rectTransform.pivot.x == 0 ? 1 : -1;
			int yMult = cctvUIBorders[i].rectTransform.pivot.y == 0 ? 1 : -1;

			cctvUIBorders[i].rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(23.5f * xMult, 26.5f * yMult), Mathf.Clamp(uiLerpTime / 0.75f, 0, 1));
			cctvUIBorders[i].rectTransform.localScale = Vector3.Lerp(new Vector3(1.25f, 1.25f, 1.25f), Vector3.one, Mathf.Clamp(uiLerpTime / 0.75f, 0, 1));
			cctvUIBorders[i].color = Color.Lerp(Color.clear, targetColor, uiLerpTime);
		}

		float lerpTimeLate = Mathf.Clamp((uiLerpTime - 0.5f) / 0.5f, 0, 1);

		hackableName.color = Color.Lerp(Color.clear, targetColor, lerpTimeLate);
		for (int i = 0; i < unhackInstructions.Length; i++) unhackInstructions[i].color = Color.Lerp(Color.clear, defaultGraphicsColor[i], lerpTimeLate);

		if (uiLerpTime >= 1 && uiFadeIn)
		{
			for (int i = 0; i < cctvUIBorders.Length; i++)
			{
				int xMult = cctvUIBorders[i].rectTransform.pivot.x == 0 ? 1 : -1;
				int yMult = cctvUIBorders[i].rectTransform.pivot.y == 0 ? 1 : -1;

				cctvUIBorders[i].rectTransform.anchoredPosition = new Vector2(23.5f * xMult, 26.5f * yMult);
				cctvUIBorders[i].rectTransform.localScale = Vector3.one;
				cctvUIBorders[i].color = targetColor;
			}

			hackableName.color = targetColor;

			for (int i = 0; i < unhackInstructions.Length; i++) unhackInstructions[i].color = defaultGraphicsColor[i];

			uiFadeInProgress = false;
			action -= LerpUITemplate;
		}
		else if (uiLerpTime <= 0 && !uiFadeIn)
		{
			foreach (Image cctvUIBorder in cctvUIBorders)
			{
				cctvUIBorder.rectTransform.anchoredPosition = Vector2.zero;
				cctvUIBorder.rectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
				cctvUIBorder.color = Color.clear;
			}

			hackableName.color = Color.clear;

			foreach (Graphic graphic in unhackInstructions) graphic.color = Color.clear;

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
		Cursor.visible = isPaused ? true : player.inSpInteraction ? true : isPaused;
		Cursor.lockState = isPaused ?  CursorLockMode.None : player.inSpInteraction ? CursorLockMode.None : CursorLockMode.Locked;
		Time.timeScale = isPaused ? 0 : 1;
	}

	public void Options()
	{
		optionsScreen.gameObject.SetActive(!optionsScreen.gameObject.activeSelf);
	}

	public void MainMenu ()
	{
		Time.timeScale = 1;
		LoadingScreen.inst.LoadScene("Main Menu");
		//SceneManager.LoadScene ("Main Menu", LoadSceneMode.Single);
	}

	public void LoadCheckpoint()
	{
		Time.timeScale = 1;
		LoadingScreen.inst.LoadScene(SceneManager.GetActiveScene().name);
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}

	public void Restart()
	{
		//Need a Proper Respawn
		Time.timeScale = 1;
		PlayerPrefs.DeleteKey("Checkpoint");
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
	#endregion
}
