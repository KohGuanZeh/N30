using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class AIDoor : MonoBehaviour
{
	public ColorIdentifier requiredColor;
	public bool nowForeverOpened;
	[SerializeField] Renderer[] emissiveRs;
	[SerializeField] Material[] emissiveMats;
	[SerializeField] Color lockedColor, unlockedColor; //For the Light Bars on Top
	[SerializeField] float lockedIntensity, unlockedIntensity;
	Animator animator;
	NavMeshObstacle obstacle;
	SoundManager soundManager;
	bool inRange = false;
	bool soundPlayed = false;
	bool otherClipPlayed = false;

	void Start ()
	{
		soundManager = SoundManager.inst;
		animator = GetComponent<Animator> ();
		obstacle = GetComponent<NavMeshObstacle> ();

		nowForeverOpened = requiredColor == ColorIdentifier.none? true : false;
		if (nowForeverOpened) 
		{
			SetDoorToUnlocked();
			animator.Play("Unlock", 1, 1.0f);
		}

		inRange = false;
		otherClipPlayed = false;
		soundPlayed = false;

		animator.SetFloat ("Speed", -1);

		//Get Materials to Change Emission
		emissiveMats = MaterialUtils.GetMaterialsFromRenderers(emissiveRs);
	}

	void Update ()
	{
		//For Testing Purposes
		//if (Input.GetKeyDown(KeyCode.Y)) animator.SetTrigger("Unlock");
		//if (Input.GetKeyDown(KeyCode.U)) MaterialUtils.ToggleUseEmissionHDRP(emissiveMats, true);

		if (animator.GetCurrentAnimatorStateInfo (0).normalizedTime <= 0 && !inRange)
			animator.SetFloat ("Speed", 0);
		if (animator.GetCurrentAnimatorStateInfo (0).normalizedTime >= 1 && inRange)
			animator.SetFloat ("Speed", 0);
	}

	void Open ()
	{
		inRange = true;
		soundPlayed = true;
		animator.SetFloat ("Speed", 1);
	
		if (!otherClipPlayed)
			soundManager.PlaySound (soundManager.slidingDoor);

		otherClipPlayed = true;
		RespectiveGoals goal = GetComponent<RespectiveGoals>();
		if (goal) goal.isCompleted = true;
		//float startTime = 0;
		animator.SetBool("Opened", true);
		/*if (animator.GetCurrentAnimatorStateInfo(0).IsName("Closed"))
		{
			startTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		animator.SetBool  ("Opened", true);
		animator.GetCurrentAnimatorStateInfo(0).normalizedTime;*/
	}

	void Close ()
	{
		inRange = false;
		soundPlayed = false;
		animator.SetFloat ("Speed", -1);
		if (otherClipPlayed)
			soundManager.PlaySound (soundManager.slidingDoor);

		otherClipPlayed = false;
		animator.SetBool  ("Opened", false);
	}

	void ChangeEmissionColor(bool unlocked = true)
	{
		Color emissiveColor = unlocked ? unlockedColor : lockedColor;
		float intensity = unlocked ? unlockedIntensity : lockedIntensity;
		MaterialUtils.ChangeMaterialsEmission(emissiveMats, emissiveColor, intensity, "_EmissiveColor");
		
		//HDR Does not Work
		/*MaterialUtils.ChangeMaterialsEmissionHDRP(emissiveMats, emissiveColor);
		MaterialUtils.ChangeMaterialsIntensityHDRP(emissiveMats, intensity);
		MaterialUtils.ToggleUseEmissionHDRP(emissiveMats, false);
		MaterialUtils.ToggleUseEmissionHDRP(emissiveMats, true);*/
	}

	public void SetDoorToUnlocked()
	{
		ChangeEmissionColor();
		obstacle.enabled = false;
		nowForeverOpened = true;
		animator.SetBool("Unlocked", true);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Hackable" && other.GetComponent<IHackable>().color == requiredColor && !nowForeverOpened)
		{
			if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Unlock"))
			{
				animator.SetTrigger("Unlock");
				soundManager.PlaySound (soundManager.doorUnlock);
				inRange = true;
			}
		}
	}

	void OnTriggerStay (Collider other)
	{
		if (nowForeverOpened && (other.tag == "Hackable" || other.tag == "Player") && !soundPlayed)
		{
			Open();
		}	
	}

	void OnTriggerExit (Collider other)
	{
		if (nowForeverOpened && (other.tag == "Hackable" || other.tag == "Player"))
		{
			Close();
		}
	}
}