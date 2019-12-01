using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

public class MainMenu : MonoBehaviour
{
	[Header("Menus")] //Using Rect Transform in case there needs to be animation in the Future
	[SerializeField] RectTransform backgroundOverlay;
	[SerializeField] float backgroundOverlayDefaultXSize;
	[SerializeField] Image mainMenuOverlay;
	[SerializeField] float overlayLerpTime;
	[SerializeField] bool isOpened; //Check if Settings/Credits Page is opened
	[SerializeField] bool isLerping;
	[SerializeField] RectTransform settingsContent;
	[SerializeField] RectTransform creditsContent;

	[SerializeField] Image gameTitle;
	[SerializeField] RectTransform buttonParent;
	[SerializeField] Button[] mainMenuButtons;

	[SerializeField] Action lerpFunctions;

	AudioSource audioSource;

	void Start () 
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		backgroundOverlayDefaultXSize = backgroundOverlay.sizeDelta.x;
		backgroundOverlay.sizeDelta = new Vector2(0, backgroundOverlay.sizeDelta.y);

		mainMenuOverlay.color = Color.clear;

		mainMenuButtons = buttonParent.GetComponentsInChildren<Button>();
		audioSource = GetComponent<AudioSource> ();
	}

	private void Update()
	{
		if (lerpFunctions != null) lerpFunctions();
	}

	void LerpBackgroundOverlay()
	{
		overlayLerpTime = isOpened ? Mathf.Min(overlayLerpTime + Time.deltaTime * 5, 1) : Mathf.Max(overlayLerpTime - Time.deltaTime * 5, 0);
		float x = Mathf.Lerp(0, backgroundOverlayDefaultXSize, overlayLerpTime);
		backgroundOverlay.sizeDelta = new Vector2(x, backgroundOverlay.sizeDelta.y);

		mainMenuOverlay.color = Color.Lerp(Color.clear, new Color(0, 0, 0, 0.75f), overlayLerpTime);

		float alpha = 1 - overlayLerpTime;
		foreach (Button mainMenuButton in mainMenuButtons) mainMenuButton.targetGraphic.color = new Color(mainMenuButton.targetGraphic.color.r, mainMenuButton.targetGraphic.color.g, mainMenuButton.targetGraphic.color.b, alpha);
		gameTitle.color = new Color(gameTitle.color.r, gameTitle.color.g, gameTitle.color.b, alpha);

		if (overlayLerpTime >= 1 && isOpened)
		{
			backgroundOverlay.sizeDelta = new Vector2(backgroundOverlayDefaultXSize, backgroundOverlay.sizeDelta.y);
			mainMenuOverlay.color = new Color(0, 0, 0, 0.75f);
			foreach (Button mainMenuButton in mainMenuButtons) mainMenuButton.targetGraphic.color = ColorUtils.ChangeAlpha(mainMenuButton.targetGraphic.color, 0);
			isLerping = false;
			lerpFunctions -= LerpBackgroundOverlay;
		}
		else if (overlayLerpTime <= 0 && !isOpened)
		{
			backgroundOverlay.sizeDelta = new Vector2(0, backgroundOverlay.sizeDelta.y);
			mainMenuOverlay.color = Color.clear;
			foreach (Button mainMenuButton in mainMenuButtons)
			{
				mainMenuButton.interactable = true;
				mainMenuButton.targetGraphic.color = new Color(mainMenuButton.targetGraphic.color.r, mainMenuButton.targetGraphic.color.g, mainMenuButton.targetGraphic.color.b, 1);
			} 
			OnScreenClose();
			isLerping = false;
			lerpFunctions -= LerpBackgroundOverlay;
		}
	}

	void OnScreenClose()
	{
		settingsContent.gameObject.SetActive(false);
		creditsContent.gameObject.SetActive(false);
	}

    public void Play(bool newGame) 
	{
		if (isLerping) return;
		if (newGame) PlayerPrefs.DeleteAll();
		audioSource.Play ();
		LoadingScreen.inst.LoadScene("Tutorial 1");
		//SceneManager.LoadScene ("Office", LoadSceneMode.Single);
	}

	//May not be needed
	public void Settings()
	{
		if (isLerping) return;

		isOpened = !isOpened;

		if (isOpened)
		{
			settingsContent.gameObject.SetActive(true);
			foreach (Button mainMenuButton in mainMenuButtons) mainMenuButton.interactable = false;
		}
		isLerping = true;

		lerpFunctions += LerpBackgroundOverlay;
		audioSource.Play ();
	}

	public void Credits() 
	{
		if (isLerping) return;

		isOpened = !isOpened;

		if (isOpened)
		{
			creditsContent.gameObject.SetActive(true);
			foreach (Button mainMenuButton in mainMenuButtons) mainMenuButton.interactable = false;
		} 
		isLerping = true;

		lerpFunctions += LerpBackgroundOverlay;
		audioSource.Play ();
	}

	public void Quit() 
	{
		if (isLerping) return;
		audioSource.Play ();

		Application.Quit ();
	}
}