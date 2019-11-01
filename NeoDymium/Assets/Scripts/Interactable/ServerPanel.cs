//to be used in interact
using UnityEngine;
public class ServerPanel : IInteractable
{
	public ExitDoor linkedDoor;
	AudioSource audioSource;

	[Header("For Mat Change")]
	[SerializeField] Renderer[] screenRs;
	[SerializeField] Material[] screenMats;

	public override void Start()
	{
		base.Start();
		screenMats = MaterialUtils.GetMaterialsFromRenderers(screenRs);
		audioSource = GetComponent<AudioSource> ();
	}

	//If anything hackables try to interact, deny it. Only player can interact
	public override void TryInteract(ColorIdentifier userColor)
	{
		return; 
	}

	public override void Interact ()
	{
		audioSource.Play ();
		Disable();
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
	}

	public override string GetError(int key = 0)
	{
		if (player.inHackable) return "AI cannot interact with this Object";
		else if (!linkedDoor.locked) return "Server Panel has already been Disabled";
		else return string.Empty;
	}

	public void Disable()
	{
		linkedDoor.locked = false;
		MaterialUtils.ToggleMaterialsEmission(screenMats, false);
	}

	//If there is even a Restore for the Server Panel
	public void Restore()
	{
		linkedDoor.locked = true;
		MaterialUtils.ToggleMaterialsEmission(screenMats, true);
	}
}