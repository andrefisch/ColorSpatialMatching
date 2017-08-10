using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip[] notes;
    public AudioClip freeze;
    public AudioClip thaw;
    public AudioClip warning;


    // public static int lastNote;

	// Use this for initialization
	void Start () {
        // lastNote = 0;
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void PlayNote(int note)
    {
        bool DEPLAYNOTE = false;
        int max = notes.Length - 1;
        if (note < 0)
        {
            audioSource.PlayOneShot(notes[0]);
            if (DEPLAYNOTE)
                Debug.Log("Now playing note number 0");
        }
        else if (note < max)
        {
            audioSource.PlayOneShot(notes[note]);
            if (DEPLAYNOTE)
                Debug.Log("Now playing note number " + note);
        }
        else
        {
            audioSource.PlayOneShot(notes[max]);
            if (DEPLAYNOTE)
                Debug.Log("Now playing note number " + max);
        }
    }
    /*
    public void PlayNote(int baseNote, int combo)
    {
        bool DEPLAYNOTE = true;
        int max = notes.Length - 1;
        if (lastNote == 0)
        {
            lastNote = baseNote + combo - 2;
            if (lastNote < 0)
            {
                audioSource.PlayOneShot(notes[0]);
                if (DEPLAYNOTE)
                    Debug.Log("Now playing note number 0");
            }
            else if (lastNote < max)
            {
                audioSource.PlayOneShot(notes[lastNote]);
                if (DEPLAYNOTE)
                    Debug.Log("Now playing note number " + lastNote);
            }
            else
            {
                audioSource.PlayOneShot(notes[max]);
                if (DEPLAYNOTE)
                    Debug.Log("Now playing note number " + max);
            }
        }
        else
        {
            if (lastNote < max)
            {
                lastNote++;
                audioSource.PlayOneShot(notes[lastNote]);
                if (DEPLAYNOTE)
                    Debug.Log("Now playing note number " + lastNote);
            }
            else
            {
                audioSource.PlayOneShot(notes[max]);
                if (DEPLAYNOTE)
                    Debug.Log("Now playing note number " + max);
            }
        }
    }
    */

    public void PlayFreezeSound()
    {
        audioSource.PlayOneShot(freeze);
    }

    public void PlayThawSound()
    {
        audioSource.PlayOneShot(thaw);
    }

    public void PlayWarningSound()
    {
        audioSource.PlayOneShot(warning);
    }
}
