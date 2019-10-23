using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreaNamesManager : MonoBehaviour
{
    public TextMeshProUGUI areaNameText;
    public static AreaNamesManager inst;

    private void Awake ()
    {
        inst = this;
    }

	private void Start()
	{
		if (!areaNameText) areaNameText = UIManager.inst.GetRoomAndHackableName();
	}

	//Anim for Text Fading
	IEnumerator FadeInFadeOut ()
    {  
        areaNameText.canvasRenderer.SetAlpha(0.0f);

        areaNameText.CrossFadeAlpha(1.0f, 0.5f, false);

        yield return new WaitForSeconds (1.0f);
    }
}
