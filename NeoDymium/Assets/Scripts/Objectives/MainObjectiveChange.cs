using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainObjectiveChange : MonoBehaviour
{
    ObjectiveManager objectiveManager;

    void Start ()
    {
        objectiveManager = FindObjectOfType<ObjectiveManager>();
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            objectiveManager.mainObjectives[objectiveManager.currentMainObjNumber].SetActive (false);
            objectiveManager.currentMainObjNumber++;
            objectiveManager.mainObjectives[objectiveManager.currentMainObjNumber].SetActive (true);
            gameObject.SetActive (false);
        }
    }
}
