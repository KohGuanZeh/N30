using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;

	PlayerController player;

	void Start () 
	{
		player = PlayerController.inst;
	}
	
	void Update () 
	{
		healthText.text = player + "/" + player;
	}
}