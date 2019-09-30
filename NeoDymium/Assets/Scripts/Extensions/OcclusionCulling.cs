using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionCulling : MonoBehaviour
{
	//Only Attach it to Renderers
	Renderer r;

    // Start is called before the first frame update
    void Start()
    {
		r = GetComponent<Renderer>();
    }

	void OnBecameInvisible()
	{
		if (r) r.enabled = false; 
	}

	void OnBecameVisible()
	{
		if (r) r.enabled = true;
	}
}
