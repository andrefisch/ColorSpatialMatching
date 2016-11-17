using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaygridController : MonoBehaviour {

    private bool DEBUG = true;
    public GameObject gridPiece;
    public GameObject highlighter;
    public GameObject selector;

    // HOW MANY EXTRA ROWS AND COLUMNS THERE ARE
    public const int extraX = 2;
    public const int extraY = 1;

    public Vector2 gridSize;
    public Vector2 gridSpacing;

    //public GridpieceController gpc;
    public Vector2 currentPiece;
    public Vector2 lastPiece;

    // This is how you do a 2D array in C# ---  int[][] is an array of arrays
    public GameObject[,] gridObjects;
    // Keep track of which objects fell down for combo checking
    public List<Vector2> movedObjects;

    //private GridpieceController[,] objectControllers;
    private Vector3[,] gridPositions;

	private bool removePiece;
	private bool addPiece;
	private int selectedX;
	private int selectedY;

    // Use this for initialization
    void Start () {
        currentPiece = new Vector2(-1, -1);
        lastPiece = new Vector2(-1, -1);

        gridObjects = new GameObject[(int)gridSize.x + extraX, (int)gridSize.y + extraY];

		gridPositions = new Vector3[(int)gridSize.x + extraX, (int)gridSize.y + extraY];
		//objectControllers = new GridpieceController[(int)gridSize.x + extraX, (int)gridSize.y + extraY];
        // set up Grid Piece positions
        // Could probably do a better version where they're even around 0 instead of spaced out as I have them currently
		for (int i = 0; i < gridSize.x + extraX; i++) {
            for (int j = 0; j < gridSize.y + extraY; j++) {
                gridPositions[i, j] = new Vector3( i * gridSpacing.x, j * gridSpacing.y, 0);
            }
        } 
        // Set up the actual pieces
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j <= gridSize.y - 1; j++) {
				AddPieceAtPostion(i, j, -1);
            }
        }
		// Set up the edge pieces
		// Go from 0 to size on the left edge
		for (int i = 0; i < gridSize.y; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[0, i], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[0, i] = go;
		}
		// Go from 0 to size on the right edge
		for (int i = 0; i < gridSize.y; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[(int)gridSize.x + extraX - 1, i], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[(int)gridSize.x + extraX - 1, i] = go;
		}
		// Go from 0 to size on the top
		for (int i = 0; i < gridSize.x + extraX; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i, (int)gridSize.y + extraY - 1], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[i, (int)gridSize.y + extraY - 1] = go;
		}
    }

	// Update is called once per frame
	void Update () {
		/*
		// FOR TESTING ADDING AND REMOVAL OF PIECES
		if (Input.GetKeyDown("p")) {
			removePiece = !removePiece;
			if (removePiece)
				addPiece = false;
		}
		if (Input.GetKeyDown("o")) {
			addPiece = !addPiece;
			if (addPiece)
				removePiece = false;
		}
		// DONE WITH TESTING
		*/

		// This part deals with the highlighting and selecting of objects
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).x,GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
		if (hit.collider != null) {

			GridpieceController gpc = hit.collider.gameObject.GetComponent<GridpieceController>();
			gpc.highlighted = true;

			if (Input.GetMouseButtonDown(0)) {
				// First deselect any selected piece
				for (int i = 1; i <= gridSize.x; i++) {
					for (int j = 0; j < gridSize.y; j++) {
						if (gridObjects[i,j]){
							gridObjects[i,j].GetComponent<GridpieceController>().selected = false;
						}
					}
				}
				// Select only pieces that aren't 0 and are in the playable grid
				if (gpc.type != 0 && (gpc.dimX >= 1 && gpc.dimX <= gridSize.x && gpc.dimY >= 0 && gpc.dimY < gridSize.y)) {

					// if it's not new, toggle the selection
					if (gpc.dimX == currentPiece.x && gpc.dimY == currentPiece.y) {
						gpc.selected = false;
						ResetCurrentLastPieces();
					}
					else {
						gpc.selected = true;

						if (currentPiece.x != -1 && currentPiece.y != -1) {
							lastPiece.x = currentPiece.x;
							lastPiece.y = currentPiece.y;
						}
						currentPiece.x = gpc.dimX;
						currentPiece.y = gpc.dimY;
					}
				
					if (DEBUG) {
						Debug.Log("----------------------Clicked Playgrid Piece----------------------------");
						Debug.Log("Dimensions of selected piece: " + currentPiece.x + ", " + currentPiece.y);
						Debug.Log("Dimensions of last selected piece: " + lastPiece.x + ", " + lastPiece.y);
					}
				}
			}

		}
		else {
			// The mouse is not hovering over any of the objects, you dehighlight any of them
			for (int i = 1; i <= gridSize.x; i++) {
				for (int j = 0; j < gridSize.y; j++) {
					if (gridObjects[i,j])
						gridObjects[i,j].GetComponent<GridpieceController>().highlighted = false;
				}
			}
			// If you click away, it deselects any piece
			if (Input.GetMouseButtonDown(0)) {
				ResetCurrentLastPieces();
				for (int i = 1; i <= gridSize.x; i++) {
					for (int j = 0; j < gridSize.y; j++)
						if (gridObjects[i,j])
							gridObjects[i,j].GetComponent<GridpieceController>().selected = false;
				}
			}
		}

		// Move the highlighted and selected indicator to their proper positions.
		bool highlightedPiece = false;
		bool selectedPiece = false;
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().highlighted) {
					highlighter.transform.position = gridPositions[i, j];
					highlightedPiece = true;
				}
				if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().selected) {
					selector.transform.position = gridPositions[i, j];
					selectedPiece = true;
				}
			}
		}
		if (!highlightedPiece)
			highlighter.transform.position = new Vector3(-10, 10, 0);
		if (!selectedPiece)
			selector.transform.position = new Vector3(-10, 10, 0);



		// Check for matches
		if (lastPiece.x > -1) {
			if (Match(gridObjects, (int)currentPiece.x, (int)currentPiece.y, (int)lastPiece.x, (int)lastPiece.y)) {
				Debug.Log("THEY MATCH!");
				ProcessBoard(gridObjects);
			}
			else {
				Debug.Log("NO MATCH...");
			}
		}


	}

    // do the two blocks match?
    // NO KNOWN BUGS
    bool Match(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        if (DEBUG)
        {
            Debug.Log("----------------------------Running Matching Algorithm-----------------------------------");
        }
        bool match = false;
        // make sure the pieces are all valid pieces
        if (x1 >= 0 && x1 <= gridSize.x + extraX - 1 &&
            x2 >= 0 && x2 <= gridSize.x + extraX - 1 &&
            y1 <= gridSize.y + extraY - 1 &&
            y2 <= gridSize.y + extraY - 1)
        {
            // if the blocks arent the same type no more checking needs to be done
            if (board[x1, y1].GetComponent<GridpieceController>().type == board[x2, y2].GetComponent<GridpieceController>().type)
            {
                if (DEBUG)
                {
                    Debug.Log("Now checking adjacency of " + x1 + ", " + y1 + " and " + x2 + ", " + y2);
                }
                // if the blocks are adjacent we are done and no more checking needs to happen
                if (Adjacent(board, x1, y1, x2, y2))
                {
                    // board[x1, y1] = null;
                    // board[x2, y2] = null;
                    if (DEBUG)
                    {
                        Debug.Log("They are adjacent so we have a match!");
                    }
                    match = true;
                }
                // then we check to see if there is a straight shot from A to B
                else if (StraightShot(board, x1, y1, x2, y2))
                {
                    match = true;
                }
                else
                {
                    if (DEBUG)
                    {
                        Debug.Log("Not adjacent or a straight shot so checking match trails");
                    }
                    // create the match trails
                    match = CheckMatchTrails(board, CreateMatchTrail(board, x1, y1), CreateMatchTrail(board, x2, y2));
                }
            }
        }
        if (DEBUG)
        {
            Debug.Log("Do we have a match?: " + match);
        }
        if (match)
        {
            // make sure we set the pieces to 0 and gray
            gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
            gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
            // make sure we deselect and unhighlight last piece
			gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().selected = false;
			// Make sure we reset current piece
			currentPiece.x = -1;
			currentPiece.y = -1;
        }
		// Reset the last piece so that we aren't checking for a match every frame
		lastPiece.x = -1;
		lastPiece.y = -1;
        MatchTrailCleanup(board);
        return match;
    }
    

    // are the two blocks adjacent?
    // WORKS
    bool Adjacent(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        if ((Mathf.Abs(x1 - x2) == 1 && Mathf.Abs(y1 - y2) == 0) || (Mathf.Abs(x1 - x2) == 0 && Mathf.Abs(y1 - y2) == 1))
        {
            if (DEBUG)
            {
                Debug.Log("Now comparing " + x1 + "; " + y1 + " and " + x2 + ", " + y2);
            }
            if (board[x1, y1].GetComponent<GridpieceController>().type == board[x2, y2].GetComponent<GridpieceController>().type)
            {
                return true;
            } 
            else 
            {
                return false;
            }
        } 
        else 
        {
            return false;
        }
    }

    // THE DIRECTIONS NEED TO BE CHANGED
    // creates a trail of -1's from all possible directions of input block
    // WORKS
    List<Vector2> CreateMatchTrail(GameObject[,] board, int x1, int y1)
    {
        List<Vector2> output = new List<Vector2>();
        // create a trail in up to 4 possible directions for both blocks
        // check down only if we are not at the bottom row
        if (DEBUG)
        { 
            Debug.Log ("now checking down from " + x1 + ", " + y1); 
        }
        for (int i = y1 - 1; i > 0; i--)
        {
            if (DEBUG)
            {
                Debug.Log ("Now checking " + x1 + ", " + i);
            }
            if (board[x1, i].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    Debug.Log("Added a -1!");
                }
                board[x1, i].GetComponent<GridpieceController>().type = -1; 
                output.Add(new Vector2(x1, i)); 
            }
            else
            {
                break;
            }
        }
        // check up
        if (DEBUG)
        {
            Debug.Log ("now checking up from " + x1 + ", " + y1);
        }
        for (int i = y1 + 1; i < gridSize.y + extraY; i++)
        {
            if (DEBUG)
            {
                Debug.Log ("Now checking " + x1 + ", " + i);
            }
            if (board[x1, i].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    Debug.Log("Added a -1!");
                }
                board[x1, i].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(x1, i));
            }
            else
            {
                break;
            }
        }
        // check right
        if (DEBUG)
        {
            Debug.Log("now checking right from " + x1 + ", " + y1);
        }
        for (int i = x1 + 1; i < gridSize.x + extraX; i++)
        {
            if (DEBUG)
            {
                Debug.Log ("Now checking " + i + ", " + y1);
            }
            if (board[i, y1].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    Debug.Log("Added a -1!");
                }
                board[i, y1].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(i, y1));
            }
            else
            {
                break;
            }
        }
        // check left
        if (DEBUG)
        {
            Debug.Log("now checking left from " + x1 + ", " + y1);
        }
        for (int i = x1 - 1; i > -1; i--)
        {
            if (DEBUG)
            {
                Debug.Log("Now checking " + x1 + ", " + i);
            }
            if (board[i, y1].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    Debug.Log("Added a -1!");
                }
                board[i, y1].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(i, y1));
            }
            else
            {
                break;
            }
        }
        // if the output list is empty we may have a 90* match so add coordinates of tile for checking
        if (output.Count == 0)
        {
            output.Add(new Vector2(x1, y1));
        }
        return output;
    }

    // checks to see if there is anything but 0's between the two spots
    // will even work with indeces that are one out of bounds so we can
    // test blocks on the edge against each other
    // WORKS
    bool StraightShot(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        bool output = false;
        if (x1 == x2)
        {
            if (y2 > y1)
            {
                for (int y = y1 + 1; y < y2; y++)
                {
                    if (board[x1, y].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int y = y2 + 1; y < y1; y++)
                {
                    if (board[x1, y].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            output = true;
        }
        else if (y1 == y2)
        {
            if (x2 > x1)
            {
                for (int x = x1 + 1; x < x2; x++)
                {
                    if (board[x, y1].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int x = x2 + 1; x < x2; x++)
                {
                    if (board[x, y1].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            output = true;
        }
        return output;
    }


    // checks to see if there is a straightShot between any -1 in list1 and list2
    // WORKS
    bool CheckMatchTrails(GameObject[,] board, List<Vector2> list1, List<Vector2> list2)
    {
        bool match = false;
        if (DEBUG)
        {
            Debug.Log("Checking this list");
            for (int i = 0; i < list1.Count; i++)
            {
                Debug.Log(list1[i].x + ", " + list1[i].y);
            }
            Debug.Log("Against this one");
            for (int i = 0; i < list2.Count; i++)
            {
                Debug.Log(list2[i].x + ", " + list2[i].y);
            }
        }
        // check straight shots for each list
        for (int i = 0; i < list1.Count; i++)
        {
            for (int j = 0; j < list2.Count; j++)
            {
                if (StraightShot(board, (int)list1[i].x, (int)list1[i].y, (int)list2[j].x, (int)list2[j].y))
                {
                    return true;
                }
            }
        }
        return match;
    }

    // remove all -1's from the board
    // WORKS
    void MatchTrailCleanup(GameObject[,] board)
    {
        for (int i = 0; i < gridSize.x + extraX; i++)
        {
            for (int j = 0; j < gridSize.y + extraY; j++)
            {
				if (board[i,j] && board[i, j].GetComponent<GridpieceController>().type == -1)
                {
                    board[i, j].GetComponent<GridpieceController>().type = 0;
                }
            }
        }
    }

    // Returns true if there are no empty blocks above this one in the playable game space.
	// Returns false otherwise
    // UNTESTED
    bool OnlyEmptySpaceAboveBlock(GameObject[,] board, int x, int y)
    {
        bool onTop = true;
        for (int i = y + 1; i < gridSize.y; i++)
        {
            if (board[x, i].GetComponent<GridpieceController>().type != 0)
            {
                onTop = false;
                return onTop;
            }
        }
        return onTop;
    }

    // Move all blocks that can be moved down as far as possible
    // Keep track of which blocks moved so that we can combo later
    // UNTESTED
    void ProcessBoard(GameObject[,] board)
    {
		if (DEBUG)
			Debug.Log("-------------------------- Processing Board ----------------------------");
        // First remove all the grey pieces in the playable grid
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().type == 0)
                    // first get rid of the piece
                    RemovePieceAtPosition(i, j);
            }
        }
		// Next, move pieces down.  I'm currently not accounting for any pieces larger than 1x1
		// TODO:  FOR ANDREW-- This part mostly works.  It needs to be updated for moving pieces further down than one place without being buggy
		// Iterate through the grid
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				// If you come across a gap in the grid that isn't in the top row, bring the objects down
				if (gridObjects[i, j] == null && j+1 < gridSize.y) {
					if (gridObjects[i, j + 1] == null && j + 2 < gridSize.y && gridObjects[i, j+2] != null) {
						MovePieceToPosition(gridObjects[i, j + 2], i, j);
						i++;
					}
					else if (gridObjects[i, j+1] != null)
						MovePieceToPosition(gridObjects[i, j + 1], i, j);
				}
			}
		}
		// Finally, fill the board back up with grey pieces
		/*  This part should be implmented once the above move-pieces works perfectly
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i, j] == null)
					AddPieceAtPostion(i, j, 0);
			}
		}
		*/
    }

	// Removes any piece
	public void RemovePieceAtPosition(int x, int y) {
		if (DEBUG)
			Debug.Log("Removing block at space " + x + ", " + y);
		if (gridObjects[x, y]) {
			GameObject.Destroy(gridObjects[x, y]);
			gridObjects[x, y] = null;
		}
	}

    // Add a piece
    // - if the num supplied is negative we choose a number randomly
    // - otherwise the type is the num we supplied
	public GameObject AddPieceAtPostion(int x, int y, int num) {
		GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[x, y], Quaternion.identity);
		GridpieceController gpc = go.GetComponent<GridpieceController>();
        if (num < 0)
			gpc.type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
        else
            gpc.type = num;
        
		gpc.dimX = x;
		gpc.dimY = y;
		gridObjects[x, y] = go;
		//if (DEBUG)
		//	Debug.Log("Created Piece at position: " + x + ", " + y);
		return go;
	}


	public void MovePieceToPosition(GameObject piece, int x, int y) {
		
		if (piece == null)
			Debug.Log("Warning, piece given in MovePiece method does not exist");
		else if (y < 0 || y >= gridSize.y + extraY)
			Debug.Log("Warning, trying to move piece below or above grid Y limits");
		else if (x < 0 || x >= gridSize.x + extraX)
			Debug.Log("Warning, trying to move piece below or above grid X limits");
		else if (gridObjects[x, y] != null)
			Debug.Log("Warning, trying to move piece into occupied space " + x + ", " + y);
		else {
			GridpieceController gpc = piece.GetComponent<GridpieceController>();

			if (DEBUG)
				Debug.Log("Moving Piece: [" + gpc.dimX + ", " + gpc.dimY + "] to [" + x + ", " + y + "]");

			gridObjects[gpc.dimX, gpc.dimY] = null;
			gridObjects[x, y] = piece;
			gpc.dimX = x;
			gpc.dimY = y;
			piece.transform.position = gridPositions[x, y];
			// Can swap the line above for the line below in order to create a moving piece
			//StartCoroutine(MovePiece(piece, x, y));
		}
	}

	public IEnumerator MovePiece(GameObject piece, int x, int y) {
		float timeForPieceToMove = 0.5f;
		Vector3 startPos = piece.transform.position;
		for (float i = 0; i < timeForPieceToMove; i += Time.deltaTime) {
			float counter = i / timeForPieceToMove;
			piece.transform.position = Vector3.Lerp(startPos, gridPositions[x, y], counter);
			yield return null;
		}
	}

	public void ResetCurrentLastPieces() {
		currentPiece = Vector2.one * -1;
		lastPiece = Vector2.one * -1;
	}
}
