using System.Collections.Generic;
using UnityEngine;

//to be called using tryinteract
public class ControlPanel : IInteractable
{
	public List<IHackable> affectedItems;
	[SerializeField] Material controlPanelMat;
	[SerializeField] Color defaultColor;

	public bool activated = false;

	void Start ()
	{
		activated = false;
		controlPanelMat = transform.GetChild(0).GetComponent<Renderer>().material; //To Access the Material and Deactivate the Colors
		defaultColor = controlPanelMat.color;
	}

	public override void Interact ()
	{
		if (!activated)
		{
			activated = true;
			controlPanelMat.DisableKeyword("_EMISSION");
			controlPanelMat.color = Color.grey;
			foreach (IHackable item in affectedItems)
				item.EnableDisable (false, color);
		}
	}

	public void OnRestore() //If we adding Circuit Enablers
	{
		controlPanelMat.EnableKeyword("_EMISSION");
		controlPanelMat.color = defaultColor;
	}
}