using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHackable : MonoBehaviour
{
	public new Camera camera;
	protected PlayerController player;
	[SerializeField] Renderer playerRenderer;
	public bool hacked = false;

	protected virtual void Start()
	{
		player = PlayerController.inst;
		playerRenderer = player.playerRenderer;
		camera = GetComponentInChildren<Camera>();
	}

	protected virtual void Update()
	{
		if (camera) CatchPlayer(); //If Hackable Object has a Camera, it should actively look out for Player
		if (hacked) ExecuteHackingFunctionaliy();
	}

	public virtual void CatchPlayer()
	{
		//May want a Threshold to activate this so this function does not keep calling
		//Scared that this(IsVisibleFrom()) will lag the game
		//Game Over for Stealth Gauge is implemented in Player Script
		if (playerRenderer.IsVisibleFrom(camera)) player.stealthGauge = Mathf.Min(player.stealthGauge + Time.deltaTime * player.increaseMultiplier, player.stealthThreshold);
	}

	public virtual void OnHack()
	{
		if (camera) player.ChangeViewCamera(camera);
		hacked = true;
	}

	public virtual void OnUnhack()
	{
		if (camera) player.ChangeViewCamera(player.GetPlayerCamera(), player.GetHeadRefTransform());
		hacked = false;
	}

	/// <summary>
	/// What to Execute when Player Hacks into the Object
	/// </summary>
	protected virtual void ExecuteHackingFunctionaliy()
	{

	}
}
