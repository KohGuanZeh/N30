﻿using System.Collections;
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

	public virtual void CatchPlayer()
	{
		//May want a Threshold to activate this so this function does not keep calling
		//Scared that this(IsVisibleFrom()) will lag the game
		//Game Over for Stealth Gauge is implemented in Player Script
		if (player.GetPlayerCollider().IsVisibleFrom(camera)) player.stealthGauge = Mathf.Min(player.stealthGauge + Time.deltaTime * player.increaseMultiplier, player.stealthThreshold);
	}

	public virtual void ForcedUnhack()
	{
		//For Animations for Forced Unhack
		player.Unhack();
	}

	public virtual void OnHack()
	{
		if (camera)
		{
			camera.enabled = true;
			player.ChangeViewCamera(camera);
		}
		hacked = true;
	}

	public virtual void OnUnhack()
	{
		if (camera)
		{
			camera.enabled = false;
			player.ChangeViewCamera(player.GetPlayerCamera(), player.GetHeadRefTransform());
		}
		hacked = false;
	}

	//For Control Panel's Access
	public virtual void EnableDisable(bool isEnabler, ColorIdentifier controlPanelColor)
	{
		EnableDisableShield(isEnabler, controlPanelColor);
		EnableDisableHackable(isEnabler, controlPanelColor);
	}
	
	public virtual void EnableDisableHackable(bool enable, ColorIdentifier controlPanelColor)
	{
		if (color != controlPanelColor) return;
		isDisabled = !enable;
	}

	public virtual void EnableDisableShield(bool enable, ColorIdentifier controlPanelColor)
	{
		if (hasNoShields) return;

		if (enable)
		{
			foreach (Shield shield in disabledShields)
			{
				if (shield.color == controlPanelColor)
				{
					shield.EnableDisableShield(true);
					disabledShields.Remove(shield);
					enabledShields.Add(shield);
				}
			}
		}
		else
		{
			foreach (Shield shield in enabledShields)
			{
				if (shield.color == controlPanelColor)
				{
					shield.EnableDisableShield(false);
					enabledShields.Remove(shield);
					disabledShields.Add(shield);
				}
			}
		}
	}

	/// <summary>
	/// What to Execute when Player Hacks into the Object
	/// </summary>
	protected virtual void ExecuteHackingFunctionaliy()
	{

	}
}