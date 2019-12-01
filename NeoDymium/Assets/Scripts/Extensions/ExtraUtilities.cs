using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script is Developed by Koh Guan Zeh
//Last Updated: 2 Dec 2019
public static class MathFunctions
{
	public static float SmoothPingPong(float time, float maxOffset = 1, float speed = 1, float startOffset = 0, bool minIsStartNumber = false)
	{
		return minIsStartNumber ? maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset + maxOffset : maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset;
	}
}

public static class MaterialUtils
{
	public static Material[] GetMaterialsFromRenderers(Renderer[] rs)
	{
		Material[] mats = new Material[rs.Length];
		for (int i = 0; i < mats.Length; i++) mats[i] = rs[i].material;

		return mats;
	}

	//Not Tested
	public static Material[] GetMaterialsFromRenderers(Renderer[] rs, int matIndex)
	{
		Material[] mats = new Material[rs.Length];
		for (int i = 0; i < mats.Length; i++) mats[i] = rs[i].materials[matIndex];

		return mats;
	}

	/// <summary>
	/// Toggles the Emission Property for a Selected Material.
	/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
	/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
	/// </summary>
	/// <param name="mat">Material To Toggle Emission</param>
	/// <param name="emissionOn">Should Emission be On?</param>
	/// <param name="emissionProperty">Shader Keyword for Emission</param>
	public static void ToggleMaterialEmission(Material mat, bool emissionOn, string emissionProperty = "_EMISSION") //Emission must always be turned on first for Enable Keyword to work
	{
		if (emissionOn) mat.EnableKeyword(emissionProperty);
		else mat.DisableKeyword(emissionProperty);
	}

	/// <summary>
	/// Toggles the Emission Property for the Selected Materials.
	/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
	/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
	/// </summary>
	/// <param name="mats">Materials To Toggle Emission</param>
	/// <param name="emissionOn">Should Emission be On?</param>
	/// <param name="emissionProperty">Shader Keyword for Emission</param>
	public static void ToggleMaterialsEmission(Material[] mats, bool emissionOn, string emissionProperty = "_EMISSION") //Emission must always be turned on first for Enable Keyword to work
	{
		foreach (Material mat in mats)
		{
			if (emissionOn) mat.EnableKeyword(emissionProperty);
			else mat.DisableKeyword(emissionProperty);
		}
	}

	/// <summary>
	/// Changes the Selected Material's Emission Color
	/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
	/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
	/// </summary>
	/// <param name="mat">Material requiring Emissive Color Change</param>
	/// <param name="color">Color of the Emission</param>
	/// <param name="intensity">Intensity of the Emission</param>
	/// <param name="emissionProperty">Shader Keyword for Emission</param>
	public static void ChangeMaterialEmission(Material mat, Color color, float intensity, string emissionProperty = "_EmissionColor")
	{
		mat.SetColor(emissionProperty, color * intensity);
	}

	/// <summary>
	/// Changes the Selected Material's Emission Color
	/// Emission must always be turned on at the Start of the Game for the Enable Keyword to work.
	/// This is because Unity will leave out the Emission Property when Exporting the Material when it is not registered as being used.
	/// </summary>
	/// <param name="mats">Materials that requires Emissive Color Change</param>
	/// <param name="color">Color of the Emission</param>
	/// <param name="intensity">Intensity of the Emission</param>
	/// <param name="emissionProperty">Shader Keyword for Emission</param>
	public static void ChangeMaterialsEmission(Material[] mats, Color color, float intensity, string emissionProperty = "_EmissionColor")
	{
		foreach (Material mat in mats) mat.SetColor(emissionProperty, color * intensity);
	}
}

public static class ColorUtils
{
	//Mainly used for Image.Color
	public static Color ChangeAlpha(this Color color, float alpha = 1)
	{
		color.a = alpha;
		return color;
	}
}
