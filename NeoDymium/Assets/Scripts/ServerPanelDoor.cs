using UnityEngine;

public class ServerPanelDoor : MonoBehaviour
{
    public string password;
	public Transform[] deskPositions;
	public DeskInfo deskInfoTemplate;

	//for randoming of the personal info, age and password will be random
	public string[] possibleNames;
	public string[] possibleJobPositions; //dont include IT guy in array

	void Start ()
	{
		int actualPasswordIndex = Random.Range (0, deskPositions.Length);
		password = Random.Range (0, 1000).ToString ("000");

		for (int i = 0; i < deskPositions.Length; i++)
		{
			DeskInfo info = Instantiate (deskInfoTemplate, deskPositions[i].position, Quaternion.identity, deskPositions[i]);
			info.employeeName.text = "Name: " + possibleNames[Random.Range (0, possibleNames.Length)];
			info.age.text = "Age: " + Random.Range (0, 100).ToString ();

			if (i == actualPasswordIndex)
			{
				info.jobPosition.text = "Job: IT Personnel";
				info.password.text = "Password: " + password;
			}
			else
			{
				info.jobPosition.text = "Job: " + possibleJobPositions[Random.Range (0, possibleJobPositions.Length)];
				info.password.text = "Password: " + Random.Range (0, 1000).ToString ("000");
			}
		}
	}
}