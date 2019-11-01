using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAreaTrigger : MonoBehaviour
{
    UIManager uIManager;
    
    void Start ()
    {
        uIManager = UIManager.inst;
    }
    void OnTriggerEnter (Collider other)
    {
        uIManager.currentHint.text = string.Empty;
    }
    void OnTriggerStay (Collider other)
    {
        if (other.tag == "Player")
        {
            RespectiveGoals goal = GetComponent<RespectiveGoals>();
			if (goal) goal.isCompleted = true;
        }
    }
}
