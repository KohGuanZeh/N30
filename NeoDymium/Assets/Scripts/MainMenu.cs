using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[Header("Menus")] //Using Rect Transform in case there needs to be animation in the Future
	[SerializeField] RectTransform optionsScreen;
	[SerializeField] RectTransform creditsScreen;

	void Start () 
	{

	}

    public void Play() 
	{
		SceneManager.LoadScene ("Office");
	}

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