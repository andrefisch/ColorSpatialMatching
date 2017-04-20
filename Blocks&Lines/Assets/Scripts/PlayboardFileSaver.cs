using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayboardFileSaver : MonoBehaviour {

	public PlaygridController pgc;

	public InputField input;

	private string inputName;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		inputName = input.text.Trim();

		//print(inputName);
	}

	public void SaveBoardFile() {
		if (inputName.Length != 0)
			ExportBoardToFile(inputName);
		else
			print("Input field left empty -- Did not save board");
		Time.timeScale = 1;
		this.gameObject.SetActive(false);
	}

	public void ExportBoardToFile(string name) {
		
	}
}
