using UnityEngine;
using TMPro;

[System.Serializable]
public struct TutorialSegment
{
	public bool triggered; //put to false at start
	[TextArea (5, 20)]
	public string description;
}

public class NewTutorial : MonoBehaviour
{
	public TutorialSegment[] tutorials;
	[Space (10)]
	public TextMeshProUGUI descriptionText;
	public GameObject tutorialObj;

	int currentTutorialIndex = -1;

	void Start ()
	{
		tutorialObj.SetActive (false);
	}

	public void TutorialStart (int index)
	{
		if (!tutorials[index].triggered)
		{
			currentTutorialIndex = index;
			tutorials[index].triggered = true;
			descriptionText.text = tutorials[index].description;
			tutorialObj.SetActive (true);
		}
	}

	public void TutorialEnd (int index)
	{
		if (index == currentTutorialIndex)
			tutorialObj.SetActive (false);
	}
}