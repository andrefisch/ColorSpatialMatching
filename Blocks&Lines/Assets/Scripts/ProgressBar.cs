using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    Image fillImg;
    float total;
    float current;

    // Use this for initialization
    void Start () 
    {
        fillImg = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () 
    {
        total = GameObject.Find("Playgrid").GetComponent<PlaygridController>().newLineInterval;
        current = GameObject.Find("Playgrid").GetComponent<PlaygridController>().newLineCounter;
        fillImg.fillAmount = (total - current) / total;
    }
}

