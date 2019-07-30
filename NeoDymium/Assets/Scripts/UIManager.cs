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
	[SerializeField] Image stealthGauge;
	[SerializeField] Image[] rings;
	[SerializeField] float[] rotationSpeeds;
	[SerializeField] Image animatedRing;
	[SerializeField] float lerpTime;

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
	}

	void Update () 
	{
		stealthGauge.fillAmount = (player.stealthGauge / player.stealthThreshold);
		if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver) PausePlay();

		RotateRings();

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

	public void RotateRings()
	{
		for (int i = 0; i < rings.Length; i++)
		{
			rings[i].rectTransform.Rotate(0, 0, rotationSpeeds[i] * Time.deltaTime);
		}
	}

	public void StartHacking()
	{
		action += Hacking;
		animatedRing.gameObject.SetActive(true);
	}

	//For UI Hacking
	void Hacking()
	{
		lerpTime += Time.deltaTime * 5;
		animatedRing.rectTransform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(20, 20, 20), lerpTime);

		if (lerpTime >= 1)
		{
			action -= Hacking;
			lerpTime = 0;
			animatedRing.gameObject.SetActive(false);
			animatedRing.rectTransform.localScale = Vector3.one;
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