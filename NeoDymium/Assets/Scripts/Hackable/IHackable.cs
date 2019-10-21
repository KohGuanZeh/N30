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

	[Header("UI")]
	public string roomName;
	public string hackableName;
	public GameObject exclamationMark;
	public GameObject questionMark;
	public float whiteDotRaycastHeightOffset = 0.5f;
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

		whiteDot = Instantiate (ui.whiteDot, Vector3.zero, Quaternion.identity, ui.whiteDotHolder);
		col = GetComponent<CapsuleCollider>();
	}

	protected virtual void Update()
	{
		if (!isDisabled) NewWhiteDot();

		if (ui.isPaused || ui.isGameOver) return;

		if (camera && !isDisabled) CatchPlayer(); //If Hackable Object has a Camera, and is not disabled, it should actively look out for Player
		if (hacked)
		{
			if (isDisabled || enabledShields.Count > 0) ForcedUnhack(); //Force Player to Unhack when 
			else ExecuteHackingFunctionaliy();
		}
	}

	/*void FixedUpdate ()
	{
		if (!isDisabled) NewWhiteDot ();
	}*/

	void NewWhiteDot()
	{
		Ray ray = new Ray(player.CurrentViewingCamera.transform.position, (transform.position + Vector3.up * whiteDotRaycastHeightOffset - player.CurrentViewingCamera.transform.position).normalized);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, player.aimingRaycastLayers))
		{
			//Debug.DrawLine(ray.origin, hit.point, Color.red);

			if (col == hit.collider)
			{
				//print(name + "is Hit");
				whiteDot.gameObject.SetActive(true);
				whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint(transform.position);
			}
			else whiteDot.gameObject.SetActive(false);
		}
		else whiteDot.gameObject.SetActive(false);
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

		//whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
	
		/* 
		RaycastHit hit;
		Vector3 currentPos = transform.position + Vector3.up * whiteDotRaycastHeightOffset;
		Physics.Raycast (currentPos, (player.CurrentViewingCamera.transform.position - currentPos).normalized, out hit, Mathf.Infinity, player.aimingRaycastLayers);
		
		if (hit.collider != null)
		{
			if (hit.collider.tag == player.CurrentViewingCamera.transform.tag && hit.collider.name != gameObject.name) 
			{
				whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
				whiteDot.SetActive(true);
			}
			else whiteDot.SetActive(false);
		}
		else whiteDot.SetActive(false);
		*/

        Ray r = new Ray (transform.position, (player.CurrentViewingCamera.transform.position - transform.position).normalized);
		RaycastHit[] hits = Physics.RaycastAll (r, (player.CurrentViewingCamera.transform.position - transform.position).magnitude, player.aimingRaycastLayers);

		bool passed = true;
		foreach (RaycastHit hit in hits)
		{
			if (hit.collider.gameObject != player.CurrentViewingCamera.gameObject)
			{
				passed = false;
				whiteDot.gameObject.SetActive (false);
			}
		}

		if (passed)
		{
			whiteDot.gameObject.SetActive (true);
        	whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
		}
		/* 
        if (Physics.RaycastAll (r, out hit, (player.CurrentViewingCamera.transform.position - transform.position).magnitude, player.aimingRaycastLayers)) 
		{
            if (hit.collider.gameObject == player.CurrentViewingCamera.gameObject) 
			{
                whiteDot.gameObject.SetActive (true);
                whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
            } 
			else 
			{
                whiteDot.gameObject.SetActive (false);
            }
        }
		*/

		/*if (hit.collider == null)
		{
			whiteDot.SetActive (false);
			return;
		}

		if (!(hit.collider.tag == "Hackable" || 
			hit.collider.tag == "Interactable" ||
			hit.collider.tag == "Player") && 
			hit.collider.name != gameObject.name)
		{
			whiteDot.SetActive (false);
		}
		else
		{
			whiteDot.SetActive (true);
			whiteDot.transform.position = player.CurrentViewingCamera.WorldToScreenPoint (transform.position);
		}*/
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
		//postProcessVolume.profile = ppp;
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

		//postProcessVolume.profile = player.ppp;
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

	/// <summary>
	/// Get the Error Message Corresponding to the Action
	/// </summary>
	/// <param name="key"> 0 is Hacking, 1 is Wipe Memory</param>
	/// <returns></returns>
	public virtual string GetError(int key = 0)
	{
		if (isDisabled) return "Error. System is Disabled";
		else if (enabledShields.Count > 0) return "Error.System Protection Level Too High";
		else if (!hackable) return "Error. Entity is preventing further Action";
		else if (key > 0)
		{
			//Only thing I did not Check is the Distance
			if (player.inHackable) return "Error. Can only Wipe in Player Body";
			else if (!canWipeMemory) return "Error. Entity is preventing further Action";
			else return string.Empty;
		}
		else return string.Empty;
	}

	public void GetSetPlayerMemory(int cpIndex, int index, bool get = true) //If Get is false, It is Set
	{
		if (get) hasPlayerMemory = PlayerPrefs.GetInt(string.Format("Checkpoint {0} Hackable {1}", cpIndex, index)) == 1 ? true : false;
		else PlayerPrefs.SetInt(string.Format("Checkpoint {0} Hackable {1}", cpIndex, index), hasPlayerMemory ? 1 : 0);
		//print(string.Format("Checkpoint {0} Hackable {1} Has Memory: {2}", cpIndex, index, hasPlayerMemory));
	}
}
