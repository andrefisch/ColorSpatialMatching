using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    Image fillImg;
    float total;
    float current;

	private PlaygridController playgrid;

    // Use this for initialization
    void Start () 
    {
        fillImg = this.GetComponent<Image>();

		playgrid = GameObject.FindGameObjectWithTag("GameController").GetComponent<PlaygridController>();

    }

    // Update is called once per frame
    void Update () 
    {
		total = playgrid.newLineInterval;
		current = playgrid.newLineCounter;
        fillImg.fillAmount = (total - current) / total;
    }
}

