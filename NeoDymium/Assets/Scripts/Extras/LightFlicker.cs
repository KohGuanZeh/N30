using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	[System.Serializable]
	public struct LightFlickerComp
	{
		public Color emissiveColor;
		public float emissiveIntensity;
		public float lightIntensity;
	}

	[Header ("Light Components")]
	[SerializeField] Renderer lightR;
	[SerializeField] Material lightMat;
	[SerializeField] Light pointLight;
	[SerializeField] Light spotLight;

	[Header("Light Flicker Adjustments")]
	[SerializeField] bool includeDelayBeforeStart;
	[SerializeField] bool startAsLit = true;
	[SerializeField] LightFlickerComp litSettings, unlitSettings;

	[Header("Light Flicker Cycle Adjustments")]
	[SerializeField] float flickerSpeed;
	[SerializeField] int cyclesBeforeRest;
	[SerializeField] float cycleInterval;
	[SerializeField] float restInterval;

	[Header("Lerp Time Etc")]
	[SerializeField] bool toLit;
	[SerializeField] bool atRest;
	[SerializeField] int halfCycles;
	[SerializeField] float lastTimeBeforeFlicker;
	[SerializeField] float flickerTime;
	Action action;

    void Start()
    {
		lightMat = lightR.material;

		if (startAsLit)
		{
			MaterialUtils.ChangeMaterialEmission(lightMat, litSettings.emissiveColor, litSettings.emissiveIntensity, "_EmissiveColor");
			pointLight.intensity = litSettings.lightIntensity;
			spotLight.intensity = litSettings.lightIntensity;
			toLit = false;
			flickerTime = 0;
		}
		else
		{
			MaterialUtils.ChangeMaterialEmission(lightMat, unlitSettings.emissiveColor, unlitSettings.emissiveIntensity, "_EmissiveColor");
			pointLight.intensity = unlitSettings.lightIntensity;
			spotLight.intensity = litSettings.lightIntensity;
			toLit = true;
			flickerTime = 1;
		}

		lastTimeBeforeFlicker = Time.time;
		if (includeDelayBeforeStart) atRest = true;
		action += RunFlickerCycle;
    }

    // Update is called once per frame
    void Update()
    {
		if (action != null) action();
    }

	void RunFlickerCycle()
	{
		if (!atRest)
		{
			flickerTime = toLit ? Mathf.Min(flickerTime + Time.deltaTime * flickerSpeed, 1) : Mathf.Max(flickerTime - Time.deltaTime * flickerSpeed, 0);

			//Lerping Components
			Color eColor = Color.Lerp(unlitSettings.emissiveColor, litSettings.emissiveColor, flickerTime);
			float eIntensity = Mathf.Lerp(unlitSettings.emissiveIntensity, litSettings.emissiveIntensity, flickerTime);
			float intensity = Mathf.Lerp(unlitSettings.lightIntensity, litSettings.lightIntensity, flickerTime);

			MaterialUtils.ChangeMaterialEmission(lightMat, eColor, eIntensity, "_EmissiveColor");
			pointLight.intensity = intensity;
			spotLight.intensity = intensity;

			if (toLit && flickerTime == 1)
			{
				MaterialUtils.ChangeMaterialEmission(lightMat, litSettings.emissiveColor, litSettings.emissiveIntensity, "_EmissiveColor");
				pointLight.intensity = litSettings.lightIntensity;
				spotLight.intensity = litSettings.lightIntensity;
				halfCycles++;
				toLit = false;

				if (cyclesBeforeRest * 2 <= halfCycles) SetToRest();
			}
			else if (!toLit && flickerTime == 0)
			{
				MaterialUtils.ChangeMaterialEmission(lightMat, unlitSettings.emissiveColor, unlitSettings.emissiveIntensity, "_EmissiveColor");
				pointLight.intensity = unlitSettings.lightIntensity;
				spotLight.intensity = unlitSettings.lightIntensity;
				halfCycles++;
				toLit = true;

				if (cyclesBeforeRest * 2 <= halfCycles) SetToRest();
			}
		}
		else
		{
			if (Time.time - restInterval > lastTimeBeforeFlicker) ContinueCycle();
		}
	}

	void SetToRest()
	{
		halfCycles = 0;
		lastTimeBeforeFlicker = Time.time;
		atRest = true;
	}

	void ContinueCycle()
	{
		atRest = false;
	}
}
