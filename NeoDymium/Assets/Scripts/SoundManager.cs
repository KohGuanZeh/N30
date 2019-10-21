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

	PlayerController player;	

	[Header ("General")]
	public Audio hack;
	public Audio unhack;
	public Audio forcedUnhack;

	[Header ("Player")]
	public Audio playerWalk;
	public Audio playerCrouch;
	public Audio playerCrouchWalk;

	[Header ("AI")]
	public Audio aiWalk;
	
	[Header ("CCTV")]
	public Audio cctvRotate;

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

	public bool IsSourcePlaying (Audio audio)
	{
		return audioSources[audio.sourceIndex].isPlaying;
	}

	public void PlaySound (Audio audio)
	{
		AudioSource source = audioSources[audio.sourceIndex];
		source.clip = audio.clip;
		source.pitch = audio.pitch;
		source.volume = audio.volume;
		source.Play ();
	}

	public void StopSound (Audio audio)
	{
		AudioSource source = audioSources[audio.sourceIndex];
		source.Stop ();
	}
}