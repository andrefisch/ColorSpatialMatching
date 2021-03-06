﻿using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
    public int segments;
    public float radius;
    public LineRenderer line;
    public Color color;
    public int maxRadius;
    public float change;
    public Material circleMat;

    void Start ()
    {
        line = gameObject.GetComponent<LineRenderer>();
        // line.SetColors(color, color);
        line.startColor = color;
        line.endColor = color;
        line.positionCount = (segments + 1);
        line.useWorldSpace = false;
        change = maxRadius / 30f;
        // Debug.Log("maxRadius is " + maxRadius);
        CreatePoints ();
    }

    void Update()
    {
        bool DEUPDATE = false;
        if (radius < maxRadius)
        {
            if (DEUPDATE)
            {
                Debug.Log("Current radius is " + radius);
            }
            radius += change;
        }
        /*
        if (radius < maxRadius)
        {
            if (DEUPDATE)
            {
                Debug.Log("Current radius is " + radius);
            }
            radius += 0.15f;
        }
        */
        else if (color.a > 0)
        {
            color.a -= 0.04f;
            if (DEUPDATE)
            {
                Debug.Log("Start color is: " + color.a);
            }
            // line.SetColors(color, color);
            line.startColor = color;
            line.endColor = color;

        }
        else if (color.a <= 0)
        {
            Destroy(this.gameObject);
        }

        // line.SetWidth(0.4f, 0.4f);
        line.startWidth = 0.4f;
        line.endWidth = 0.4f;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.up);
        Material whiteDiffuseMat = circleMat;
        line.material = whiteDiffuseMat;
        CreatePoints();
    }


    void CreatePoints ()
    {
        float x;
        float y;
        float z = 0f;

        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin (Mathf.Deg2Rad * angle);
            y = Mathf.Cos (Mathf.Deg2Rad * angle);
            line.SetPosition (i, new Vector3(x, y, z) * radius);
            angle += (360f / segments);
        }
    }
}
