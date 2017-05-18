using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    Image fillImg;
    float total;
    float current;
    bool pause;

    // Use this for initialization
    void Start () 
    {
        fillImg = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () 
    {
        pause = GameObject.Find("Playgrid").GetComponent<PlaygridController>().pause;
        current = GameObject.Find("Playgrid").GetComponent<PlaygridController>().newLineCounter;
        total = GameObject.Find("Playgrid").GetComponent<PlaygridController>().newLineInterval;
        if (!pause)
        {
            fillImg.fillAmount = (total - current) / total;
        }
    }
}

