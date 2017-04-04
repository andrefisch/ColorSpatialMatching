using UnityEngine;
using System.Collections;

public class GlobalVariables : MonoBehaviour {


	public static Camera cam;
	public static GridpieceController gridC;

	// Use this for initialization
	void Start () {
		cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

		//gridC = GameObject.FindGameObjectWithTag("GridObject").GetComponent<GridpieceController>();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
