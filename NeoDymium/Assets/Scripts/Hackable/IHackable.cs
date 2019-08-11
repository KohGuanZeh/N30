using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorIdentifier { none, red, blue };

[System.Serializable]
public struct ShieldStruct
{
	public ColorIdentifier color;
	public bool isDisabled;
	public GameObject shieldObj;
}

public class IHackable : MonoBehaviour
{
	[Header ("General Hackable Properties")]
	protected PlayerController player;
	protected UIManager ui;

	[Header ("Hacking Related Variables")]
	public new Camera camera;
	public ColorIdentifier color;
	public bool hacked = false;
	public bool isDisabled = false;

	[Header("For Checking of Shields")]
	public bool hasNoShields;
	public List<Shield> enabledShields;
	public List<Shield> disabledShields;

	protected virtual void Start()
	{
		//General
		player = PlayerController.inst;
		ui = UIManager.inst;

		//Camera
		camera = GetComponentInChildren<Camera>();
		if (camera) camera.enabled = false; //Disable Camera Module at Start

		//Shields
		if (enabledShields.Count == 0 && disabledShields.Count == 0) hasNoShields = true;
		else hasNoShields = false;
	}

	protected virtual void Update()
	{
		if (ui.isPaused || ui.isGameOver) return;

		if (camera && !isDisabled) CatchPlayer(); //If Hackable Object has a Camera, and is not disabled, it should actively look out for Player

		if (hacked)
		{
			if (isDisabled || enabledShields.Count > 0) ForcedUnhack(); //Force Player to Unhack when 
			else ExecuteHackingFunctionaliy();
		}
	}

	/// <summary>
	/// What to Execute when Player Hacks into the Object
	/// </summary>
	protected virtual void ExecuteHackingFunctionaliy()
	{

	}

	public virtual void CatchPlayer()
	{
		//May want a Threshold to activate this so this function does not keep calling
		//Scared that this(IsVisibleFrom()) will lag the game
		//Game Over for Stealth Gauge is implemented in Player Script
		if (player.GetPlayerCollider().IsVisibleFrom(camera)) player.stealthGauge = Mathf.Min(player.stealthGauge + Time.deltaTime * player.increaseMultiplier, player.stealthThreshold);
	}

	public virtual Transform GetCameraRefPoint() //Meant for Head Bobbing
	{
		return null;
	}

	public virtual void ForcedUnhack()
	{
		//For Animations for Forced Unhack
		ui.ShowStaticScreen();
		player.Unhack();
	}

	public virtual void OnHack()
	{
		#region Using Old Hacking
		/*if (camera)
		{
			camera.enabled = true;
			player.ChangeViewCamera(camera);
		}*/
		#endregion

		hacked = true;
	}

	public virtual void OnUnhack()
	{
		#region Using Old Unhacking
		/*if (camera)
		{
			camera.enabled = false;
			player.ChangeViewCamera(player.GetPlayerCamera(), player.GetHeadRefTransform());
		}*/
		#endregion

		hacked = false;
	}

	//For Control Panel's Access
	public virtual void EnableDisable(bool isEnabler, ColorIdentifier controlPanelColor)
	{
		EnableDisableShield(isEnabler, controlPanelColor);
		EnableDisableHackable(isEnabler, controlPanelColor);
	}
	
	public virtual void EnableDisableHackable(bool isEnable, ColorIdentifier controlPanelColor)
	{
		if (color != controlPanelColor) return;
		isDisabled = !isEnable;
	}

	public virtual void EnableDisableShield(bool enable, ColorIdentifier controlPanelColor)
	{
		if (hasNoShields) return;
		
		List<int> indexesToRemove = new List<int>();

		if (enable)
		{
			for (int i = 0; i < disabledShields.Count; i++)
			{
				if (disabledShields[i].color == controlPanelColor) indexesToRemove.Add(i);
			}

			for (int i = 0; i < indexesToRemove.Count; i ++)
			{
				Shield shield = disabledShields[indexesToRemove[i]];
				shield.EnableDisableShield(true);
				disabledShields.Remove(shield);
				enabledShields.Add(shield);
			}
		}
		else
		{
			for (int i = 0; i < enabledShields.Count; i++)
			{
				if (enabledShields[i].color == controlPanelColor) indexesToRemove.Add(i);
			}

			for (int i = 0; i < indexesToRemove.Count; i ++)
			{
				Shield shield = enabledShields[indexesToRemove[i]];
				shield.EnableDisableShield(false);
				enabledShields.Remove(shield);
				disabledShields.Add(shield);
			}
		}
	}
}
