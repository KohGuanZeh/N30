using UnityEngine;
using TMPro;

[System.Serializable]
public struct Goal
{
	public RespectiveGoals goal;
	public Vector3 offset;
}

public class ObjectiveManager : MonoBehaviour
{
	public Goal[] goal;
    public TextMeshProUGUI displayCurrentGoal;
    public GameObject[] mainObjectives;
    public int currentMainObjNumber;
    public int currentGoalNumber;
    public bool overrideGoalNumber;

    UIManager uiManager;
    public static ObjectiveManager inst;

    void Awake ()
    {
        inst = this;
    }

    void Start() 
    {
        if (PlayerPrefs.HasKey ("Last Objective Saved")) 
        {
            currentGoalNumber = PlayerPrefs.GetInt ("Last Objective Saved");
        }
        else currentGoalNumber = 0;

        if (overrideGoalNumber)
            currentGoalNumber = 0;

        if (currentGoalNumber >= goal.Length)
            currentGoalNumber = 0;
            
        uiManager = UIManager.inst;
    }

    void Update() 
    {
        if (Input.GetKeyDown (KeyCode.O))
            currentGoalNumber = 0;

        if (goal.Length == 0 || currentGoalNumber >= goal.Length)
            return;

		if (uiManager.objective == Vector3.zero)
			uiManager.SetNewObjective (goal[currentGoalNumber].goal.transform.position + goal[currentGoalNumber].offset, true);

        if (goal[currentGoalNumber].goal.isCompleted)
		{
            currentGoalNumber++;
			if (currentGoalNumber >= goal.Length)
				return;
            else if (currentGoalNumber == 0)
                return;
			//currentGoalNumber = Mathf.Min(currentGoalNumber, goal.Length - 1); //To prevent errors!
            uiManager.SetNewObjective (goal[currentGoalNumber].goal.transform.position + goal[currentGoalNumber].offset);
        }

        displayCurrentGoal.text = goal[currentGoalNumber].goal.currentGoal;
    }
}
