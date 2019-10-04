using UnityEngine;
using TMPro;

public class DeskInfo : MonoBehaviour
{
	public TextMeshProUGUI employeeName;
	public TextMeshProUGUI age;
	public TextMeshProUGUI jobPosition;
	public TextMeshProUGUI password;

	PlayerController player;

	void Start ()
	{
		player = PlayerController.inst;
	}

	void Update ()
	{
		transform.LookAt (player.CurrentViewingCamera.transform.position);
	}
}