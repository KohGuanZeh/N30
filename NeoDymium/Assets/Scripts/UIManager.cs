using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
	public GameObject mainObj;
	public GameObject pauseObj;

    public TextMeshProUGUI healthText;

	PlayerShooterController player;

	void Start () 
	{
		player = PlayerShooterController.inst;
		mainObj.SetActive (true);
		pauseObj.SetActive (false);
	}
	
	void Update () 
	{
		if (Input.GetKeyDown (key: KeyCode.Escape)) 
			PausePlay ();

		healthText.text = player.currentHealth + "/" + player.maxHealth;
	}

	public void PausePlay () 
	{
		mainObj.SetActive (!mainObj.activeSelf);
		pauseObj.SetActive (!pauseObj.activeSelf);
		Cursor.lockState = mainObj.activeSelf ? CursorLockMode.Locked : CursorLockMode.None;
		Time.timeScale = mainObj.activeSelf ? 1 : 0;
	}

	public void MainMenu () 
	{
		SceneManager.LoadScene ("Main Menu");
	}
}