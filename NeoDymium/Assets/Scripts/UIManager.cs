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

	[Header ("Menus")]
	[SerializeField] RectTransform pauseScreen;
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform gameOverScreen;

	[Header("Player UI")]
	[SerializeField] bool detectedHackable;
	[SerializeField] Image crosshair;
	[SerializeField] float crosshairLerpTime;
	[SerializeField] bool crosshairIsLerping;

	[SerializeField] Image stealthGauge;

	[SerializeField] Image animatedRing;
	[SerializeField] float hackingLerpTime;

	[SerializeField] bool showError;
	[SerializeField] Image errorScreen, errorLineImg;
	[SerializeField] Material errorLineMat;
	[SerializeField] float errorScreenLerpTime;

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

		errorLineMat = new Material(errorLineImg.material);
		errorLineImg.material = errorLineMat;
	}

	void Update () 
	{
		stealthGauge.fillAmount = (player.stealthGauge / player.stealthThreshold);
		if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver) PausePlay();
		if (Input.GetKeyDown(KeyCode.P)) ForcedUnhack();

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

	public void AimFeedback(bool detected)
	{
		if (detected && !detectedHackable) detectedHackable = true;
		else if (!detected && detectedHackable) detectedHackable = false;
		else return;

		if (crosshairIsLerping) return;

		crosshairIsLerping = true;
		action += LerpAimFeedback;
	}

	public void StartHacking()
	{
		action += Hacking;
		animatedRing.gameObject.SetActive(true);
	}

	public void ForcedUnhack()
	{
		errorScreenLerpTime = 0;
		errorScreen.rectTransform.localScale = new Vector2(1, 0);
		errorLineImg.material.SetTextureOffset("_MainTex", Vector2.zero);
		errorScreen.gameObject.SetActive(true);
		action += ErrorMsg;
	}

	//GUI Animations
	void LerpAimFeedback()
	{
		crosshairLerpTime = detectedHackable ? Mathf.Min(crosshairLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(crosshairLerpTime - Time.deltaTime * 5, 0);
		crosshair.rectTransform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.5f, 0.5f, 05f), crosshairLerpTime);

		if (crosshairLerpTime >= 1 && detectedHackable)
		{
			crosshair.rectTransform.localScale = new Vector3(0.5f, 0.5f, 05f);
			crosshairIsLerping = false;
			action -= LerpAimFeedback;
		}
		else if (crosshairLerpTime <= 0 && !detectedHackable)
		{
			crosshair.rectTransform.localScale = Vector3.one;
			crosshairIsLerping = false;
			action -= LerpAimFeedback;
		}
	}

	void Hacking()
	{
		hackingLerpTime += Time.deltaTime * 5;
		animatedRing.rectTransform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(20, 20, 20), hackingLerpTime);

		if (hackingLerpTime >= 1)
		{
			action -= Hacking;
			hackingLerpTime = 0;
			animatedRing.gameObject.SetActive(false);
			animatedRing.rectTransform.localScale = Vector3.one;
		}
	}

	void ErrorMsg()
	{
		errorScreenLerpTime = Mathf.Min(errorScreenLerpTime + Time.deltaTime, 1);
		float errorScreenExpandTime = Mathf.Min(errorScreenLerpTime, 0.3f) / 0.3f;

		errorScreen.rectTransform.localScale = Vector2.Lerp(new Vector2(1,0), Vector2.one, errorScreenExpandTime);
		Vector2 offset = Vector2.Lerp(Vector2.zero, new Vector2(0, -5), errorScreenLerpTime);
		errorLineImg.material.SetTextureOffset("_MainTex", offset);

		if (errorScreenLerpTime >= 1)
		{
			action -= ErrorMsg;
			errorScreen.gameObject.SetActive(false);
		}
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