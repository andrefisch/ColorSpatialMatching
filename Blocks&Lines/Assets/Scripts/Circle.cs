using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
    public int segments;
    public float radius;
    public LineRenderer line;
    public Color color;

    void Start ()
    {
        // color = gameObject.GetComponent<SpriteRenderer>().color;
        // color = Color.red;
        line = gameObject.GetComponent<LineRenderer>();
        line.SetColors(color, color);
        line.SetVertexCount (segments + 1);
        line.useWorldSpace = false;
        CreatePoints ();
    }

    void Update()
    {
        bool DEUPDATE = false;
        if (radius < 3)
        {
            radius += 0.13f;
        }
        else if (color.a > 0)
        {
            color.a -= 0.04f;
            if (DEUPDATE)
            {
                Debug.Log("Start color is: " + color.a);
            }
            line.SetColors(color, color);
        }

        line.SetWidth(0.4f, 0.4f);
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.up);
        Material whiteDiffuseMat = new Material(Shader.Find("Particles/Additive (Soft)"));
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
