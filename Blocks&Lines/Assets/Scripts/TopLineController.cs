using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopLineController : MonoBehaviour {
    public GameObject playGrid;
	public SpriteRenderer sr;
	// Use this for initialization
	void Start () {
		sr.color = Color.green;
	}
	
	// Update is called once per frame
	void Update () {
        
	    if (playGrid.GetComponent<PlaygridController>().AlmostLost())
        {
            sr.color = Color.red;
        }
        else
        {
            sr.color = Color.green;
        }
	}
}
