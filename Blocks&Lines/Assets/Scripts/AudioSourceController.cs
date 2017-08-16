using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip[] notes;
    public AudioClip freeze;
    public AudioClip thaw;
    public AudioClip warning;
    public AudioClip bomb;


    // public static int lastNote;

	// Use this for initialization
	void Start () {} 

	// Update is called once per frame
	void Update () {}

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

    // GOOD BLOCK SOUNDS
    public void PlaySquiggleSound()
    {
    }
    public void PlayVertSound()
    {
    }
    public void PlayHorizSound()
    {
        // Zap sound
    }
    public void PlayPlusSound()
    {
    }
    public void PlayBombSound()
    {
        audioSource.PlayOneShot(bomb);
    }
    public void PlayFreezeSound()
    {
        audioSource.PlayOneShot(freeze);
    }
    public void PlayThawSound()
    {
        audioSource.PlayOneShot(thaw);
    }

    // BAD BLOCK SOUNDS
    public void PlayUpActivateSound()
    {
        // Some kind of spring
    }
    public void PlayWhiteOutSound()
    {
        // Some kind of spring
    }
    public void PlayRepaintSound()
    {
    }
    public void PlayBadRemoveSound()
    {
    }
    public void PlayBubbleSound()
    {
        // Some kind of spring
    }

    // OTHER SOUNDS
    // When about to lose
    public void PlayWarningSound()
    {
        audioSource.PlayOneShot(warning);
    }
}
