using UnityEngine;
using System.Collections;

public class GlobalVariables : MonoBehaviour {


	public static Camera cam;
	public static GridpieceController gridC;

	public static bool gameOver;


	// Use this for initialization
	void Start () 
    {
		cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		gameOver = false;
		//gridC = GameObject.FindGameObjectWithTag("GridObject").GetComponent<GridpieceController>();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
