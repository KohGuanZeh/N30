using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    UIManager uIManager;
    IEnumerator currentCoroutine;

    void Start ()
    {
        uIManager = UIManager.inst;
    }

    public void DiscountCoroutine (GameObject obj, string text)
    {
        if (currentCoroutine != null)
            StopCoroutine (currentCoroutine);
        currentCoroutine = TutorialShowDuration (obj, text);
        StartCoroutine (currentCoroutine);
    }

    IEnumerator TutorialShowDuration (GameObject obj, string text)
    {
        obj.SetActive (false);
        uIManager.currentHint.text = text;
        yield return new WaitForSeconds (10.0f);
        currentCoroutine = null;
        uIManager.currentHint.text = string.Empty;
    }
}