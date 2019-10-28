using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAreaTrigger : MonoBehaviour
{
    void OnTriggerStay (Collider other)
    {
        if (other.tag == "Player")
        {
            RespectiveGoals goal = GetComponent<RespectiveGoals>();
			if (goal) goal.isCompleted = true;
        }
    }
}
