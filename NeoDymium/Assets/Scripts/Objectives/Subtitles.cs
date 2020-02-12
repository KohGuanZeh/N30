using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subtitles : MonoBehaviour
{
    public string[] subtitlesList;
    UIManager uIManager;

    void Start ()
    {
        uIManager = UIManager.inst;
    }

    void OnTriggerEnter()
    {
        StartCoroutine (uIManager.ShowHideSubtitles(subtitlesList));
    }
}
