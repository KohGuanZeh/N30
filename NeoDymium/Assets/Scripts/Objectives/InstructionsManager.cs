using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionsManager : MonoBehaviour
{
    public GameObject instructionHolder;
    public Image instructionImage;
    public TextMeshProUGUI instructionText;
    public bool inInstruction = false;
    public bool lockCameraRotation = false; //Disable Rotation of Camera during Instruction Screen
    public static InstructionsManager inst;
    PlayerController player;

    void Awake ()
    {
        inst = this;
        player = PlayerController.inst;
    }

    void Update ()
    {
        if (inInstruction)
        {
            Time.timeScale = 0;
            lockCameraRotation = true;
            if (Input.GetMouseButton(0))
            {
                Time.timeScale = 1;
                lockCameraRotation = false;
                inInstruction = false;
                if (instructionHolder.activeInHierarchy) instructionHolder.SetActive (false);
            }
        } 
    }

    public void WhileInInstructionScreen ()
    {
        inInstruction = true;
        if (!instructionHolder.activeInHierarchy) instructionHolder.SetActive(true);
    }
}
