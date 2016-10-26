using UnityEngine;
using System.Collections;

public class PlaygridController : MonoBehaviour {

	public Vector2 gridSize;
	public Vector2 gridSpacing;

	public GameObject gridPiece;

	// This is how you do a 2D array in C# ---  int[][] is an array of arrays
	public GameObject[,] gridObjects;

	private Vector3[,] gridPositions;

	// Use this for initialization
	void Start () {
	
		gridObjects = new GameObject[(int)gridSize.x + 2, (int)gridSize.y + 2];


		gridPositions = new Vector3[(int)gridSize.x, (int)gridSize.y];


		for (int i =0; i<gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				// set up Grid Piece positions
				// Could probably do a better version where they're even
				gridPositions[i, j] = new Vector3( i * gridSpacing.x, j * gridSpacing.y, 0);
			}
		} 

		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 1; j <= gridSize.y; j++) {
				GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i-1,j-1], Quaternion.identity);
				go.GetComponent<GridpieceController>().type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
				gridObjects[i, j] = go;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
