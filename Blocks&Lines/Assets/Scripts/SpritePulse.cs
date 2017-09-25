using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpritePulse : MonoBehaviour {
    
    private SpriteRenderer sr;

    private Color clearCol = new Color(1, 1, 1, 0);
    private Color fullCol = new Color(1, 1, 1, .6f);

    // Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Pulse());
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private IEnumerator Pulse() {
        float i = 0;
        bool fadeIn = true;
        while (true) {
            if (fadeIn) {
                sr.color = Color.Lerp(clearCol, fullCol, i);
                i += Time.deltaTime;

                if (i > .6f)
                    fadeIn=false;
            }
            else {
                sr.color = Color.Lerp(clearCol, fullCol, i);
                i -= Time.deltaTime;

                if (i < .05f)
                    fadeIn=true;
        
            }
        
        
            yield return null;
        }
        
    }

}
