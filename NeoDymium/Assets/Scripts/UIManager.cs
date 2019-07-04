using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;

	PlayerShooterController player;

	void Start () 
	{
		player = PlayerShooterController.inst;
	}
	
	void Update () 
	{
		healthText.text = player.currentHealth + "/" + player.maxHealth;
	}
}