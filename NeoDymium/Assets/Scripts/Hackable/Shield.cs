using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
	[Header ("Shield Properties")]
	public ColorIdentifier color;
	public bool isDisabled;
	Renderer rend; 

	[Header ("Others")]
	public Color playerColor = Color.white;
	public Color redColor = Color.red;
	public Color blueColor = Color.blue;
	public Color yellowColor = Color.yellow;
	public Color greenColor = Color.green;
	//[SerializeField] Collider collider; //If the Shield has a Collider

	//If Shield is to have a Collider, need to make it a Different Layer

	void Start ()
	{
		rend = GetComponentInChildren<Renderer> ();
		MaterialUtils.ChangeMaterialEmission(rend.material, ChangeShieldColor(), 1.0f, "_Emission");
	}

	public void EnableDisableShield(bool enable)
	{
		isDisabled = !enable;

		if (isDisabled) OnShieldDisable();
		else OnShieldEnable();
	}

	void OnShieldEnable()
	{
		//For Any Lerp Functions
		rend.enabled = true;
	}

	void OnShieldDisable()
	{
		rend.enabled = false;
	}

	Color ChangeShieldColor ()
	{
		switch (color)
		{
			case ColorIdentifier.red:
				return redColor;
			case ColorIdentifier.blue:
				return blueColor;
			case ColorIdentifier.yellow:
				return yellowColor;
			case ColorIdentifier.green:
				return greenColor;
			default:
				return playerColor;
		}
	}
}
