using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAreaTrigger : MonoBehaviour
{
    UIManager uIManager;

    RespectiveGoals goal;
    
    void Start ()
    {
        uIManager = UIManager.inst;
        goal = GetComponent<RespectiveGoals>();
    }
    void OnTriggerEnter (Collider other)
    {
        uIManager.currentHint.text = string.Empty;

        if (other.tag == "Player")
        {
			goal.isCompleted = true;
        }
    }
}
