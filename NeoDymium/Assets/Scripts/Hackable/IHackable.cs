using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHackable : MonoBehaviour
{
	[Header ("General Hackable Properties")]
	public new Camera camera;
	protected PlayerController player;
	protected UIManager ui;
	[SerializeField] Renderer playerRenderer;
	public bool hacked = false;

	protected virtual void Start()
	{
		player = PlayerController.inst;
		ui = UIManager.inst;
		playerRenderer = player.playerRenderer;
		camera = GetComponentInChildren<Camera>();
		if (camera) camera.enabled = false; //Disable Camera Module at Start
	}

	protected virtual void Update()
	{
		if (ui.isPaused || ui.isGameOver) return;
		if (camera) CatchPlayer(); //If Hackable Object has a Camera, it should actively look out for Player
		if (hacked) ExecuteHackingFunctionaliy();
	}

	public virtual void CatchPlayer()
	{
		//May want a Threshold to activate this so this function does not keep calling
		//Scared that this(IsVisibleFrom()) will lag the game
		//Game Over for Stealth Gauge is implemented in Player Script
		if (player.GetPlayerCollider().IsVisibleFrom(camera)) player.stealthGauge = Mathf.Min(player.stealthGauge + Time.deltaTime * player.increaseMultiplier, player.stealthThreshold);
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

	public virtual void Disable ()
	{
		gameObject.layer = 0;
		camera.enabled = false;
		this.enabled = false;
	}

	/// <summary>
	/// What to Execute when Player Hacks into the Object
	/// </summary>
	protected virtual void ExecuteHackingFunctionaliy()
	{

	}
}
