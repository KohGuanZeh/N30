using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSection : MonoBehaviour
{
    [TextArea (5, 20)]
    public string textToDisplay;
    TutorialManager tutorialManager;
    UIManager uIManager;
    bool tutHasFinished;

    public GameObject controlKeys;
    public TextMeshProUGUI controlText;

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
        // if (other.tag == "Player")
        //     tutorialManager.DiscountCoroutine (gameObject, textToDisplay);
    }
    void OnTriggerStay (Collider other)
    {
        // if (!tutHasFinished)
        // {
        //     if (Input.GetKeyDown (KeyCode.LeftControl))
        //     {
        //         uIManager.currentHint.text = string.Empty;
        //         tutHasFinished = true;
        //     }
        // }
        if (other.tag == "Player")
        {
            
        }
    }

    void OnTriggerExit (Collider other)
    {
        // if (controlKeys.activeSelf) controlKeys.SetActive (false);
        // if (controlText.gameObject.activeSelf) controlText.gameObject.SetActive (false);
    }
}