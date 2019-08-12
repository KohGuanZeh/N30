using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[Header("Camera")]
	[SerializeField] int currentScreenIndex; //0 Denotes Play, 1 Denotes Options, 2 Denotes Credits
	[SerializeField] int nextScreenIndex;
	[SerializeField] Vector2 currScreenDefaultPos, nextScreenDefaultPos;
	[SerializeField] Camera mainCam;
	[SerializeField] bool isMoving;
	[SerializeField] float cameraLerpTime;
	[SerializeField] Transform startPos;
	[SerializeField] Transform[] cameraPositions;
	[SerializeField] RectTransform[] screens;
	[SerializeField] GameObject startIcon;
	[SerializeField] GameObject[] icons; //0 is Options, 1 is Credits, 2 is Play 

	[Header("Menus")] //Using Rect Transform in case there needs to be animation in the Future
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform creditsScreen;

	[SerializeField] Action action;

	void Start () 
	{
		mainCam = Camera.main;

		currentScreenIndex = -1;
		nextScreenIndex = 0;

		foreach (RectTransform screen in screens) screen.gameObject.SetActive(false);

		foreach (GameObject icon in icons) icon.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray mouseRay = mainCam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity))
			{
				if (hit.collider != null)
				{
					print("Yes");
					if (hit.collider.tag == "Hackable") ChangeCamera(hit.transform);;
				}
			}
		}

		if (action != null) action();
	}

	public void LerpToNextCamPosition()
	{
		cameraLerpTime = Mathf.Min(cameraLerpTime + Time.deltaTime * 5,  1);

		if (currentScreenIndex > -1)
		{
			mainCam.transform.position = Vector3.Lerp(cameraPositions[currentScreenIndex].position, cameraPositions[nextScreenIndex].position, cameraLerpTime);
			mainCam.transform.eulerAngles = Vector3.Lerp(cameraPositions[currentScreenIndex].eulerAngles, cameraPositions[nextScreenIndex].eulerAngles, cameraLerpTime);
			screens[nextScreenIndex].anchoredPosition = Vector2.Lerp(nextScreenDefaultPos, Vector2.zero, cameraLerpTime);
			screens[currentScreenIndex].anchoredPosition = Vector2.Lerp(Vector2.zero, currScreenDefaultPos, cameraLerpTime);
		}
		else
		{
			mainCam.transform.position = Vector3.Lerp(startPos.position, cameraPositions[nextScreenIndex].position, cameraLerpTime);
			mainCam.transform.eulerAngles = Vector3.Lerp(startPos.eulerAngles, cameraPositions[nextScreenIndex].eulerAngles, cameraLerpTime);
			screens[nextScreenIndex].anchoredPosition = Vector2.Lerp(nextScreenDefaultPos, Vector2.zero, cameraLerpTime);
		}

		if (cameraLerpTime >= 1)
		{
			mainCam.transform.position = cameraPositions[nextScreenIndex].position;
			mainCam.transform.eulerAngles = cameraPositions[nextScreenIndex].eulerAngles;
			screens[nextScreenIndex].anchoredPosition = Vector2.zero;

			if (currentScreenIndex > -1)
			{
				screens[currentScreenIndex].anchoredPosition = currScreenDefaultPos;
				screens[currentScreenIndex].gameObject.SetActive(false);
				icons[currentScreenIndex].SetActive(false);
			}
			else startIcon.gameObject.SetActive(false);

			currentScreenIndex = nextScreenIndex;
			currScreenDefaultPos = nextScreenDefaultPos;

			cameraLerpTime = 0;
			isMoving = false;

			action -= LerpToNextCamPosition;
		}
	}

	void ChangeCamera(Transform nextCam)
	{
		if (isMoving) return;

		nextScreenIndex = currentScreenIndex + 1;
		if (nextScreenIndex == screens.Length) nextScreenIndex = 0;

		screens[nextScreenIndex].gameObject.SetActive(true);
		nextScreenDefaultPos = screens[nextScreenIndex].anchoredPosition;

		icons[nextScreenIndex].SetActive(true);

		action += LerpToNextCamPosition;
	}

    public void Play() 
	{
		SceneManager.LoadScene ("Office");
	}

	//May not be needed
	public void Options()
	{
		optionsScreen.gameObject.SetActive(!optionsScreen.gameObject.activeSelf);
	}

	public void Credits() 
	{
		creditsScreen.gameObject.SetActive (!creditsScreen.gameObject.activeSelf);
	}

	public void Quit() 
	{
		Application.Quit ();
	}
}