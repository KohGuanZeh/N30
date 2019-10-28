using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSection : MonoBehaviour
{
    public int tutorialSectionNumber;
    TutorialManager tutorialManager;
    void Awake ()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }
    void OnTriggerStay (Collider other)
    {
        if (other.tag == "Player")
        {
            tutorialManager.currentTutoriaSectionNumber = tutorialSectionNumber;
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.tag == "Player")
        {
            tutorialManager.currentTutoriaSectionNumber = 0;
            gameObject.SetActive (false);
        }
    }
}
