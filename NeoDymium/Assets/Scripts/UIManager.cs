using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;

	Player player;

	void Start () 
	{
		player = Player.inst;
	}
	
	void Update () 
	{
		healthText.text = player.currentHealth + "/" + player.maxHealth;
	}
}