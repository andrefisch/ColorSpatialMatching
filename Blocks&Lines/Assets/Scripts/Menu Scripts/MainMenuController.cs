using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

	public GameObject bubbleExplosion;

	private bool changing;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) || (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1))) {
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Instantiate(bubbleExplosion, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
		}
	}

	public void PlayGame() {
		if (!changing) {
			changing = true;
			SceneManager.LoadScene("ArcadeMode");	
		}
	}
		
	public void PlayTutorial() {
		if (!changing) {
			changing = true;
			SceneManager.LoadScene("Tutorial");
		}
	}

	public void ToggleMusic() {
		GlobalVariables.musicOn = !GlobalVariables.musicOn;
	}

	public void ToggleSound() {
		GlobalVariables.soundOn = !GlobalVariables.soundOn;
	}

}
