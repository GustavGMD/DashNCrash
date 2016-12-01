using UnityEngine;
using System.Collections;
using System;

public class AudioObject : MonoBehaviour {

    public event Action onFinishedPlaying;

	// Update is called once per frame
	void Update () {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            if (onFinishedPlaying != null) onFinishedPlaying();
        }
	}

    void OnEnable()
    {
        GetComponent<AudioSource>().Play();
    }
}
