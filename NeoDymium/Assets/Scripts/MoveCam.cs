using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
	public Camera yes;
	float time;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (time > 2)
		{
			yes.enabled = !yes.enabled;
			time = 0;
		}
		time++;
    }
}
