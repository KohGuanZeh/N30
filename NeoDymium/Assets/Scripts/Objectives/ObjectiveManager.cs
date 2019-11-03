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

    UIManager uiManager;

    void Start() 
    {
        currentGoalNumber = 0;
        uiManager = UIManager.inst;
		
    }

    void Update() 
    {
		if (uiManager.objective == Vector3.zero)
			uiManager.SetNewObjective (goal[0].goal.transform.position + goal[0].offset, true);

        if (goal[currentGoalNumber].goal.isCompleted)
		{
            currentGoalNumber++;
			currentGoalNumber = Mathf.Min(currentGoalNumber, goal.Length - 1); //To prevent errors!
            uiManager.SetNewObjective (goal[currentGoalNumber].goal.transform.position + goal[currentGoalNumber].offset);
        }

        displayCurrentGoal.text = goal[currentGoalNumber].goal.currentGoal;
    }
}
