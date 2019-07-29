using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathFunctions
{
	public static float SmoothPingPong(float time, float maxOffset = 1, float speed = 1, float startOffset = 0, bool minIsStartNumber = false)
	{
		return minIsStartNumber ? maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset + maxOffset : maxOffset * Mathf.Sin(speed * Mathf.PI * time) + startOffset;
	}
}
