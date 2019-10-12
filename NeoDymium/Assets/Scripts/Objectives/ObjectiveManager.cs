using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public GameObject[] goals;

    public TextMeshProUGUI displayCurrentGoal;

    public int currentGoalNumber;

    ScoreSystem scoreSystem;
    UIManager uiManager;

    void Awake() {
        scoreSystem = FindObjectOfType<ScoreSystem>();
        uiManager = FindObjectOfType<UIManager>();
    }
    void Start() {
        currentGoalNumber = 0;
    }

    void Update() {
        if (goals[currentGoalNumber].GetComponent<RespectiveGoals>().isCompleted)
		{
            scoreSystem.totalScore += goals[currentGoalNumber].GetComponent<RespectiveGoals>().pointsGained;
			currentGoalNumber = Mathf.Min(currentGoalNumber, goals.Length - 1); //To prevent errors!
            uiManager.SetNewObjective (goals[currentGoalNumber].transform);
        }

        //displayCurrentGoal.text = goals[currentGoalNumber].GetComponent<RespectiveGoals>().currentGoal;
    }
}
