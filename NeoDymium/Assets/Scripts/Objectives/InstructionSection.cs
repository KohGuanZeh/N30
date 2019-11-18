﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionSection : MonoBehaviour
{
    [TextArea (5, 20)]
    public string instructionToDisplay;
    public Sprite instructionPictureToDisplay;
    InstructionsManager iM;
    UIManager uIManager;

    void Start ()
    {
        iM = InstructionsManager.inst;
        uIManager = UIManager.inst;
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
        {
            uIManager.currentHint.text = string.Empty;
            iM.instructionImage.sprite = instructionPictureToDisplay;
            iM.instructionText.text = instructionToDisplay;
            iM.WhileInInstructionScreen();
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.tag == "Player")
        {
            gameObject.SetActive (false);
        }
    }
}
