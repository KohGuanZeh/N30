using UnityEngine;
using TMPro;

[System.Serializable]
public struct TutorialSegment
{
	public bool triggered; //put to false at start
	[TextArea (5, 20)]
	public string description;
	public bool movementTut;
}

public class NewTutorial : MonoBehaviour
{
	public static NewTutorial inst;

	public TutorialSegment[] tutorials;

	int currentTutorialIndex = -1;

	UIManager ui;

	void Awake ()
	{
		inst = this;
	}

	void Start ()
	{
		ui = UIManager.inst;
	}

	public void TutorialStart (int index)
	{
		if (!tutorials[index].triggered)
		{
			currentTutorialIndex = index;
			tutorials[index].triggered = true;
			if (!tutorials[index].movementTut) 
				ui.ShowHideTutorial (true, tutorials[index].description, false);
			else 
			{
				ui.movementTutorial.SetActive (true);
				ui.ShowHideTutorial (true, tutorials[index].description, true);
			}
		}
	}

	public void TutorialEnd (int index)
	{
		if (index == currentTutorialIndex)
		{
			ui.ShowHideTutorial (false, "", false);
			ui.movementTutorial.SetActive (false);
		}
	}
}