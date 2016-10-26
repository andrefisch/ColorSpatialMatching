using UnityEngine;
using System.Collections;

public class PlaygridController : MonoBehaviour {

	public GameObject gridPiece;
	public GameObject highlighter;
	public GameObject selector;

	public Vector2 gridSize;
	public Vector2 gridSpacing;

	// This is how you do a 2D array in C# ---  int[][] is an array of arrays
	public GameObject[,] gridObjects;

	private GridpieceController[,] objectControllers;
	private Vector3[,] gridPositions;

	private Vector2 selectedPiece;
	// Use this for initialization
	void Start () {
	
		gridObjects = new GameObject[(int)gridSize.x + 2, (int)gridSize.y + 2];


		gridPositions = new Vector3[(int)gridSize.x, (int)gridSize.y];
		objectControllers = new GridpieceController[(int)gridSize.x, (int)gridSize.y];

		// set up Grid Piece positions
		// Could probably do a better version where they're even around 0 instead of spaced out as I have them currently
		for (int i =0; i<gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				gridPositions[i, j] = new Vector3( i * gridSpacing.x, j * gridSpacing.y, 0);
			}
		} 

		// Set up the actual pieces
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 1; j <= gridSize.y; j++) {
				GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i-1,j-1], Quaternion.identity);
				objectControllers[i-1,j-1] = go.GetComponent<GridpieceController>();
				objectControllers[i-1, j-1].type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
				gridObjects[i, j] = go;
			}
		}


		//selectedPiece = new Vector2(-1,-1);


	}
	
	// Update is called once per frame
	void Update () {

		bool highlightedPiece = false;
		bool selectedPiece = false;
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 1; j <= gridSize.y; j++) {
				if (objectControllers[i - 1, j - 1].highlighted) {
					highlighter.transform.position = gridPositions[i - 1, j - 1];
					highlightedPiece = true;
				}
				if (objectControllers[i-1, j-1].selected) {
					selector.transform.position = gridPositions[i-1, j-1];
					selectedPiece = true;
				}
			}
		}

		if (!highlightedPiece)
			highlighter.transform.position = new Vector3(-10, 10, 0);
		if (!selectedPiece)
			selector.transform.position = new Vector3(-10, 10, 0);

		// This part deals with the highlighting and selecting of objects
		// This should be updated so that we don't have to get the controller components every frame if we highlight
		// Also, it feels very janky and can probably be improved a bunch
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).x,GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
		if (hit.collider != null) {
			hit.collider.gameObject.GetComponent<GridpieceController>().highlighted = true;
			if (Input.GetMouseButtonDown(0)) {
				for (int i = 1; i <= gridSize.x; i++) {
					for (int j = 1; j <= gridSize.y; j++) {
						// FOR ANDREW:  Based on your algorithm, this part might screw with it because when you select a new object, it goes through and deselects all the existing objects before selecting the new one
						// This is because I don't keep track well enough of the selected piece.  Probably should track it better throughout
						objectControllers[i - 1, j - 1].selected = false;
						}
					}
				hit.collider.GetComponent < GridpieceController>().selected = true;
			}
		}
		else {
			for (int i = 1; i <= gridSize.x; i++) {
				for (int j = 1; j <= gridSize.y; j++) {
					objectControllers[i-1, j-1].highlighted = false;
				}
			}
			// If you click away, it deselects any piece
			if (Input.GetMouseButtonDown(0)) {
				for (int i = 1; i <= gridSize.x; i++) {
					for (int j = 1; j <= gridSize.y; j++)
						objectControllers[i - 1, j - 1].selected = false;
				}
			}
		}
			
	}
}
