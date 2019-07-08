using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
	public GameObject pauseObj;

    public TextMeshProUGUI healthText;

	PlayerShooterController player;

	void Start () 
	{
		player = PlayerShooterController.inst;
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
		pauseObj.SetActive (!pauseObj.activeSelf);
		player.paused = pauseObj.activeSelf;
		Cursor.lockState = pauseObj.activeSelf ?  CursorLockMode.None : CursorLockMode.Locked;
		Time.timeScale = pauseObj.activeSelf ? 0 : 1;
	}

	public void MainMenu () 
	{
		SceneManager.LoadScene ("Main Menu");
	}
}