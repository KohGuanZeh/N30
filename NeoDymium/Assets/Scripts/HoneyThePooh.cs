using UnityEngine;
using UnityEngine.UI;

public class HoneyThePooh : MonoBehaviour
{
	public GameObject circleParent;
	public float speed1;
	public float speed2;
	public float speed3;
	[Space (10)]
	public GameObject bigDiamondParent;
	public byte bigMaxOpacity;
	[Space (10)]
	public GameObject smallDiamondParent;
	public byte smallMaxOpacity;
	[Space (10)]
	public float frequency;
	public float fadeSpeed;

	Transform[] circles;

	Image[] bigDiamonds;
	bool[] bigDiamondActive;

	Image[] bigDiamondLines;
	bool[] bigDiamondLinesActive;

	Image[] smallDiamonds;
	bool[] smallDiamondActive;

	Image[] smallDiamondLines;
	bool[] smallDiamondLinesActive;

    void Start ()
	{
		circles = new Transform[3];
		for (int i = 0; i < 3; i++)
		{
			circles[i] = circleParent.transform.GetChild (i);
		}

		bigDiamonds = new Image[bigDiamondParent.transform.childCount];
		bigDiamondActive = new bool [bigDiamonds.Length];
		bigDiamondLines = new Image [bigDiamonds.Length];
		bigDiamondLinesActive = new bool [bigDiamonds.Length];
		for (int i = 0; i < bigDiamonds.Length; i++)
		{
			bigDiamonds[i] = bigDiamondParent.transform.GetChild (i).GetChild (0).GetComponent<Image> ();
			bigDiamondLines[i] = bigDiamondParent.transform.GetChild (i).GetChild (1).GetComponent<Image> ();
			bigDiamonds[i].color = new Color32 (255, 255, 255, (byte) Random.Range (0, bigMaxOpacity));
			bigDiamondLines[i].color = new Color32 (255, 255, 255, (byte) Random.Range (0, bigMaxOpacity));

			bool active = RandomBool ();
			bigDiamondActive[i] = active;
			bigDiamondLinesActive[i] = active;
		}

		smallDiamonds = new Image[smallDiamondParent.transform.childCount];
		smallDiamondActive = new bool [smallDiamonds.Length];
		smallDiamondLines = new Image [smallDiamonds.Length];
		smallDiamondLinesActive = new bool [smallDiamonds.Length];
		for (int i = 0; i < smallDiamonds.Length; i++)
		{
			smallDiamonds[i] = smallDiamondParent.transform.GetChild (i).GetChild (0).GetComponent<Image> ();
			smallDiamondLines[i] = smallDiamondParent.transform.GetChild (i).GetChild (1).GetComponent<Image> ();
			smallDiamonds[i].color = new Color32 (255, 255, 255, (byte) Random.Range (0, smallMaxOpacity));
			smallDiamondLines[i].color = new Color32 (255, 255, 255, (byte) Random.Range (0, smallMaxOpacity));

			bool active = RandomBool ();
			smallDiamondActive[i] = active;
			smallDiamondLinesActive[i] = active;
		}

		InvokeRepeating ("ChangeOpacity", frequency, frequency);
	}

	void Update ()
	{
		RotateCircle ();
		UpdateOpacity ();
	}

	void RotateCircle ()
	{
		circles[0].eulerAngles += Vector3.forward * speed1 * Time.deltaTime;
		circles[1].eulerAngles -= Vector3.forward * speed2 * Time.deltaTime;
		circles[2].eulerAngles += Vector3.forward * speed3 * Time.deltaTime;
	}

	void UpdateOpacity ()
	{
		for (int i = 0; i < bigDiamonds.Length; i++)
		{
			if (bigDiamondActive[i])
			{
				bigDiamonds[i].color = Color.Lerp (bigDiamonds[i].color, new Color32 (255, 255, 255, bigMaxOpacity), fadeSpeed * Time.deltaTime);
				bigDiamondLines[i].color = Color.Lerp (bigDiamondLines[i].color, new Color32 (255, 255, 255, bigMaxOpacity), fadeSpeed * Time.deltaTime);
			}
			else
			{
				bigDiamonds[i].color = Color.Lerp (bigDiamonds[i].color, new Color32 (255, 255, 255, 0), fadeSpeed * Time.deltaTime);
				bigDiamondLines[i].color = Color.Lerp (bigDiamondLines[i].color, new Color32 (255, 255, 255, 0), fadeSpeed * Time.deltaTime);
			}
		}

		for (int i = 0; i < smallDiamonds.Length; i++)
		{
			if (smallDiamondActive[i])
			{
				smallDiamonds[i].color = Color.Lerp (smallDiamonds[i].color, new Color32 (255, 255, 255, smallMaxOpacity), fadeSpeed * Time.deltaTime);
				smallDiamondLines[i].color = Color.Lerp (smallDiamondLines[i].color, new Color32 (255, 255, 255, smallMaxOpacity), fadeSpeed * Time.deltaTime);
			}
			else
			{
				smallDiamonds[i].color = Color.Lerp (smallDiamonds[i].color, new Color32 (255, 255, 255, 0), fadeSpeed * Time.deltaTime);
				smallDiamondLines[i].color = Color.Lerp (smallDiamondLines[i].color, new Color32 (255, 255, 255, 0), fadeSpeed * Time.deltaTime);
			}
		}
	}

	void ChangeOpacity ()
	{
		for (int i = 0; i < bigDiamondActive.Length; i++)
		{
			bool active = RandomBool ();
			bigDiamondActive[i] = active;
			bigDiamondLinesActive[i] = active;
		}

		for (int i = 0; i < smallDiamondActive.Length; i++)
		{
			bool active = RandomBool ();
			smallDiamondActive[i] = active;
			smallDiamondLinesActive[i] = active;
		}
	}

	bool RandomBool ()
	{
		return Random.Range (0, 2) == 0 ? true : false;
	}
}