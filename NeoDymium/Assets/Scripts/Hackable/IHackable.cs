using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

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
	public Collider col;
	
	[Header ("Hacking Related Variables")]
	public new Camera camera;
	public PostProcessVolume postProcessVolume;
	public PostProcessProfile ppp;
	public ColorIdentifier color;
	public bool hacked = false;
	public bool hackable = true;
	public bool isDisabled = false;
	public Material disabledMaterial;
	public Renderer[] renderersToChangeMaterial;

	[Header("For Checking of Shields")]
	public bool hasNoShields;
	public List<Shield> enabledShields;
	public List<Shield> disabledShields;

	[Header("Player Detection")]
	public bool hasPlayerMemory = false;
	public bool canWipeMemory = true;
	public RectTransform pointer; //Stores the Pointer of the UI so that to specify which Pointer belongs to which AI.
	
	[Header ("UI")]
	public GameObject exclamationMark;
	public GameObject questionMark;
	GameObject whiteDot;

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

		whiteDot = Instantiate (UIManager.inst.whiteDot, Vector3.zero, Quaternion.identity, UIManager.inst.whiteDotHolder);
		col = GetComponent<Collider> ();
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

	void FixedUpdate ()
	{
		if (!isDisabled) WhiteDot ();
	}

	void WhiteDot ()
	{
		/*
		if (col.IsVisibleFrom (player.CurrentViewingCamera))
		{
			whiteDot.SetActive (true);
			whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
		}			
		else if (hacked)
			whiteDot.SetActive (false);
		else
			whiteDot.SetActive (false);
		*/

		whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
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
		if (player.GetPlayerCollider().IsVisibleFrom(camera))
		{
			if (!pointer)
			{
				for (int i = 0; i < ui.detectedPointers.Count; i++)
				{
					if (ui.detectedPointers[i].gameObject.activeSelf) continue;
					else
					{
						pointer = ui.detectedPointers[i];
						pointer.gameObject.SetActive(true);
						break;
					}
				}
			}
			else ui.LocateHackable(this, pointer);

			player.IncreaseStealthGauge();
			//print("Seen by " + gameObject.name);
			
			hasPlayerMemory = true;
			exclamationMark.SetActive(true);
			questionMark.SetActive (false);
			exclamationMark.transform.LookAt(player.CurrentViewingCamera.transform);
		}
		else
		{
			questionMark.SetActive (hasPlayerMemory);
			questionMark.transform.LookAt (player.CurrentViewingCamera.transform);
			exclamationMark.SetActive(false);
			if (pointer)
			{
				pointer.gameObject.SetActive(false);
				pointer = null;
			}
		}
	}

	public virtual Transform GetCameraRefPoint() //Meant for Head Bobbing
	{
		return null;
	}

	public virtual void ForcedUnhack()
	{
		//For Animations for Forced Unhack
		player.Unhack(true);
		ui.ShowStaticScreen();
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
		postProcessVolume.profile = ppp;
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

		postProcessVolume.profile = player.ppp;
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
		exclamationMark.SetActive (false);
		questionMark.SetActive (false);
		whiteDot.SetActive (false);

		for (int i = 0; i < renderersToChangeMaterial.Length; i++)
			renderersToChangeMaterial[i].material = disabledMaterial;
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
