using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

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
		if (inputName.Length != 0) {
			ExportBoardToFile(inputName);
			print("Board Saved to file '" + inputName + ".txt'!");
		}
		else
			print("Input field left empty -- Did not save board");
		Time.timeScale = 1;
		this.gameObject.SetActive(false);
	}


	public void ExportBoardToFile(string name) {
		bool DEEXPORTFILE = false;
		string path = null;
		//#if UNITY_EDITOR
		if (Application.isEditor) {
			path = "Assets/Resources/" + name + ".txt";

			string[] playgrid = pgc.StringifyBoard();
			string fileString = "";
			fileString += ((int)pgc.gridSize.x + " " + (int)pgc.gridSize.y + "\n");
			for (int i = 0; i < (pgc.gridSize.x * pgc.gridSize.y); i++) {
				fileString += playgrid[i];
				if ((i+1) % pgc.gridSize.x == 0 && i != 0)
					fileString += "\n";
				else
					fileString += " ";
			}

			if (DEEXPORTFILE)
				print(fileString);

			/*
			using (FileStream fs = new FileStream(path, FileMode.Create)) {
				using (StreamWriter writer = new StreamWriter(fs)) {
					writer.Write(fileString);
				}
			}
			*/
			if (File.Exists(path)) {
				if (DEEXPORTFILE)
					print("Emptying Existing File");
				File.WriteAllText(path, string.Empty);
				if (DEEXPORTFILE)
					print("Wrting new data to file");
				File.WriteAllText(path, fileString);
			}
			else {
				
				if (DEEXPORTFILE)
					print("Creating new file");
				File.Create(path).Close();
				if (DEEXPORTFILE)
					print("Wrting new data to new file");
				File.WriteAllText(path, fileString);

			}

		}

	}


}
