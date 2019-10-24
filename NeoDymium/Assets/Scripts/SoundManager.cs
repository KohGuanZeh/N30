using UnityEngine;

[System.Serializable]
public struct Audio
{
	public AudioClip clip;
	public float volume; //0.00 - 1.00
	public float pitch; //-3.00 - 3.00
	public int sourceIndex;
}

[RequireComponent (typeof (AudioListener))]
public class SoundManager : MonoBehaviour
{
	public static SoundManager inst;

	AudioSource[] audioSources;
	// 0: general
	// 1: movement
	// 2: interactables
	// 3: environment
	// 4: ui

	PlayerController player;	

	[Header ("General")]
	public Audio hack;
	public Audio unhack;

	[Header ("Player")]
	public Audio playerWalk;
	public Audio playerCrouch; //sourceindex 0
	public Audio playerCrouchWalk;

	[Header ("AI")]
	public Audio aiWalk;
	
	[Header ("CCTV")]
	public Audio cctvRotate; //sourceindex 1

	[Header ("Environment")]
	public Audio slidingDoor;

	[Header ("UI")]
	public Audio nextObjective;
	public Audio playerDetected;

	[Header ("BGM")]
	public Audio bgm;
	public Audio ambientNoise;

	[Header ("Interactable")]
	public Audio[] numpad;
	public Audio numpadSuccess;
	public Audio numpadFail;
	[Space (10)]
	public Audio vipCardPickUp;
	public Audio vipCardSuccess;
	public Audio vipCardFail;

	//Sounds that use their own audiosources:
	// 1. control panel
	// 2. server panel
	// 3. emergency alarm

	void Awake ()
	{
		inst = this;
	}

	void Start ()
	{
		audioSources = GetComponents<AudioSource> ();
		foreach (AudioSource source in audioSources)
		{
			source.loop = false;
			source.playOnAwake = false;
		}
			
		player = PlayerController.inst;
	}

	void Update ()
	{
		transform.position = player.CurrentViewingCamera.transform.position;
	}

	public bool IsSourcePlaying (int sourceIndex)
	{
		return audioSources[sourceIndex].isPlaying;
	}

	public void PlaySound (Audio audio)
	{
		AudioSource source = audioSources[audio.sourceIndex];
		source.clip = audio.clip;
		source.pitch = audio.pitch;
		source.volume = audio.volume;
		source.Play ();
	}

	public void StopSound (int sourceIndex)
	{
		AudioSource source = audioSources[sourceIndex];
		source.Stop ();
	}
}