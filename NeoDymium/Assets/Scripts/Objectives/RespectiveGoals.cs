using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespectiveGoals : MonoBehaviour
{
    public bool isCompleted = false;
    public bool forTriggers;
    public string currentGoal;
    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player" && forTriggers)
        {
            isCompleted = true;
        }
    }
}
