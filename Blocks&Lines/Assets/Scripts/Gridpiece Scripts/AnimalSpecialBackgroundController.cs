using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpecialBackgroundController : MonoBehaviour {

	public int color;

	private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetColor() {

		if (sr == null)
			sr = GetComponent<SpriteRenderer>();

		if (color == 1) {
			sr.color = Color.red;	
		}
		else if (color == 2) {
			sr.color = new Color(1, 142f / 255f, 0);	
		}
		else if (color == 3) {
			sr.color = Color.yellow;	
		}
		else if (color == 4) {
			sr.color = Color.green;	
		}
		else if (color == 5) {
			sr.color = Color.blue;	
		}
		else if (color == 6) {
			sr.color = new Color(1, 86f / 255f, 1);
		}
		else if (color == 7) {
			sr.color = new Color(135f / 255f, 0, 1);	
		}
		else if (color == 8) {
			sr.color = new Color(208f / 255f, 135f / 255f, 35f / 255f);
		}
		else if (color == 9) {
			sr.color = Color.grey;	
		}
		else if (color == 10) {
			sr.color = Color.white;	
		}
		else {
			Debug.LogWarning("Warning (SetColor-ASBC):  Attempting to set color -- " + color + " --that doesn't correspond to proper background color -- Setting Color to Red");
			color = 1;
			sr.color = Color.red;
		}
	
	}

}
