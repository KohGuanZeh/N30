using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject menuObj;
	public GameObject creditsObj;

	void Start () 
	{
		menuObj.SetActive (true);
		creditsObj.SetActive (false);
	}

    public void Play () 
	{
		SceneManager.LoadScene ("playground");
	}

	public void Credits () 
	{
		menuObj.SetActive (!menuObj.activeSelf);
		creditsObj.SetActive (!creditsObj.activeSelf);
	}

	public void Quit () 
	{
		Application.Quit ();
	}
}