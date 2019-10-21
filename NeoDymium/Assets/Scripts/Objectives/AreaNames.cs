using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreaNames : MonoBehaviour
{
    public string currentAreaName;
    public bool fadeNow = false;
    AreaNamesManager areaNamesManager;
    public static AreaNames inst;

    void Awake ()
    {
        inst = this;
    }

    void Start ()
    {
        areaNamesManager = AreaNamesManager.inst;
    }

    void Update ()
    {
        if (fadeNow == true)
        {
            areaNamesManager.StartCoroutine("FadeInFadeOut");
            fadeNow = false;
        }
    }
}
