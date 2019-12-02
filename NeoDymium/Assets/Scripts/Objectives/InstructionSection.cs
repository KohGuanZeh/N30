using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        if (other.tag == "Player" || other.tag == "Hackable")
        {
            uIManager.currentHint.text = string.Empty;
            iM.instructionImage.sprite = instructionPictureToDisplay;
            iM.instructionText.text = instructionToDisplay;
            iM.WhileInInstructionScreen();
            gameObject.SetActive (false);
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.tag == "Player" || other.tag == "Hackable")
        {
            gameObject.SetActive (false);
        }
    }
}
