using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public GameObject[] goals;
    public TextMeshProUGUI displayCurrentGoal;
    public GameObject[] mainObjectives;
    public int currentMainObjNumber;
    public int currentGoalNumber;

    UIManager uiManager;

    void Start() 
    {
        currentGoalNumber = 0;
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update() 
    {
        if (goals[currentGoalNumber].GetComponent<RespectiveGoals>().isCompleted)
		{
            currentGoalNumber++;
			currentGoalNumber = Mathf.Min(currentGoalNumber, goals.Length - 1); //To prevent errors!
            uiManager.SetNewObjective (goals[currentGoalNumber].transform);
        }

        displayCurrentGoal.text = goals[currentGoalNumber].GetComponent<RespectiveGoals>().currentGoal;
    }
}
