using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip[] notes;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void PlayNote(int baseNote, int combo)
    {
        int max = notes.Length;
        int note = baseNote + combo - 2;
        if (note < max)
        {
            audioSource.PlayOneShot(notes[note]);
        }
        else
        {
            audioSource.PlayOneShot(notes[max - 1]);
        }
    }
}
