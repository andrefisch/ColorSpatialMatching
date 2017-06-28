using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaveOverTextsImage : MonoBehaviour {

	public Text gameOverText;
	public Text resetText;
	public Image im;





	// Use this for initialization
	void Start () {
		gameOverText.enabled = false;
		resetText.enabled = false;
		im.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

		gameOverText.enabled = GlobalVariables.gameOver;
		resetText.enabled = GlobalVariables.gameOver;
		im.enabled = GlobalVariables.gameOver;


	}
}
