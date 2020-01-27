using UnityEngine;
using UnityEngine.UI;

public class DiamondAnimate : MonoBehaviour
{
	Image[] fadeObjs;

	float maxOpacity;
	bool fadingIn;

	float startOpacity;
	float changeSpeed;

	void Start ()
	{
		fadeObjs = GetComponentsInChildren<Image> ();
		maxOpacity = fadeObjs[0].color.a;
		float t = Random.Range (1.50f, 3.00f);
		InvokeRepeating ("ChangeOpacity", t, t);
	}

	void Update ()
	{
		UpdateOpacity ();
	}

	void UpdateOpacity ()
	{
		for (int i = 0; i < fadeObjs.Length; i++)
		{
			float opacity = Mathf.SmoothDamp (fadeObjs[i].color.a, fadingIn ? maxOpacity : 0, ref changeSpeed, 0.5f * Time.deltaTime, 0.5f);
			fadeObjs[i].color = new Color (fadeObjs[i].color.r, fadeObjs[i].color.b, fadeObjs[i].color.g, opacity);
		}
	}

	void ChangeOpacity ()
	{
		bool change = RandomBool ();
		if (!change)
			return;
		fadingIn = !fadingIn;

		startOpacity = fadeObjs[0].color.a;
		changeSpeed = 0;
	}

	bool RandomBool ()
	{
		return Random.Range (0, 2) == 0 ? true : false;
	}
}