using UnityEngine;

public class ControlPanel : MonoBehaviour
{
	public bool activated = false;

	void Start ()
	{
		activated = false;
		
	}

	public void Activate ()
	{
		if (!activated)
		{
			activated = true;
			IHackable[] items = FindObjectsOfType<IHackable> ();
			/*foreach (IHackable item in items)
				item.Disable ();*/
		}
	}
}