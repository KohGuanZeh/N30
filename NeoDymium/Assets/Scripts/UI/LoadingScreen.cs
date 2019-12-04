using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

public class LoadingScreen : MonoBehaviour
{
	[Header("For Loading")]
	public static LoadingScreen inst;
	public bool isLoading;
	[SerializeField] AsyncOperation async;
	[SerializeField] string levelToLoad;
	[SerializeField] int levelIdxToLoad = -1;

	[Header("Loading Screen Items")]
	[SerializeField] Image bg;
	[SerializeField] Color bgDefaultColor = new Color(0.1f, 0.1f, 0.1f, 1);
	[SerializeField] Image[] loadingIcons;
	[SerializeField] float[] rotationSpeeds;
	[SerializeField] TextMeshProUGUI loadingTxt;

	[Header("For Lerping")]
	[SerializeField] bool fadeIn;
	[SerializeField] float fadeSpeed = 2;
	[SerializeField] float fadeLerpTime;
	[SerializeField] Action action;

	[Header("For Any Scripts that require Real Time")]
	Stopwatch watch;

	private void Awake()
	{
		//Singleton
		if (inst) Destroy(gameObject);
		else
		{
			inst = this;
			DontDestroyOnLoad(gameObject);
		}

		watch = new Stopwatch();
		watch.Start();

		bg.color = loadingTxt.color = Color.clear;
		foreach (Image loadingIcon in loadingIcons) loadingIcon.color = Color.clear;
		bg.gameObject.SetActive(false);

		levelIdxToLoad = -1;
		levelToLoad = string.Empty;
	}

	private void Update()
	{
		if (action != null) action();

		if (async != null)
		{
			if (async.isDone)
			{
				OnSceneLoaded();
				async = null;
			}
		}
	}

	//Meant for Transition for Linear Level Progression
	public void AutoLoadNextScene()
	{
		int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
		if (nextLevelIndex == SceneManager.sceneCountInBuildSettings)
		{
			PlayerPrefs.DeleteKey("Scene Index");
			nextLevelIndex = 0;
		}
		else PlayerPrefs.SetInt("Scene Index", nextLevelIndex);

		LoadScene(nextLevelIndex); //If current scene index is the last one. Load main menus
	}

	public void LoadScene(string levelName)
	{
		if (isLoading) return;
		bg.gameObject.SetActive(true);
		levelToLoad = levelName;

		isLoading = true;
		fadeIn = true;
		action += FadeInFadeOut;
		action += RotateIcon;
		print("Loading Scene Through String");
	}

	public void LoadScene(int levelIndex)
	{
		if (isLoading) return;
		bg.gameObject.SetActive(true);
		levelIdxToLoad = levelIndex;

		isLoading = true;
		fadeIn = true;
		action += FadeInFadeOut;
		action += RotateIcon;
		print("Loading Scene Through Int");
	}

	public void OnSceneLoaded()
	{
		//Save Level Key whenever you switch Levels
		if (SceneManager.GetActiveScene().buildIndex != 0) 

		action -= FadeInFadeOut; //Remove first to prevent errors.

		fadeIn = false;
		watch.Restart();
		action += FadeInFadeOut;
	}

	public void RotateIcon()
	{
		for (int i = 0; i < loadingIcons.Length; i++) loadingIcons[i].rectTransform.Rotate(new Vector3(0, 0, rotationSpeeds[i] * Time.deltaTime));
	}

	public void FadeInFadeOut()
	{
		fadeLerpTime = fadeIn ? Mathf.Min(fadeLerpTime + fadeSpeed * Time.deltaTime, 1) : Mathf.Max(fadeLerpTime - fadeSpeed * Time.deltaTime, 0);

		bg.color = Color.Lerp(Color.clear, bgDefaultColor, fadeLerpTime);
		foreach (Image loadingIcon in loadingIcons) loadingIcon.color = Color.Lerp(Color.clear, Color.white, fadeLerpTime);
		loadingTxt.color = Color.Lerp(Color.clear, Color.white, fadeLerpTime);

		if (fadeIn && fadeLerpTime >= 1)
		{
			bg.color = bgDefaultColor;
			foreach (Image loadingIcon in loadingIcons) loadingIcon.color = Color.white;
			loadingTxt.color = Color.white;
			OnFadeIn();

			action -= FadeInFadeOut;
		}
		else if (!fadeIn && fadeLerpTime <= 0)
		{
			bg.color = Color.clear;
			foreach (Image loadingIcon in loadingIcons) loadingIcon.color = Color.clear;
			loadingTxt.color = Color.clear;
			OnFadeOut();

			action -= FadeInFadeOut;
		}
	}

	public void OnFadeIn()
	{
		if (!string.IsNullOrEmpty(levelToLoad)) async = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Single);
		else if (levelIdxToLoad > -1) async = SceneManager.LoadSceneAsync(levelIdxToLoad, LoadSceneMode.Single);
		else UnityEngine.Debug.LogError("Invalid String or Int Loaded");
	}

	public void OnFadeOut()
	{
		isLoading = false;
		bg.gameObject.SetActive(false);

		levelToLoad = string.Empty;
		levelIdxToLoad = -1;

		action -= RotateIcon;
		foreach (Image loadingIcon in loadingIcons) loadingIcon.rectTransform.eulerAngles = Vector3.zero;
	}

	public void PausePlayWatch(bool pause)
	{
		if (pause) watch.Stop();
		else watch.Start();
	}

	public double GetTimeElapsed()
	{
		return watch.Elapsed.TotalSeconds;
	}
}
