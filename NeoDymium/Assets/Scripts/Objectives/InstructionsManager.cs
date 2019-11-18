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
    public bool lockCameraRotation = false; //Disable Rotation/Movement/Sound of Camera during Instruction Screen
    public bool closeTimerStart;
    public float closeTimerValue;
    public static InstructionsManager inst;
    PlayerController player;
    UIManager uIManager;

    void Awake ()
    {
        inst = this;
    }

    void Start ()
    {
        player = PlayerController.inst;
        uIManager = UIManager.inst;
    }

    void Update ()
    {
        if (inInstruction)
        {
            Time.timeScale = 0;
            lockCameraRotation = true;
            if (Input.GetMouseButton(0) && !uIManager.isPaused && !closeTimerStart)
            {
                Time.timeScale = 1;
                lockCameraRotation = false;
                inInstruction = false;
                closeTimerValue = 0.0f;
                if (instructionHolder.activeInHierarchy) instructionHolder.SetActive (false);
            }
        } 
        if (closeTimerStart)
        {
            closeTimerValue += Time.unscaledDeltaTime;
            if (closeTimerValue >= 3.0f)
            {
                closeTimerStart = false;
            }
        }
    }

    public void WhileInInstructionScreen ()
    {
        inInstruction = true;
        closeTimerStart = true;
        if (!instructionHolder.activeInHierarchy) instructionHolder.SetActive(true);
    }
}
