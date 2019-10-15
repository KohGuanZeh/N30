using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
	[Header ("For Loading")]
	public static LoadingScreen inst;
	public bool isLoading;
	[SerializeField] AsyncOperation async;

	[Header("Loading Screen Items")]
	[SerializeField] Image bg;
	[SerializeField] Color bgDefaultColor = new Color(0.1f, 0.1f, 0.1f, 1);
	[SerializeField] Image loadingIcon;
	[SerializeField] Image loadingTxt;

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
	}

	private void Update()
	{
		if (action != null) action();

		if (async != null)
		{
			if (async.isDone) OnSceneLoaded();
		}
	}

	public void LoadNextScene(string levelName)
	{
		async = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);

		isLoading = true;
		fadeIn = true;
		action += FadeInFadeOut;
		action += FadeInFadeOut;
		action += FadeInFadeOut;
		action += FadeInFadeOut;
		action += FadeInFadeOut;
		action += FadeInFadeOut;
		action += RotateIcon;

		print(string.Format("Action has {0} functions suscribed to it", action.GetInvocationList().Length)); 
	}

	public void OnSceneLoaded()
	{
		action -= FadeInFadeOut; //Remove first to prevent errors

		fadeIn = false;
		action += FadeInFadeOut;
	}

	public void RotateIcon()
	{

	}

	public void FadeInFadeOut()
	{
		fadeLerpTime = fadeIn ? Mathf.Min(fadeLerpTime + fadeSpeed * Time.deltaTime, 1) : Mathf.Max(fadeLerpTime - fadeSpeed * Time.deltaTime, 0);

		bg.color = Color.Lerp(Color.clear, bgDefaultColor, fadeLerpTime);
		loadingIcon.color = Color.Lerp(Color.clear, Color.white, fadeLerpTime);
		loadingTxt.color = Color.Lerp(Color.clear, Color.white, fadeLerpTime);

		if (fadeIn && fadeLerpTime >= 1)
		{
			bg.color = bgDefaultColor;
			loadingIcon.color = Color.white;
			loadingTxt.color = Color.white;
			OnFadeIn();

			action -= FadeInFadeOut;
		}
		else if (!fadeIn && fadeLerpTime <= 0)
		{
			bg.color = Color.clear;
			loadingIcon.color = Color.clear;
			loadingTxt.color = Color.clear;
			OnFadeOut();

			action -= FadeInFadeOut;
		}
	}

	public void OnFadeIn()
	{

	}

	public void OnFadeOut()
	{
		isLoading = false;

		action -= RotateIcon;
		//Set Rotation of Icon back to Original Rotation
	}
}
