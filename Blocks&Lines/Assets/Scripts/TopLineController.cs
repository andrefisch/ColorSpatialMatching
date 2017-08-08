using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopLineController : MonoBehaviour 
{
    public GameObject playGrid;
	public SpriteRenderer sr;
    public int counter;
    public bool justReachedTop;
    public bool playedWarning;
	// Use this for initialization
	void Start () 
    {
		sr.color = Color.green;
        counter = 0;
        justReachedTop = false;
        playedWarning = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (counter < 15)
        {
            counter++;
        }
        else
        {
            counter = 0;
        }
	    if (playGrid.GetComponent<PlaygridController>().AlmostLost())
        {
            justReachedTop = true;
            if (counter == 15)
            {
                ToggleColor();
            }
        }
        else
        {
            justReachedTop = false;
            playedWarning = false;
            sr.color = Color.green;
        }
        if (justReachedTop && !playedWarning)
        {
            playGrid.GetComponent<AudioSourceController>().PlayWarningSound();
            playedWarning = true;
        }
	}

    void ToggleColor()
    {
        if (sr.color == Color.green)
        {
            sr.color = Color.red;
        }
        else
        {
            sr.color = Color.green;
        }
    }
}
