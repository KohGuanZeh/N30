using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class LightFlicker : MonoBehaviour
{
	[System.Serializable]
	public struct LightFlickerComp
	{
		public Color emissiveColor;
		public float emissiveIntensity;
		public float pointLightIntensity;
		public float spotLightIntensity;
	}

	[Header ("Light Components")]
	[SerializeField] Renderer lightR;
	[SerializeField] Material lightMat;
	[SerializeField] HDAdditionalLightData pointLight;
	[SerializeField] HDAdditionalLightData spotLight;

	[Header("Light Flicker Adjustments")]
	[SerializeField] bool includeDelayBeforeStart;
	[SerializeField] bool startAsLit = true;
	[SerializeField] LightFlickerComp litSettings, unlitSettings;

	[Header("Light Flicker Cycle Adjustments")]
	[SerializeField] float flickerSpeed;
	[SerializeField] int cyclesBeforeRest;
	[SerializeField] double restInterval;

	[Header("Lerp Time Etc")]
	[SerializeField] bool toLit;
	[SerializeField] bool atRest;
	[SerializeField] int halfCycles;
	[SerializeField] double lastTimeBeforeFlicker;
	[SerializeField] float flickerTime;
	Action action;

    void Start()
    {
		lightMat = lightR.material;

		if (startAsLit)
		{
			MaterialUtils.ChangeMaterialEmission(lightMat, litSettings.emissiveColor, litSettings.emissiveIntensity, "_EmissiveColor");
			pointLight.intensity = litSettings.pointLightIntensity;
			spotLight.intensity = litSettings.spotLightIntensity;
			toLit = false;
			flickerTime = 0;
		}
		else
		{
			MaterialUtils.ChangeMaterialEmission(lightMat, unlitSettings.emissiveColor, unlitSettings.emissiveIntensity, "_EmissiveColor");
			pointLight.intensity = unlitSettings.pointLightIntensity;
			spotLight.intensity = litSettings.spotLightIntensity;
			toLit = true;
			flickerTime = 1;
		}

		lastTimeBeforeFlicker = LoadingScreen.inst.GetTimeElapsed();
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
			float pIntensity = Mathf.Lerp(unlitSettings.pointLightIntensity, litSettings.pointLightIntensity, flickerTime);
			float sIntensity = Mathf.Lerp(unlitSettings.spotLightIntensity, litSettings.spotLightIntensity, flickerTime);

			MaterialUtils.ChangeMaterialEmission(lightMat, eColor, eIntensity, "_EmissiveColor");
			pointLight.intensity = pIntensity;
			spotLight.intensity = sIntensity;

			if (toLit && flickerTime == 1)
			{
				MaterialUtils.ChangeMaterialEmission(lightMat, litSettings.emissiveColor, litSettings.emissiveIntensity, "_EmissiveColor");
				pointLight.intensity = litSettings.pointLightIntensity;
				spotLight.intensity = litSettings.spotLightIntensity;
				halfCycles++;
				toLit = false;

				if (cyclesBeforeRest * 2 <= halfCycles) SetToRest();
			}
			else if (!toLit && flickerTime == 0)
			{
				MaterialUtils.ChangeMaterialEmission(lightMat, unlitSettings.emissiveColor, unlitSettings.emissiveIntensity, "_EmissiveColor");
				pointLight.intensity = unlitSettings.pointLightIntensity;
				spotLight.intensity = unlitSettings.spotLightIntensity;
				halfCycles++;
				toLit = true;

				if (cyclesBeforeRest * 2 <= halfCycles) SetToRest();
			}
		}
		else
		{
			if (LoadingScreen.inst.GetTimeElapsed() - restInterval > lastTimeBeforeFlicker) ContinueCycle();
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
