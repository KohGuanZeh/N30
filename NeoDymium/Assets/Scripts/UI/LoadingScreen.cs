﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

public class LoadingScreen : MonoBehaviour
{
	[Header ("For Loading")]
	public static LoadingScreen inst;
	public bool isLoading;
	[SerializeField] AsyncOperation async;
	[SerializeField] string levelToLoad;

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

	private void Awake()
	{
		//Singleton
		if (inst) Destroy(gameObject);
		else
		{
			inst = this;
			DontDestroyOnLoad(gameObject);
		}

		bg.color = loadingTxt.color = Color.clear;
		foreach (Image loadingIcon in loadingIcons) loadingIcon.color = Color.clear;
		bg.gameObject.SetActive(false);
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

	public void LoadScene(string levelName)
	{
		bg.gameObject.SetActive(true);
		levelToLoad = levelName;

		isLoading = true;
		fadeIn = true;
		action += FadeInFadeOut;
		action += RotateIcon;
	}

	public void OnSceneLoaded()
	{
		action -= FadeInFadeOut; //Remove first to prevent errors.

		fadeIn = false;
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
		async = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Single);
	}

	public void OnFadeOut()
	{
		isLoading = false;
		bg.gameObject.SetActive(false);

		action -= RotateIcon;
		foreach (Image loadingIcon in loadingIcons) loadingIcon.rectTransform.eulerAngles = Vector3.zero;
	}
}