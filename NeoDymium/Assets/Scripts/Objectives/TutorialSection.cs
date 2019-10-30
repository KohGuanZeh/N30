using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSection : MonoBehaviour
{
    public int tutorialSectionNumber;
    TutorialManager tutorialManager;
    bool CorIsRunning = false;
    void Awake ()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            tutorialManager.currentTutoriaSectionNumber = tutorialSectionNumber;
            if (CorIsRunning)
            {
                CancelInvoke ();
            }
            Invoke ("TutorialShowDuration", 5.0f);
        }
    }

    // void OnTriggerExit (Collider other)
    // {
    //     if (other.tag == "Player")
    //     {
    //         tutorialManager.currentTutoriaSectionNumber = 0;
    //         gameObject.SetActive (false);
    //     }
    // }

    void TutorialShowDuration ()
    {
        CorIsRunning = true;
        tutorialManager.currentTutoriaSectionNumber = 0;
        gameObject.SetActive (false);
        CorIsRunning = false;
    }
}
