using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlaygridController : MonoBehaviour {

    private bool DEBUG = true;
    public GameObject gridPiece;
    // 1x1 is 0, 1x2 is 1, 2x1 is 2, 2x2 is 3
	public GameObject[] highlighters;
    public GameObject[] selectors;

    // HOW MANY EXTRA ROWS AND COLUMNS THERE ARE
    public const int extraX = 2;
    public const int extraY = 1;

    public Vector2 gridSize;
    public Vector2 gridSpacing;

	public bool includeBigPieces;

    //public GridpieceController gpc;
	private Vector2 currentPiece;
	private Vector2 lastPiece;

    // This is how you do a 2D array in C# ---  int[][] is an array of arrays
	public GameObject[,] gridObjects;
    // Keep track of which objects fell down for combo checking
	public List<GameObject> movedObjects;
	public List<GameObject> objectsToProcess;

    //private GridpieceController[,] objectControllers;
    private Vector3[,] gridPositions;

	private bool removePiece;
	private bool addPiece;

	//  Thos os tp help keep track of the highlighted piece's position so that it doesn't get messed up if you don't happen to move the mouse off a piece between frames 
	private Vector2 highlightedPiece;

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
		for (int i = (int)gridSize.x; i >= 1; i--) {
			for (int j = (int)gridSize.y - 1; j >= 0; j--) {
				if (!gridObjects[i, j]) {
					if (i >= 1 && j > 0 && includeBigPieces)
						AddPieceAtPosition(i, j, -1, -1);
					else 
						AddPieceAtPosition(i, j, -1, GridpieceController.ONExONE);
				}
            }
        }
		// Set up the edge pieces
		// Go from 0 to size on the left edge
		for (int i = 0; i < gridSize.y; i++) {
			AddPieceAtPosition(0, i, 0, GridpieceController.ONExONE);
		}
		// Go from 0 to size on the right edge
		for (int i = 0; i < gridSize.y; i++) {
			AddPieceAtPosition((int)gridSize.x + extraX - 1, i, 0, GridpieceController.ONExONE);
		}
		// Go from 0 to size on the top
		for (int i = 0; i < gridSize.x + extraX; i++) {
			AddPieceAtPosition(i, (int)gridSize.y + extraY - 1, 0, GridpieceController.ONExONE);
		}
    }

	// Update is called once per frame
    // NO KNOWN BUGS
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
        // check to see if there are any null blocks
        if (Input.GetKeyDown("a")) {
            AddRow();
        }
        // FOR DEBUGGING MAKES SURE THERE ARE NO BLANK SPACES IN THE GRID
        if (Input.GetKeyDown("c")) {
            CheckPieces();
        }
        // move down pieces
        if (Input.GetKeyDown("m")) {
            MovePiecesDown();
        }
        // process combos
        if (Input.GetKeyDown("p")) {
            ProcessCombos();
        }
        // remove row of selected piece
        if (Input.GetKeyDown("q")) {
            RemoveRow();
        }
        // remove column of selected piece
        if (Input.GetKeyDown("w")) {
            RemoveColumn();
        }
        // remove row and column of selected piece
        if (Input.GetKeyDown("e")) {
            RemoveRowAndColumn();
        }
        // remove blocks same color as selected piece
        if (Input.GetKeyDown("r")) {
            RemoveOneColor();
        }
	
		// This part deals with the highlighting and selecting of objects
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).x,GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
		// We need these so that the highlighter and selector part knows the size of the piece we're dealing with so that it can use the proper image
		int gpcHighlightSize = -1;
		highlightedPiece = Vector2.one * -1;
		int gpcSelectSize = -1;
		if (hit.collider != null) {

			GridpieceController gpc = hit.collider.gameObject.GetComponent<GridpieceController>();
			gpcHighlightSize = gpc.size;
			if (gpc.type != 0 && (gpc.dimX >= 1 && gpc.dimX <= gridSize.x && gpc.dimY >= 0 && gpc.dimY < gridSize.y)) {
				highlightedPiece.x = gpc.dimX;
				highlightedPiece.y = gpc.dimY;
			}

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
						gpcSelectSize = gpc.size;
						gpc.selected = true;
                        if (DEBUG)
                        {
                            Debug.Log("Grid spaces of this block are:");
                            Vector2[] gridSpaces = gpc.GetComponent<GridpieceController>().GetPositions();
                            for (int i = 0; i < gridSpaces.Length; i++)
                            {
                                Debug.Log((int)gridSpaces[i].x + ", " + (int)gridSpaces[i].y);
                            }
                        }

						if (currentPiece.x != -1 && currentPiece.y != -1) {
							lastPiece.x = currentPiece.x;
							lastPiece.y = currentPiece.y;
						}
						currentPiece.x = gpc.dimX;
						currentPiece.y = gpc.dimY;
					}
				
					if (DEBUG) {
						// Debug.Log("----------------------Clicked Playgrid Piece----------------------------");
						// Debug.Log("Dimensions of selected piece: " + currentPiece.x + ", " + currentPiece.y);
						// Debug.Log("Dimensions of last selected piece: " + lastPiece.x + ", " + lastPiece.y);
					}
				}
			}

		}
		else {
			gpcHighlightSize = -1;

			// If you click away, it deselects any piece
			if (Input.GetMouseButtonDown(0)) {
				ResetCurrentLastPieces();
				for (int i = 1; i <= gridSize.x; i++) {
					for (int j = 0; j < gridSize.y; j++)
						if (gridObjects[i,j])
							gridObjects[i,j].GetComponent<GridpieceController>().selected = false;
				}
				gpcSelectSize = -1;
			}
		}

		// Move the highlighted and selected indicator to their proper positions.
		bool highlighted = false;
		bool selectedPiece = false;
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i, j]) {
					GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
					if (highlightedPiece.x != -1 && highlightedPiece.y != -1) {
						if (gpcHighlightSize == GridpieceController.ONExONE) {
							highlighters[GridpieceController.ONExONE].transform.position = gridPositions[(int)highlightedPiece.x, (int)highlightedPiece.y];
							highlighters[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcHighlightSize == GridpieceController.ONExTWO) {
							highlighters[GridpieceController.ONExTWO].transform.position = Vector3.Lerp(gridPositions[(int)highlightedPiece.x, (int)highlightedPiece.y], gridPositions[(int)highlightedPiece.x, (int)highlightedPiece.y - 1], 0.5f);
							highlighters[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcHighlightSize == GridpieceController.TWOxONE) {
							highlighters[GridpieceController.TWOxONE].transform.position = Vector3.Lerp(gridPositions[(int)highlightedPiece.x, (int)highlightedPiece.y], gridPositions[(int)highlightedPiece.x - 1, (int)highlightedPiece.y], 0.5f);
							highlighters[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcHighlightSize == GridpieceController.TWOxTWO) {
							highlighters[GridpieceController.TWOxTWO].transform.position = Vector3.Lerp(gridPositions[(int)highlightedPiece.x, (int)highlightedPiece.y], gridPositions[(int)highlightedPiece.x - 1, (int)highlightedPiece.y - 1], 0.5f);
							highlighters[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							highlighters[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
						}
						highlighted = true;
					}
					if (gpc.selected) {
						if (gpcSelectSize == GridpieceController.ONExONE) {
							selectors[GridpieceController.ONExONE].transform.position = gridPositions[i, j];
							selectors[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcSelectSize == GridpieceController.ONExTWO) {
							selectors[GridpieceController.ONExTWO].transform.position = Vector3.Lerp(gridPositions[gpc.dimX, gpc.dimY], gridPositions[gpc.dimX, gpc.dimY - 1], 0.5f);
							selectors[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcSelectSize == GridpieceController.TWOxONE) {
							selectors[GridpieceController.TWOxONE].transform.position = Vector3.Lerp(gridPositions[gpc.dimX, gpc.dimY], gridPositions[gpc.dimX - 1, gpc.dimY], 0.5f);
							selectors[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
						}
						else if (gpcSelectSize == GridpieceController.TWOxTWO) {
							selectors[GridpieceController.TWOxTWO].transform.position = Vector3.Lerp(gridPositions[gpc.dimX, gpc.dimY], gridPositions[gpc.dimX - 1, gpc.dimY - 1], 0.5f);
							selectors[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
							selectors[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
						}
						selectedPiece = true;
					}
				}
			}
		}

		if (!highlighted) {
			highlighters[0].transform.position = new Vector3(-10, 10, 0);
			highlighters[1].transform.position = new Vector3(-10, 10, 0);
			highlighters[2].transform.position = new Vector3(-10, 10, 0);
			highlighters[3].transform.position = new Vector3(-10, 10, 0);
		}
		if (!selectedPiece) {
			selectors[0].transform.position = new Vector3(-10, 10, 0);
			selectors[1].transform.position = new Vector3(-10, 10, 0);
			selectors[2].transform.position = new Vector3(-10, 10, 0);
			selectors[3].transform.position = new Vector3(-10, 10, 0);
		}

		// Check for matches
		if (lastPiece.x > -1) {
			if (Match((int)currentPiece.x, (int)currentPiece.y, (int)lastPiece.x, (int)lastPiece.y)) {
				// Debug.Log("THEY MATCH!");
                // CHANGED FOR TESTING
				// MovePiecesDown();
			}
			else {
				// Debug.Log("NO MATCH...");
			}
		}


	}

    // do the two blocks match?
    // TRAIL MATCHING DOES NOT ALWAYS WORK
    bool Match(int x1, int y1, int x2, int y2)
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
            if (gridObjects[x1, y1] && gridObjects[x2, y2] && gridObjects[x1, y1].GetComponent<GridpieceController>().type == gridObjects[x2, y2].GetComponent<GridpieceController>().type)
            {
                Vector2[] object1 = gridObjects[x1, y1].GetComponent<GridpieceController>().GetPositions();
                Vector2[] object2 = gridObjects[x2, y2].GetComponent<GridpieceController>().GetPositions();
                if (DEBUG)
                {
                    // Debug.Log("Now checking adjacency of " + x1 + ", " + y1 + " and " + x2 + ", " + y2);
                }
                // if the blocks are adjacent we are done and no more checking needs to happen
                for (int i = 0; i < object1.Length; i++)
                {
                    for (int j = 0; j < object2.Length; j++)
                    {
                        if (Adjacent((int)object1[i].x, (int)object1[i].y, (int)object2[j].x, (int)object2[j].y))
                        {
                            if (DEBUG)
                            {
                                Debug.Log("They are ADJACENT so we have a match!");
                            }
                            match = true;
                        }
                    }
                }
                if (!match)
                {
                    for (int i = 0; i < object1.Length; i++)
                    {
                        for (int j = 0; j < object2.Length; j++)
                        {
                            // then we check to see if there is a straight shot from A to B
                            if (StraightShot((int)object1[i].x, (int)object1[i].y, (int)object2[j].x, (int)object2[j].y))
                            {
                                if (DEBUG)
                                {
                                    Debug.Log("There is a STRAIGHTSHOT so we have a match!");
                                }
                                match = true;
                            }
                        }
                    }
                }
                if (!match)
                {
                    List<Vector2> list1 = new List<Vector2>();
                    List<Vector2> list2 = new List<Vector2>();
                    // create the match trails
                    for (int i = 0; i < object1.Length; i++)
                    {
                        list1.AddRange(CreateMatchTrail((int)object1[i].x, (int)object1[i].y));
                    }
                    for (int j = 0; j < object2.Length; j++)
                    {
                        list2.AddRange(CreateMatchTrail((int)object2[j].x, (int)object2[j].y));
                    }
                    if (DEBUG)
                    {
                        Debug.Log("These spaces are being processed...");
                        for (int i = 0; i < list1.Count; i++)
                        {
                            Debug.Log((int)list1[i].x + ", " + (int)list1[i].y);
                        }
                        Debug.Log("Against these spaces");
                        for (int i = 0; i < list2.Count; i++)
                        {
                            Debug.Log((int)list2[i].x + ", " + (int)list2[i].y);
                        }
                    }
                    match = CheckMatchTrails(list1, list2);
                    if (DEBUG && match)
                    {
                        Debug.Log("MATCHTRAILS line up so we have a match!");
                    }
                }
                if (match)
                {
                    // make sure we set the pieces to 0 and gray
                    // RIENZI: PRETTY SURE THE PROBLEM IS SOMEWHERE IN THE NEXT TWO FOR LOOPS
                    for (int i = 0; i < object1.Length; i++)
                    {
                        Debug.Log("Now turning this space from FIRST piece gray: " + (int)object1[i].x + ", " + (int)object1[i].y);
                        gridObjects[(int)object1[i].x, (int)object1[i].y].GetComponent<GridpieceController>().type = 0;
                        gridObjects[(int)object1[i].x, (int)object1[i].y].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                    // make sure we deselect and unhighlight last piece
                    gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().selected = false;
                    for (int i = 0; i < object2.Length; i++)
                    {
                        Debug.Log("Now turning this space from SECOND piece gray: " + (int)object2[i].x + ", " + (int)object2[i].y);
                        gridObjects[(int)object2[i].x, (int)object2[i].y].GetComponent<GridpieceController>().type = 0;
                        gridObjects[(int)object2[i].x, (int)object2[i].y].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                    // Make sure we reset current piece
                    currentPiece.x = -1;
                    currentPiece.y = -1;
                }
            }
        }
        // Reset the last piece so that we aren't checking for a match every frame
        lastPiece.x = -1;
        lastPiece.y = -1;
        MatchTrailCleanup();
        return match;
    }


    // are the two blocks adjacent?
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    bool Adjacent(int x1, int y1, int x2, int y2)
    {
        if ((Mathf.Abs(x1 - x2) == 1 && Mathf.Abs(y1 - y2) == 0) || (Mathf.Abs(x1 - x2) == 0 && Mathf.Abs(y1 - y2) == 1))
        {
            if (DEBUG)
            {
                // Debug.Log("Now comparing " + x1 + "; " + y1 + " and " + x2 + ", " + y2);
            }
            if (gridObjects[x1, y1] && gridObjects[x2, y2] && gridObjects[x1, y1].GetComponent<GridpieceController>().type == gridObjects[x2, y2].GetComponent<GridpieceController>().type)
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
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    List<Vector2> CreateMatchTrail(int x1, int y1)
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
                // Debug.Log ("Now checking " + x1 + ", " + i);
            }
            if (gridObjects[x1, i] && gridObjects[x1, i].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    // Debug.Log("Added a -1!");
                }
                gridObjects[x1, i].GetComponent<GridpieceController>().type = -1; 
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
                // Debug.Log ("Now checking " + x1 + ", " + i);
            }
            if (gridObjects[x1, i] && gridObjects[x1, i].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    // Debug.Log("Added a -1!");
                }
                gridObjects[x1, i].GetComponent<GridpieceController>().type = -1;
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
                // Debug.Log ("Now checking " + i + ", " + y1);
            }
            if (gridObjects[i, y1] && gridObjects[i, y1].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    // Debug.Log("Added a -1!");
                }
                gridObjects[i, y1].GetComponent<GridpieceController>().type = -1;
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
                // Debug.Log("Now checking " + x1 + ", " + i);
            }
            if (gridObjects[i, y1] && gridObjects[i, y1].GetComponent<GridpieceController>().type == 0)
            {
                if (DEBUG)
                {
                    // Debug.Log("Added a -1!");
                }
                gridObjects[i, y1].GetComponent<GridpieceController>().type = -1;
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
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    bool StraightShot(int x1, int y1, int x2, int y2)
    {
        bool output = false;
        if (x1 == x2)
        {
            if (y2 > y1)
            {
                for (int y = y1 + 1; y < y2; y++)
                {
                    if (gridObjects[x1, y] && gridObjects[x1, y].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int y = y2 + 1; y < y1; y++)
                {
                    if (gridObjects[x1, y] && gridObjects[x1, y].GetComponent<GridpieceController>().type != 0)
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
                    if (gridObjects[x, y1] && gridObjects[x, y1].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int x = x2 + 1; x < x2; x++)
                {
                    if (gridObjects[x, y1] && gridObjects[x, y1].GetComponent<GridpieceController>().type != 0)
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
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    bool CheckMatchTrails(List<Vector2> list1, List<Vector2> list2)
    {
        bool match = false;
        if (DEBUG)
        {
            // Debug.Log("Checking this list");
            for (int i = 0; i < list1.Count; i++)
            {
                // Debug.Log(list1[i].x + ", " + list1[i].y);
            }
            // Debug.Log("Against this one");
            for (int i = 0; i < list2.Count; i++)
            {
                // Debug.Log(list2[i].x + ", " + list2[i].y);
            }
        }
        // check straight shots for each list
        for (int i = 0; i < list1.Count; i++)
        {
            for (int j = 0; j < list2.Count; j++)
            {
                if (StraightShot((int)list1[i].x, (int)list1[i].y, (int)list2[j].x, (int)list2[j].y))
                {
                    return true;
                }
            }
        }
        return match;
    }

    // remove all -1's from the board
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    void MatchTrailCleanup()
    {
        for (int i = 0; i < gridSize.x + extraX; i++)
        {
            for (int j = 0; j < gridSize.y + extraY; j++)
            {
				if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().type == -1)
                {
                    gridObjects[i, j].GetComponent<GridpieceController>().type = 0;
                }
            }
        }
    }

    // Returns true if there are no empty blocks above this one in the playable game space.
	// Returns false otherwise
    // UNTESTED/UNUSED
    bool OnlyEmptySpaceAbovePiece(int x, int y)
    {
        bool onTop = true;
        for (int i = y + 1; i < gridSize.y; i++)
        {
            if (gridObjects[x, i] && gridObjects[x, i].GetComponent<GridpieceController>().type != 0)
            {
                onTop = false;
                return onTop;
            }
        }
        return onTop;
    }

    // Move all blocks that can be moved down as far as possible
    // Keep track of which blocks moved so that we can combo later
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    void MovePiecesDown()
    {
		if (DEBUG){
			Debug.Log("-------------------------- Moving Pieces Down ----------------------------");
        }
        // First remove all the grey pieces in the playable grid
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().type == 0){
                    // first get rid of the piece
                    RemovePieceAtPosition(i, j);
                }
            }
        }
		// Next, move pieces down.  I'm currently not accounting for any pieces larger than 1x1
		// Iterate through the grid
		for (int i = 1; i <= gridSize.x; i++) 
        {
			for (int j = 0; j < gridSize.y; j++) 
            {
				// If you come across a gap in the grid that isn't in the top row, bring the objects down
                for (int k = (int)gridSize.y - 1; k >= j; k--)
                {
                    // if we move the object it has to be added to the movedObjects array
                    if (gridObjects[i, k] != null && (k - 1) >= 0 && gridObjects[i, k - 1] == null) 
                    {
                        movedObjects.Add(gridObjects[i, k]);
                        MovePieceToPosition(gridObjects[i, k], i, k - 1);
                    }
                }
			}
		}
        movedObjects = movedObjects.Distinct().ToList();
        if (DEBUG)
        {
            Debug.Log(movedObjects.Count + " game pieces just moved");
        }
		// Finally, fill the board back up with grey pieces
		for (int i = 1; i <= gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				if (gridObjects[i, j] == null)
					AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
			}
		}
        // ProcessCombos();
    }

    // Process the combos
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    public void ProcessCombos()
    {
        Debug.Log("How many items are in movedObjects?: " + movedObjects.Count);
        // first move items from movedObjects to objectsToProcess
        for (int i = 0; i < movedObjects.Count; i++)
        {
            objectsToProcess.Add(movedObjects[i]);
        }
        movedObjects.Clear();
        Debug.Log("How many items are in objectsToProcess?: " + objectsToProcess.Count);
        int x = -1;
        int y = -1;
        int type = -1;
        bool combo = false;
        bool anyCombo = false;
        // check adjacency of each item in the list of objects to process
        // tileA can only combo with tileB if exactly one tile moved
        // BBB
        // O!O
        // BOB
        // if the exclamation mark above is an O that moved and the O's above are O's that did not move
        // the ! can combo with all of the O's above
        for (int i = 0; i < objectsToProcess.Count; i++)
        {
            x = objectsToProcess[i].GetComponent<GridpieceController>().dimX;
            y = objectsToProcess[i].GetComponent<GridpieceController>().dimY;
            type = objectsToProcess[i].GetComponent<GridpieceController>().type;
            combo = false;
            Debug.Log("Now processing a block of type " + type + " at coordinates " + x + ", " + y);
            // check left
            if (gridObjects[x - 1, y] && gridObjects[x - 1, y].GetComponent<GridpieceController>().type == type)
            {
                Debug.Log("Checking " + (x - 1) + ", " + y + " and " + x + ", " + y);
                // it can only be a combo if the block to the left is not in the list
                if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().dimX == (x - 1) && tile.GetComponent<GridpieceController>().dimY == y))
                {
                    Debug.Log("THESE TWO MATCH! " + (x - 1) + ", " + y + " and " + x + ", " + y);
                    combo = true;
                    gridObjects[x - 1, y].GetComponent<GridpieceController>().type = 0;
                    gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color = Color.gray;
                }
            }
            // check right
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().type == type)
            {
                Debug.Log("Checking " + (x + 1) + ", " + y + " and " + x + ", " + y);
                // it can only be a combo if the block to the right is not in the list
                if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().dimX == (x + 1) && tile.GetComponent<GridpieceController>().dimY == y))
                {
                    Debug.Log("THESE TWO MATCH! " + (x + 1) + ", " + y + " and " + x + ", " + y);
                    combo = true;
                    gridObjects[x + 1, y].GetComponent<GridpieceController>().type = 0;
                    gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color = Color.gray;
                }
            }
            // check down
            if (y > 0)
            {
                if (gridObjects[x, y - 1] && gridObjects[x, y - 1].GetComponent<GridpieceController>().type == type)
                {
                    Debug.Log("Checking " + x + ", " + (y - 1) + " and " + x + ", " + y);
                    // it can only be a combo if the block to the down is not in the list
                    if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().dimX == x && tile.GetComponent<GridpieceController>().dimY == (y - 1)))
                    {
                        Debug.Log("THESE TWO MATCH! " + x + ", " + (y - 1) + " and " + x + ", " + y);
                        combo = true;
                        gridObjects[x, y - 1].GetComponent<GridpieceController>().type = 0;
                        gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                }
            }
            // if any of the blocks comboed with the main block remove it too
            if (combo)
            {
                anyCombo = true;
                gridObjects[x, y].GetComponent<GridpieceController>().type = 0;
                gridObjects[x, y].GetComponent<GridpieceController>().sr.color = Color.gray;
            }
        }
        objectsToProcess.Clear();
        if (anyCombo)
        {
            // MovePiecesDown();
        }
    }

    // checks each grid position to see if there is a blank block there
    // NO KNOWN BUGS
    void CheckPieces()
    {
        int count = 0;
        for (int i = 0; i < gridSize.x + extraX; i++) 
        {
            for (int j = 0; j < gridSize.y + extraY; j++) 
            {
                if (!gridObjects[i, j])
                {
                    Debug.Log("Missing a piece at " + i + ", " + j);
                }
                else
                {
                    count++;
                }
            }
        }
        Debug.Log(count + " valid pieces on the game board right now");
    }

    // Adds a new row to bottom
    // NO KNOWN BUGS
	// - NOT UPDATED FOR MULTIPLE SIZES
    void AddRow()
    {
        // DELETE TOP ROW
        for (int i = 1; i <= gridSize.x; i++)
        {
            RemovePieceAtPosition(i, (int)gridSize.y);
        }
        // MOVE ALL PIECES UP
        for (int i = 1; i <= gridSize.x; i++) 
        {
            for (int j = (int)(gridSize.y) - 1; j >= 0; j--) 
            {
                MovePieceToPosition(gridObjects[i, j], i, j + 1);
            }
        }
        // ADD BOTTOM ROW
        for (int i = 1; i <= gridSize.x; i++)
        {
			AddPieceAtPosition(i, 0, -1, GridpieceController.ONExONE);
        }
        // CHECK TO SEE IF A PIECE HAS MOVED ABOVE THE TOP ROW. IF IT HAS, GAME IS OVER
        for (int i = 1; i <= gridSize.x; i++)
        {
            if (gridObjects[i, (int)gridSize.y] && gridObjects[i, (int)gridSize.y].GetComponent<GridpieceController>().type != 0)
            {
                Debug.Log("There is a game piece at " + i + ", " + (int)gridSize.y + ". You lose!!");
            }
        }
    }

    // Removes row of highlighted piece
    // NO KNOWN BUGS
    void RemoveRow()
    {
        // turn all objects in the row gray and empty
        for (int x = 1; x <= gridSize.x; x++)
        {
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
    }

    // Removes column of highlighted piece
    // NO KNOWN BUGS
    void RemoveColumn()
    {
        // turn all objects in the row gray and empty
        for (int y = 0; y < gridSize.y; y++)
        {
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
    }

    // Removes both row and column of highlighted piece
    // NO KNOWN BUGS
    void RemoveRowAndColumn()
    {
        // turn all objects in the row gray and empty
        for (int x = 1; x <= gridSize.x; x++)
        {
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
        // turn all objects in the row gray and empty
        for (int y = 0; y < gridSize.y; y++)
        {
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
    }

    // Remove all blocks of currentPiece color 
    // WILL NEED TO BE CHANGED TO BLOCKS OF LASTPIECE LATER
    // NO KNOWN BUGS
    void RemoveOneColor()
    {
        int color = gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().type;
        for (int i = 1; i <= gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                if (color == gridObjects[i, j].GetComponent<GridpieceController>().type)
                {
                    gridObjects[i, j].GetComponent<GridpieceController>().type = 0;
                    gridObjects[i, j].GetComponent<GridpieceController>().sr.color = Color.gray;
                }
            }
        }
    }

	// Removes any piece - UPDATED FOR MULTIPLE SIZES
	public void RemovePieceAtPosition(int x, int y) {
		if (DEBUG)
			Debug.Log("Removing block at space " + x + ", " + y);
		if (gridObjects[x, y]) {
			GridpieceController gpc = gridObjects[x, y].GetComponent<GridpieceController>();
			int xPos = gpc.dimX;
			int yPos = gpc.dimY;
			if (gpc.size == GridpieceController.ONExONE) {
				GameObject.Destroy(gridObjects[xPos, yPos]);
				gridObjects[xPos, yPos] = null;
			}
			else if (gpc.size == GridpieceController.ONExTWO) {
				GameObject.Destroy(gridObjects[xPos, yPos]);
				gridObjects[xPos, yPos] = null;
				gridObjects[xPos, yPos - 1] = null;
			}
			else if (gpc.size == GridpieceController.TWOxONE) {
				GameObject.Destroy(gridObjects[xPos, yPos]);
				gridObjects[xPos, yPos] = null;
				gridObjects[xPos - 1, yPos] = null;
			}
			else if (gpc.size == GridpieceController.TWOxTWO) {
				GameObject.Destroy(gridObjects[x, y]);
				gridObjects[xPos, yPos] = null;
				gridObjects[xPos, yPos - 1] = null;
				gridObjects[xPos - 1, yPos] = null;
				gridObjects[xPos - 1, yPos - 1] = null;
			}
		}
	}

    // Add a piece - UPDATED FOR MULTIPLE SIZES
    // - if the num supplied is negative we choose a number randomly
    // - otherwise the type is the num we supplied
	// - if size supplied is negative, we choose a size randomy
	// - otherwise the size is the size we supplied
	public GameObject AddPieceAtPosition(int x, int y, int num, int size) {
		GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[x, y], Quaternion.identity);
		GridpieceController gpc = go.GetComponent<GridpieceController>();
        if (num < 0)
        {
			gpc.type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
        }
        else
        {
            gpc.type = num;
        }
		gpc.SetColor();
		if (size < 0) {
			if (x > 1 && y > 0) {
				if (!gridObjects[x, y - 1])
					gpc.size = (int)Mathf.Floor(Random.Range(0, 3.99999999f));
				else {
					if (Random.value > 0.5f)
						gpc.size = GridpieceController.TWOxONE;
					else
						gpc.size = GridpieceController.ONExONE;
				}	
			}
			else if (x == 1) {
				if (!gridObjects[x, y - 1]) {
					if (Random.value > 0.5f)
						gpc.size = GridpieceController.ONExTWO;
					else
						gpc.size = GridpieceController.ONExONE;
				}
				else 
					gpc.size = GridpieceController.ONExONE;
			}
			else
				gpc.size = GridpieceController.ONExONE;
		}
		else
			gpc.size = size;
        
		gpc.dimX = x;
		gpc.dimY = y;
        gpc.selected = false;
		gridObjects[x, y] = go;

		if (gpc.size == GridpieceController.ONExTWO) {
			gridObjects[x, y - 1] = go;
			go.transform.position = Vector3.Lerp(gridPositions[x, y], gridPositions[x, y - 1], 0.5f);
		}
		else if (gpc.size == GridpieceController.TWOxONE) {
			gridObjects[x - 1, y] = go;
			go.transform.position = Vector3.Lerp(gridPositions[x, y], gridPositions[x - 1, y], 0.5f);
		}
		else if (gpc.size == GridpieceController.TWOxTWO) {
			gridObjects[x - 1, y] = go;
			gridObjects[x, y - 1] = go;
			gridObjects[x - 1, y - 1] = go;
			go.transform.position = Vector3.Lerp(gridPositions[x, y], gridPositions[x - 1, y - 1], 0.5f);
		}
		//if (DEBUG)
		//	Debug.Log("Created Piece at position: " + x + ", " + y);
		return go;
	}

	// NOT UPDATED FOR MULTIPLE SIZES
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
			//piece.transform.position = gridPositions[x, y];
			// Can swap the line above for the line below in order to create a moving piece
			StartCoroutine(MovePiece(piece, x, y));
		}
	}

	public IEnumerator MovePiece(GameObject piece, int x, int y) {
		float timeForPieceToMove = 0.5f;
		Vector3 startPos = piece.transform.position;
		for (float i = 0; i <= timeForPieceToMove; i += Time.deltaTime) {
			float counter = i / timeForPieceToMove;
			piece.transform.position = Vector3.Lerp(startPos, gridPositions[x, y], counter);
			yield return null;
		}
		piece.transform.position = gridPositions[x, y];
	}

	public void ResetCurrentLastPieces() {
		currentPiece = Vector2.one * -1;
		lastPiece = Vector2.one * -1;
	}
}
