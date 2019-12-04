using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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
	// 5: ambience/bgm

	PlayerController player;

	[Header ("Mixers")]
	public AudioMixer master;
	[Space (10)]
	public Slider masterSlider;
	public Slider bgmSlider;
	public Slider sfxSlider;

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
	public Audio doorUnlock;
	public Audio elevatorBell;
	public Audio elevatorTravel;

	[Header ("UI")]
	public Audio nextObjective;
	public Audio playerDetected;
	public Audio click;

	[Header ("BGM")]
	public Audio bgmSound;
	public Audio ambientNoise;

	[Header ("Interactable")]
	public Audio[] numpad;
	public Audio numpadSuccess;
	public Audio numpadFail;
	[Space (10)]
	public Audio vipCardPickUp;
	public Audio vipCardSuccess;
	public Audio vipCardFail;

	// Sounds that use their own audiosources:
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
		for (int i = 0; i < audioSources.Length - 1; i++)
		{	
			audioSources[i].loop = false;
			audioSources[i].playOnAwake = false;
		} 

		master.SetFloat ("masterVolume", PlayerPrefs.GetFloat ("Master", Mathf.Log (0.8f) * 20));
		master.SetFloat ("bgmVolume", PlayerPrefs.GetFloat ("BGM", Mathf.Log (0.8f) * 20));
		master.SetFloat ("sfxVolume", PlayerPrefs.GetFloat ("SFX", Mathf.Log (0.8f) * 20));

		if (PlayerPrefs.GetInt("Scene Index") <= 0) 
			player = null;
		else 
			player = PlayerController.inst;
	}

	void Update ()
	{
		if (player) 
			transform.position = player.CurrentViewingCamera.transform.position;

		master.SetFloat ("masterVolume", Mathf.Log (masterSlider.value) * 20);
		master.SetFloat ("bgmVolume", Mathf.Log (bgmSlider.value) * 20);
		master.SetFloat ("sfxVolume", Mathf.Log (sfxSlider.value) * 20);

		PlayerPrefs.SetFloat ("Master", Mathf.Log (masterSlider.value) * 20);
		PlayerPrefs.SetFloat ("BGM", Mathf.Log (bgmSlider.value) * 20);
		PlayerPrefs.SetFloat ("SFX", Mathf.Log (sfxSlider.value) * 20);
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