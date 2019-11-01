using UnityEngine;

public class TutorialSection : MonoBehaviour
{
    [TextArea (5, 20)]
    public string textToDisplay;
    TutorialManager tutorialManager;
    UIManager uIManager;
    bool tutHasFinished;
    void Awake ()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }
    void Start ()
    {
        uIManager = UIManager.inst;
    }
    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Player")
            tutorialManager.DiscountCoroutine (gameObject, textToDisplay);
    }
    void OnTriggerStay (Collider other)
    {
        if (!tutHasFinished)
        {
            if (Input.GetKeyDown (KeyCode.LeftControl))
            {
                uIManager.currentHint.text = string.Empty;
                tutHasFinished = true;
            }
        }
    }
}