using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
	[Header ("General Properties")]
	public static UIManager inst;
	[SerializeField] PlayerController player;
	[SerializeField] Animator guiAnim; //Stand in for now... Will think about how to better integrate Animations

	[Header ("Menus")]
	[SerializeField] RectTransform pauseScreen;
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform gameOverScreen;

	[Header("Crosshair")]
	[SerializeField] bool isFocusing; //Check if a Hackable or Interactable Object has been focused on
	[SerializeField] Image[] rings; //For Rotation of Rings. 0 is Inner, 1 is Middle, 2 is Outer
	[SerializeField] float[] ringRotSpeeds; //For Rotation of Rings
	[SerializeField] float crosshairLerpTime;
	[SerializeField] bool crosshairIsLerping; //Check if the Focus Animation is Ongoing

	[Header ("Stealth Gauge")]
	[SerializeField] Image stealthGauge;
	[SerializeField] bool displayWarning;
	[SerializeField] Image detectedWarning;

	[Header("Instructions and Error Msgs")]
	[SerializeField] Sprite[] controlsSprites; //Mouse Click is 0, E is 1
	[SerializeField] TextMeshProUGUI controlsInfo;
	[SerializeField] Image controlsIcon;
	[SerializeField] RectTransform controlsBackDrop;
	[SerializeField] TextMeshProUGUI errorMsg;
	[SerializeField] float instructionsLerpTime;

	[Header("Game States")]
	//May want to use Enum for Game States
	public bool isGameOver;
	public bool isPaused;

	public Action action;

	private void Awake()
	{
		inst = this;
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

		Color startColor = Color.clear;
		controlsBackDrop.anchoredPosition = new Vector2(125, 0);
		controlsInfo.color = startColor;
		controlsIcon.color = startColor;
		errorMsg.color = startColor;

		action += LerpInstructions; //Instructions will only Lerp based on Boolean
	}

	void Update () 
	{
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

	public void Focus(bool detected)
	{
		if (detected && !isFocusing) isFocusing = true;
		else if (!detected && isFocusing) isFocusing = false;
		else return;

		if (crosshairIsLerping) return;

		rings[0].gameObject.SetActive(true);
		rings[2].gameObject.SetActive(true);
		crosshairIsLerping = true;
		action += LerpFocusFeedback;
	}

	public void DisplayInstructionsAndErrors(bool isHackableObj, string errorTxt = "")
	{
		if (isHackableObj)
		{
			controlsIcon.sprite = controlsSprites[0];
			controlsInfo.text = "Hack";
		}
		else
		{
			controlsIcon.sprite = controlsSprites[1];
			controlsInfo.text = "Interact";
		}

		errorMsg.text = errorTxt;
	}

	//GUI Animations
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

	void LerpInstructions()
	{
		//Do not Lerp at all if it has reached desired Position/Color
		if ((instructionsLerpTime >= 1 && player.detected) || (instructionsLerpTime <= 0 && !player.detected)) return;

		instructionsLerpTime = player.detected ? Mathf.Min(instructionsLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(instructionsLerpTime - Time.deltaTime * 5, 0);
		controlsBackDrop.anchoredPosition = Vector2.Lerp(new Vector2(125, 0), Vector2.zero, instructionsLerpTime);

		Color infoColor = Color.Lerp(Color.clear, Color.white, instructionsLerpTime);
		controlsInfo.color = infoColor;
		controlsIcon.color = infoColor;

		errorMsg.color = Color.Lerp(Color.clear, Color.red, instructionsLerpTime);

		if (instructionsLerpTime >= 1 && player.detected)
		{
			infoColor = Color.white;
			controlsBackDrop.anchoredPosition = Vector2.zero;
			controlsInfo.color = infoColor;
			controlsIcon.color = infoColor;
			errorMsg.color = Color.red;
		}
		else if (instructionsLerpTime <= 0 && !player.detected)
		{
			infoColor = Color.clear;
			controlsBackDrop.anchoredPosition = new Vector2(125, 0);
			controlsInfo.color = infoColor;
			controlsIcon.color = infoColor;
			errorMsg.color = infoColor;
		}
	}

	//Animation Events
	public void ShowStaticScreen()
	{
		guiAnim.SetTrigger("Static");
	}

	public void StaticScreenAnimEvent()
	{
		player.ForcedUnhackAnimEvent();
	}

	//Button Functions
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

	public void Retry()
	{
		//Need a Proper Respawn
		Time.timeScale = 1;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}
}