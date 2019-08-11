﻿using System.Collections.Generic;

//to be called using tryinteract
public class ControlPanel : IInteractable
{
	public List<IHackable> affectedItems;

	public bool activated = false;

	void Start ()
	{
		activated = false;
	}

	public override void Interact ()
	{
		if (!activated)
		{
			activated = true;
			foreach (IHackable item in affectedItems)
				item.EnableDisable (false, color);
		}
	}
}