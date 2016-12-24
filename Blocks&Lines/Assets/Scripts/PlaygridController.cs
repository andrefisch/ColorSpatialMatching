﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlaygridController : MonoBehaviour {

    private bool DEBUG = true;
    public GameObject gridPiece;
    // 1x1 is 0, 1x2 is 1, 2x1 is 2, 2x2 is 3
    public GameObject[] highlighters;
    public GameObject[] selectors;

    // SCORE STUFF
    public int colorLevel;
    public int shapeLevel;
    public int score;
    public int combos;
    public int comboScore;
    public int matchesMade;
    public int blocksDestroyed;

    // HOW MANY EXTRA ROWS AND COLUMNS THERE ARE
    public const int extraX = 2;
    public const int extraY = 2;

    public Vector2 gridSize;
    public Vector2 gridSpacing;

    public bool includeBigPieces;

    // COUNTER TO KEEP TRACK OF WHEN THE BLOCKS FALL
    private bool startCounting;
    private int counter;
    private int timingInterval;

    //public GridpieceController gpc;
    private Vector2 currentPiece;
    private Vector2 lastPiece;

    // This is how you do a 2D array in C# ---  int[][] is an array of arrays
    public GameObject[,] gridObjects;

    // Keep track of which objects fell down for combo checking
    public List<GameObject> movedObjects;
    public List<GameObject> objectsToProcess;


    private Vector3[,] gridPositions;

    private bool removePiece;
    private bool addPiece;

    //  Thos os tp help keep track of the highlighted piece's position so that it doesn't get messed up if you don't happen to move the mouse off a piece between frames 
    private Vector2 highlightedPiece;

    // Use this for initialization
    void Start (){
        // includeBigPieces = false;
        // SCORE STUFF
        colorLevel = 4;
        shapeLevel = 4;
        score = 0;
        combos = 1;
        comboScore = 0;
        matchesMade = 0;
        blocksDestroyed = 0;

        startCounting = false;
        counter = 0;
        timingInterval = 20;

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

		// Set up the edge pieces
		// Go from 0 to size on the left and right edges
		for (int i = 1; i < gridSize.y+1; i++) {
			AddPieceAtPosition(0, i, 0, GridpieceController.ONExONE);
			AddPieceAtPosition((int)gridSize.x + extraX - 1, i, 0, GridpieceController.ONExONE);
		}
		// Go from 0 to size on the top and bottom
		for (int i = 0; i < gridSize.x + extraX; i++) {
			AddPieceAtPosition(i, (int)gridSize.y + extraY - 1, 0, GridpieceController.ONExONE);
			AddPieceAtPosition(i, 0, 0, GridpieceController.ONExONE);
		}

        // Set up the actual pieces
        for (int i = (int)gridSize.x; i >= 1; i--) {
            for (int j = (int)gridSize.y; j >= 1; j--) {
                if (!gridObjects[i, j]) {
                    if (i >= 1 && j > 0 && includeBigPieces){
                        AddPieceAtPosition(i, j, -1, -1);
                    } else {
                        AddPieceAtPosition(i, j, -1, GridpieceController.ONExONE);
                    }
                }
            }
        }
    }

    // Update at a fixed interval so we can count accurately
    // NO KNOWN BUGS
    void FixedUpdate()
    {
        if (startCounting)
        {
            counter++;
        }
        if (movedObjects.Count == 0 && counter >= timingInterval)
        {
            Debug.Log("Count at time of piece moving is: " + counter);
            MovePiecesDown();
        }
        else if (movedObjects.Count > 0 && counter >= timingInterval)
        {
            Debug.Log("Count at time of combo processing is: " + counter);
            ProcessCombos();
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
        // start the counting clock
        if (Input.GetKeyDown("s")) {
            startCounting = true;
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
            if (gpc.type != 0 && (gpc.dimX >= 1 && gpc.dimX <= gridSize.x && gpc.dimY >= 0 && gpc.dimY <= gridSize.y)) {
                highlightedPiece.x = gpc.dimX;
                highlightedPiece.y = gpc.dimY;
            }

            if (Input.GetMouseButtonDown(0)) {
                // First deselect any selected piece
                for (int i = 1; i <= gridSize.x; i++) {
                    for (int j = 1; j <= gridSize.y; j++) {
                        if (gridObjects[i,j]){
                            gridObjects[i,j].GetComponent<GridpieceController>().selected = false;
                        }
                    }
                }
                // Select only pieces that aren't 0 and are in the playable grid
                if (gpc.type != 0 && (gpc.dimX >= 1 && gpc.dimX <= gridSize.x && gpc.dimY >= 1 && gpc.dimY <= gridSize.y)) {

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
                    for (int j = 1; j <= gridSize.y; j++)
                        if (gridObjects[i, j])
                            gridObjects[i, j].GetComponent<GridpieceController>().selected = false;
                }
                gpcSelectSize = -1;
            }
        }

        // Move the highlighted and selected indicator to their proper positions.
        bool highlighted = false;
        bool selectedPiece = false;
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 1; j <= gridSize.y; j++) {
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
                // startCounting = true;
                // MovePiecesDown();
            }
            else {
                // Debug.Log("NO MATCH...");
            }
        }


    }

    // Score the block based on size
    // NO KNOWN BUGS
    int ScoreBlock(int x, int y)
    {
        GridpieceController piece = gridObjects[x, y].GetComponent<GridpieceController>();
        int toAdd = 0;
        // Pieces are worth exactly how many gridspaces they take up
        // So small and big pieces are worth 1 and 4 respectively
        if (piece.size == 0 || piece.size == 3)
        {
            toAdd = piece.size + 1;
        }
        // Long pieces are always worth 2
        else
        {
            toAdd = 2;
        }
        // Obviously add the score
        score += (toAdd * combos);
        // Increment blocksDestroyed variable
        blocksDestroyed++;
        return toAdd;
    }

    // Do the two blocks match?
    // NO KNOWN BUGS
    bool Match(int x1, int y1, int x2, int y2)
    {
        if (DEBUG)
        {
            Debug.Log("----------------------------Running Matching Algorithm-----------------------------------");
        }
        bool match = false;
        // Make sure the pieces are all valid pieces
        if (x1 >= 0 && x1 <= gridSize.x + extraX - 1 &&
            x2 >= 0 && x2 <= gridSize.x + extraX - 1 &&
            y1 >= 0 && y1 <= gridSize.y + extraY - 1 &&
            y2 >= 0 && y2 <= gridSize.y + extraY - 1)
        {
            // If the blocks arent the same type no more checking needs to be done
            if (gridObjects[x1, y1] && gridObjects[x2, y2] && gridObjects[x1, y1].GetComponent<GridpieceController>().type == gridObjects[x2, y2].GetComponent<GridpieceController>().type)
            {
                Vector2[] object1 = gridObjects[x1, y1].GetComponent<GridpieceController>().GetPositions();
                Vector2[] object2 = gridObjects[x2, y2].GetComponent<GridpieceController>().GetPositions();
                if (DEBUG)
                {
                    // Debug.Log("Now checking adjacency of " + x1 + ", " + y1 + " and " + x2 + ", " + y2);
                }
                // If the blocks are adjacent we are done and no more checking needs to happen
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
                            // Then we check to see if there is a straight shot from A to B
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
                    // Create the match trails
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
                    // Score the blocks
                    int first  = ScoreBlock((int)object1[0].x, (int)object1[0].y);
                    int second = ScoreBlock((int)object2[0].x, (int)object2[0].y);
                    // Here is where we would display do userfeedback stuff with the block
                    // DisplayScore(first + second) 
                    Debug.Log("MATCH WORTH " + (first + second));
                    matchesMade++;
                    // Make sure we set the pieces to 0 and gray
                    // for (int i = object1.Length - 1; i >= 0; i--)
                    for (int i = 0; i < object1.Length; i++)
                    {
                        Debug.Log("Now removing 1st piece at " + (int)object1[i].x + ", " + (int)object1[i].y);
                        if (i == 0)
                        {
                            RemovePieceAtPosition((int)object1[i].x, (int)object1[i].y);
                        }
                        AddPieceAtPosition((int)object1[i].x, (int)object1[i].y, 0, GridpieceController.ONExONE);
                        // gridObjects[(int)object1[i].x, (int)object1[i].y].GetComponent<GridpieceController>().type = 0;
                        // gridObjects[(int)object1[i].x, (int)object1[i].y].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                    // Make sure we deselect and unhighlight last piece
                    gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().selected = false;
                    // for (int i = object2.Length - 1; i >= 0; i--)
                    for (int i = 0; i < object2.Length; i++)
                    {
                        Debug.Log("Now removing 2nd piece " + (int)object2[i].x + ", " + (int)object2[i].y);
                        if (i == 0)
                        {
                            RemovePieceAtPosition((int)object2[i].x, (int)object2[i].y);
                        }
                        AddPieceAtPosition((int)object2[i].x, (int)object2[i].y, 0, GridpieceController.ONExONE);
                        // gridObjects[(int)object2[i].x, (int)object2[i].y].GetComponent<GridpieceController>().type = 0;
                        // gridObjects[(int)object2[i].x, (int)object2[i].y].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                    // Make sure we reset current piece
                    currentPiece.x = -1;
                    currentPiece.y = -1;
                    // Start the clock
                    startCounting = true;
                }
            }
        }
        // Reset the last piece so that we aren't checking for a match every frame
        lastPiece.x = -1;
        lastPiece.y = -1;
        MatchTrailCleanup();
        return match;
    }


    // Are the two blocks adjacent?
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
    // Creates a trail of -1's from all possible directions of input block
    // NO KNOWN BUGS
    // - NOT UPDATED FOR MULTIPLE SIZES
    List<Vector2> CreateMatchTrail(int x1, int y1)
    {
        List<Vector2> output = new List<Vector2>();
        // Create a trail in up to 4 possible directions for both blocks
        // Check down only if we are not at the bottom row
        if (DEBUG)
        { 
            Debug.Log ("now checking down from " + x1 + ", " + y1); 
        }
        if (y1 > 1)
        {
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
        }
        // Check up
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
        // Check right
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
        // Check left
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
        // If the output list is empty we may have a 90* match so add coordinates of tile for checking
        if (output.Count == 0)
        {
            output.Add(new Vector2(x1, y1));
        }
        return output;
    }

    // Checks to see if there is anything but 0's between the two spots
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
                for (int x = x2 + 1; x < x1; x++)
                {
                    if (gridObjects[x, y1] && gridObjects[x, y1].GetComponent<GridpieceController>().type != 0)
                    {
                        return output;
                    }
                }
            }
            Debug.Log("There is a HORIZONTAL STRAIGHTSHOT");
            output = true;
        }
        return output;
    }


    // Checks to see if there is a straightShot between any -1 in list1 and list2
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
        // Check straight shots for each list
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

    // Remove all -1's from the board
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

    // Move all blocks that can be moved down as far as possible
    // Keep track of which blocks moved so that we can combo later
    // NO KNOWN BUGS
    // RIENZI: 
    // !! I AM HAVING IT MOVE THE PIECES DOWN THREE TIMES TO COMPENSATE FOR A RARE BUG
    // !! A BETTER WAY TO DO IT WOULD BE A WHILE LOOP THAT REPEATS UNTIL NO NEW BLOCKS
    // !! MOVE BUT I HAD LIMITED TIME AND THIS WAS EASIER
    void MovePiecesDown()
    {
        if (DEBUG){
            Debug.Log("-------------------------- Moving Pieces Down ----------------------------");
        }
        //movedObjects.Clear();
        // First remove all the grey pieces in the playable grid
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 1; j <= gridSize.y; j++) {
                if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().sr.color == Color.gray){
                    // First get rid of the piece
                    RemovePieceAtPosition(i, j);
                }
            }
        }
        // How many pieces moved since last time
        // int numPiecesMoved = 0;
        // Next, move pieces down
        // Iterate through the grid
        for (int z = 0; z < 3; z++)
        {
            for (int i = 1; i <= gridSize.x; i++) 
            {
                // We start a y = 2 because you can't move down if y = 1
                for (int j = 2; j <= gridSize.y; j++) 
                {
                    if (gridObjects[i, j] != null) {
                        //Debug.Log("Found Piece at: " + i + ", " + j);
                        GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
                        int pieceSize = gpc.size;
                        int moveDown = 0;
                        if (pieceSize == GridpieceController.ONExONE || pieceSize == GridpieceController.ONExTWO) {
                            while (j - 1 - moveDown >= 1 && !gridObjects[i, j - 1 - moveDown])
                                moveDown++;
                            if (moveDown > 0) {
                                if (pieceSize == GridpieceController.ONExONE) {
                                    MovePieceToPosition(gridObjects[i, j], i, j - moveDown);
                                    movedObjects.Add(gridObjects[i, j - moveDown]);
                                }
                                else {
                                    MovePieceToPosition(gridObjects[i, j], i, (j - moveDown) + 1);
                                    movedObjects.Add(gridObjects[i, (j - moveDown) + 1]);
                                }
                            }
                        }
                        else {
                            if (i == gpc.dimX ) {
                                while (j - 1 - moveDown >= 1 && !gridObjects[i, j - 1 - moveDown] && !gridObjects[i - 1, j - 1 - moveDown])
                                    moveDown++;
                                if (moveDown > 0) {
                                    if (pieceSize == GridpieceController.TWOxONE) {
                                        MovePieceToPosition(gridObjects[i, j], i, j - moveDown);
                                        movedObjects.Add(gridObjects[i, j - moveDown]);
                                    }
                                    else {
                                        MovePieceToPosition(gridObjects[i, j], i, (j - moveDown) + 1);
                                        movedObjects.Add(gridObjects[i, (j - moveDown) + 1]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            movedObjects = movedObjects.Distinct().ToList();
            if (DEBUG)
            {
                Debug.Log(movedObjects.Count + " game pieces moved in this move");
            }
        }
        if (DEBUG)
        {
            Debug.Log(movedObjects.Count + " game pieces just moved");
        }
        // Finally, fill the board back up with grey pieces
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 1; j <= gridSize.y; j++) {
                if (gridObjects[i, j] == null){
                    AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                }
            }
        }
        counter = 0;
        if (movedObjects.Count == 0)
        {
            startCounting = false;
        }
        // ProcessCombos();
    }

    // Check if the given block is the same type as the block in the given direction
    // Directions:
    // 0 is left
    // 1 is right
    // 2 is down
    public bool CheckDirection(int x, int y, int direction)
    {
        int type = gridObjects[x, y].GetComponent<GridpieceController>().type;
        bool combo = false;
        Debug.Log("Now processing a block of type " + type + " at coordinates " + x + ", " + y);
        // Check left
        if (direction == 0)
        {
            Debug.Log("LEFT check: " + (x - 1) + ", " + y + " and " + x + ", " + y);
            if (gridObjects[x - 1, y] && gridObjects[x - 1, y].GetComponent<GridpieceController>().type == type)
            {
                // It can only be a combo if the block to the left is not in the list
                int id = gridObjects[x - 1, y].GetComponent<GridpieceController>().blockId;
                if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().blockId == id))
                {
                    Debug.Log("THESE TWO MATCH! " + (x - 1) + ", " + y + " and " + x + ", " + y);
                    combo = true;
                    comboScore += ScoreBlock(x - 1, y);
                    gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color = Color.gray;
                }
            }
        }
        // Check right
        else if (direction == 1)
        {
            Debug.Log("RIGHT check: " + (x + 1) + ", " + y + " and " + x + ", " + y);
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().type == type)
            {
                // It can only be a combo if the block to the left is not in the list
                int id = gridObjects[x + 1, y].GetComponent<GridpieceController>().blockId;
                if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().blockId == id))
                {
                    Debug.Log("THESE TWO MATCH! " + (x + 1) + ", " + y + " and " + x + ", " + y);
                    combo = true;
                    comboScore += ScoreBlock(x + 1, y);
                    gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color = Color.gray;
                }
            }
        }
        // Check down
        else if (direction == 2)
        {
            if (y > 1)
            {
                Debug.Log("LEFT check: " + x + ", " + (y - 1) + " and " + x + ", " + y);
                if (gridObjects[x, y - 1] && gridObjects[x, y - 1].GetComponent<GridpieceController>().type == type)
                {
                    Debug.Log("Checking " + x + ", " + (y - 1) + " and " + x + ", " + y);
                    // It can only be a combo if the block to the left is not in the list
                    int id = gridObjects[x, y - 1].GetComponent<GridpieceController>().blockId;
                    if (!objectsToProcess.Exists(tile => tile.GetComponent<GridpieceController>().blockId == id))
                    {
                        Debug.Log("THESE TWO MATCH! " + x + ", " + (y - 1) + " and " + x + ", " + y);
                        combo = true;
                        comboScore += ScoreBlock(x, y - 1);
                        gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color = Color.gray;
                    }
                }
            }
        }
        return combo;
    }

    // Process the combos
    // NO KNOWN BUGS
    public void ProcessCombos()
    {
        Debug.Log("How many items are in movedObjects?: " + movedObjects.Count);
        // First move items from movedObjects to objectsToProcess
        objectsToProcess.Clear();
        for (int i = 0; i < movedObjects.Count; i++)
        {
            objectsToProcess.Add(movedObjects[i]);
        }
        movedObjects.Clear();
        Debug.Log("How many items are in objectsToProcess?: " + objectsToProcess.Count);
        bool combo = false;
        bool anyCombo = false;
        // Check adjacency of each item in the list of objects to process
        // tileA can only combo with tileB if exactly one tile moved
        // BBB
        // O!O
        // BOB
        // If the exclamation mark above is an O that moved and the O's above are O's that did not move
        // then the ! can combo with all of the O's above
        for (int i = 0; i < objectsToProcess.Count; i++)
        {
            // Reset comboScore each loop
            comboScore = 0;
            int x = objectsToProcess[i].GetComponent<GridpieceController>().dimX;
            int y = objectsToProcess[i].GetComponent<GridpieceController>().dimY;
            int size = objectsToProcess[i].GetComponent<GridpieceController>().size;
            combo = false;
            if (size == GridpieceController.ONExONE)
            {
                combo = CheckDirection(x, y, 0) | // left
                        CheckDirection(x, y, 1) | // right
                        CheckDirection(x, y, 2);   // down
            }
            else if (size == GridpieceController.ONExTWO)
            {
                Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 0) | // left from top
                        CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from top
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 0) | // left from bottom
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 1) | // right from bottom
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 2);  // down from bottom
            }
            else if (size == GridpieceController.TWOxONE)
            {
                Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from right
                        CheckDirection((int)locations[0].x, (int)locations[0].y, 2) | // down from right
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 0) | // left from left
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 2);  // down from left
            }
            else if (size == GridpieceController.TWOxTWO)
            {
                Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from upright
                        CheckDirection((int)locations[2].x, (int)locations[2].y, 1) | // right from downright
                        CheckDirection((int)locations[2].x, (int)locations[2].y, 2) | // down from downright
                        CheckDirection((int)locations[3].x, (int)locations[3].y, 2) | // down from downleft
                        CheckDirection((int)locations[3].x, (int)locations[3].y, 0) | // left from downleft
                        CheckDirection((int)locations[1].x, (int)locations[1].y, 0);  // left from upleft
            }
            // If any of the blocks comboed with the main block remove it too
            if (combo)
            {
                anyCombo = true;
                // gridObjects[x, y].GetComponent<GridpieceController>().type = 0;
                int val = ScoreBlock(x, y);
                gridObjects[x, y].GetComponent<GridpieceController>().sr.color = Color.gray;
                // Here is where we will do userfeedback stuff for combo
                // DisplayComboScore(val)
                Debug.Log("COMBO WORTH " + (comboScore + val));
            }
        }
        objectsToProcess.Clear();
        if (anyCombo)
        {
            // Refresh the board
            // Remove all the grey pieces in the playable grid
            for (int i = 1; i <= gridSize.x; i++) 
            {
                for (int j = 1; j <= gridSize.y; j++) 
                {
                    if (gridObjects[i,j] && gridObjects[i,j].GetComponent<GridpieceController>().sr.color == Color.gray)
                    {
                        // First get rid of the piece
                        RemovePieceAtPosition(i, j);
                    }
                }
            }
            // Finally, fill the board back up with grey pieces
            for (int i = 1; i <= gridSize.x; i++) 
            {
                for (int j = 1; j <= gridSize.y; j++) 
                {
                    if (gridObjects[i, j] == null)
                    {
                        AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                    }
                }
            }
            // Increment combos and move pieces down
            combos++;
            counter = 0;
            // MovePiecesDown();
        }
    }

    // Checks each grid position to see if there is a blank block there
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
        for (int i = (int)(gridSize.x); i >= 1; i--) 
        {
            for (int j = (int)(gridSize.y); j >= 1; j--) 
            {
                MovePieceToPosition(gridObjects[i, j], i, j + 1);
            }
        }
        // ADD BOTTOM ROW
        for (int i = 1; i <= gridSize.x; i++)
        {
            AddPieceAtPosition(i, 1, -1, -1);
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
        // Turn all objects in the row gray and empty
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
        // Turn all objects in the row gray and empty
        for (int y = 1; y <= gridSize.y; y++)
        {
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)currentPiece.x, y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
    }

    // Removes both row and column of highlighted piece
    // NO KNOWN BUGS
    void RemoveRowAndColumn()
    {
        // Turn all objects in the row gray and empty
        for (int x = 1; x <= gridSize.x; x++)
        {
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
        // Turn all objects in the row gray and empty
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
            for (int j = 1; j <= gridSize.y; j++)
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
        //if (DEBUG)
        // Debug.Log("Removing block at space " + x + ", " + y);
		if (x < 0 || y < 0)
			Debug.Log("Warning (RemovePieceAtPosition): Attempting to remove a block outside the playgrid - nothing done");
        else if (gridObjects[x, y]) {
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
				gridObjects[xPos - 1, yPos] = null;
				gridObjects[xPos, yPos - 1] = null;
				gridObjects[xPos - 1, yPos - 1] = null;
				
            }
        }
        else if (DEBUG)
			Debug.Log("Warning (RemovePieceAtPosition): Attempting to remove a block that isn't there");
    }

    // Add a piece - UPDATED FOR MULTIPLE SIZES
    // - if the num supplied is negative we choose a number randomly
    // - otherwise the type is the num we supplied
    // - if size supplied is negative, we choose a size randomly based on what can fit in the space provided
    // - otherwise the size is the size we supplied
    // - Gives warning and does nothing if the block specified can't fit in the space provided
    public GameObject AddPieceAtPosition(int x, int y, int num, int size) {
        if (size == GridpieceController.TWOxTWO && (gridObjects[x, y] || gridObjects[x - 1, y] || gridObjects[x, y - 1] || gridObjects[x - 1, y - 1])) {
            if (DEBUG)
                Debug.Log("Warning (AddPieceAtPosition):  Attempting to add a 2x2 block in a place where it won't fit - nothing done");
            return null;
        }
        else if (size == GridpieceController.ONExTWO && (gridObjects[x, y] || gridObjects[x, y - 1])) {
            if (DEBUG)
                Debug.Log("Warning (AddPieceAtPosition):  Attempting to add a 1x2 block in a place where it won't fit - nothing done");
            return null;
        }
        else if (size == GridpieceController.TWOxONE && (gridObjects[x, y] || gridObjects[x - 1, y])) {
            if (DEBUG)
                Debug.Log("Warning (AddPieceAtPosition):  Attempting to add a 2x1 block in a place where it won't fit - nothing done");
            return null;
        }
        else if ((size == GridpieceController.ONExONE || size < 0) && (gridObjects[x, y])) {
            if (DEBUG)
                Debug.Log("Warning (AddPieceAtPosition):  Attempting to add a 1x1 or random block in a place where it won't fit - nothing done");
            return null;
        }
        else {
            GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[x, y], Quaternion.identity);
            GridpieceController gpc = go.GetComponent<GridpieceController>();
            if (num < 0) {
                gpc.type = (int)Mathf.Floor(Random.Range(1, (1 + colorLevel - 0.00000001f)));
            }
            else {
                gpc.type = num;
            }
            gpc.SetColor();
            if (size < 0) {
                if (x > 1 && y > 1) {
                    if (!gridObjects[x, y - 1])
                    {
                        if (Random.Range(0, 7.999999999f) > 7)
                        {
                            gpc.size = (int)Mathf.Floor(Random.Range(0, (shapeLevel - 0.00000001f)));
                        }
                        else
                        {
                            gpc.size = GridpieceController.ONExONE;
                        }
                    }
                    else if (shapeLevel > 2)
                    {
                        if (Random.value > 0.5f)
                        {
                            gpc.size = GridpieceController.TWOxONE;
                        }
                        else
                        {
                            gpc.size = GridpieceController.ONExONE;
                        }
                    }
                    else 
                    {
                        gpc.size = GridpieceController.ONExONE;
                    }
                }
                else if (x == 1) 
                {
                    if (!gridObjects[x, y - 1]) 
                    {
                        if (shapeLevel > 1)
                        {
                            if (Random.value > 0.5f)
                            {
                                gpc.size = GridpieceController.ONExTWO;
                            }
                            else
                            {
                                gpc.size = GridpieceController.ONExONE;
                            }
                        }
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
    }

    // UPDATED FOR MULTIPLE SIZES
    public void MovePieceToPosition(GameObject piece, int x, int y) {
        if (piece == null)
            Debug.Log("Warning (MovePieceToPosition): piece given does not exist - nothing done");
        else {
            GridpieceController gpc = piece.GetComponent<GridpieceController>();
            int pieceSize = gpc.size;
            if ((y < 0 || y >= gridSize.y + extraY) || ((pieceSize == GridpieceController.ONExTWO || pieceSize == GridpieceController.TWOxTWO) && y < 1))
                Debug.Log("Warning (MovePieceToPosition): trying to move piece below or above grid Y limits - nothing done");
            else if ((x < 1 || x >= gridSize.x + extraX) || ((pieceSize == GridpieceController.TWOxONE || pieceSize == GridpieceController.TWOxTWO) && x < 2))
                Debug.Log("Warning (MovePieceToPosition): trying to move piece below or above grid X limits - nothing done");
			else if (gridObjects[x, y] != null && !gridObjects[x, y].Equals(piece))
                Debug.Log("Warning (MovePieceToPosition): trying to move piece into occupied space " + x + ", " + y + " - nothing done");
			else if( (pieceSize == GridpieceController.ONExTWO && gridObjects[x, y - 1] && !gridObjects[x, y - 1].Equals(piece)) || 
					 (pieceSize == GridpieceController.TWOxONE && gridObjects[x - 1, y] && !gridObjects[x - 1, y].Equals(piece)) || 
					 (pieceSize == GridpieceController.TWOxTWO && 
																( (gridObjects[x - 1, y] && !gridObjects[x - 1, y].Equals(piece)) || 
																  (gridObjects[x, y - 1] && !gridObjects[x, y - 1].Equals(piece)) || 
																  (gridObjects[x - 1, y - 1] && !gridObjects[x - 1, y-1].Equals(piece)) ) ) )
                Debug.Log("Warning (MovePieceToPosition): piece of given size cannot fit into space provided because another block is in the way - nothing done");
            else {
                if (DEBUG)
                    Debug.Log("Moving Piece: [" + gpc.dimX + ", " + gpc.dimY + "] to [" + x + ", " + y + "]");

                Vector2[] originalPositions = gpc.GetPositions();
                for (int i = 0; i < originalPositions.Length; i++)
                    gridObjects[(int)originalPositions[i].x, (int)originalPositions[i].y] = null;
                gpc.dimX = x;
                gpc.dimY = y;
                Vector2[] newPositions = gpc.GetPositions();
                for (int i = 0; i < newPositions.Length; i++)
                    gridObjects[(int)newPositions[i].x, (int)newPositions[i].y] = piece;

                StartCoroutine(MovePiece(piece, pieceSize, x, y, true));
                // Can swap the line above for the line below in order to create a moving piece
                // StartCoroutine(MovePiece(piece, pieceSize, x, y, false));

                /*
                   if (pieceSize == GridpieceController.ONExONE) {

                   gridObjects[gpc.dimX, gpc.dimY] = null;
                   gridObjects[x, y] = piece;
                   gpc.dimX = x;
                   gpc.dimY = y;

                   piece.transform.position = gridPositions[x, y];
                // Can swap the line above for the line below in order to create a moving piece
                //StartCoroutine(MovePiece(piece, x, y));
                }
                else if (pieceSize == GridpieceController.ONExTWO) {
                piece.transform.position = gridPositions[x, y];
                }
                else if (pieceSize == GridpieceController.TWOxONE) {
                piece.transform.position = gridPositions[x, y];
                }
                else if (pieceSize == GridpieceController.TWOxTWO) {
                piece.transform.position = gridPositions[x, y];
                }
                */
            }
        }
    }

    public IEnumerator MovePiece(GameObject piece, int pieceSize, int x, int y, bool instantly) {
        float timeForPieceToMove = 0.5f;
        Vector3 startPos = piece.transform.position;
        Vector3 endPos;
        if (pieceSize == GridpieceController.ONExONE)
            endPos = gridPositions[x, y];
        else if (pieceSize == GridpieceController.ONExTWO)
            endPos = (gridPositions[x, y] + gridPositions[x, y-1]) / 2;
        else if (pieceSize == GridpieceController.TWOxONE)
            endPos = (gridPositions[x, y] + gridPositions[x-1, y]) / 2;
        else
            endPos = (gridPositions[x, y] + gridPositions[x-1, y-1]) / 2;

        if (!instantly) {
            for (float i = 0; i <= timeForPieceToMove; i += Time.deltaTime) {
                float counter = i / timeForPieceToMove;
                piece.transform.position = Vector3.Lerp(startPos, endPos, counter);
                yield return null;
            }
        }
        piece.transform.position = endPos;
    }

    public void ResetCurrentLastPieces() {
        currentPiece = Vector2.one * -1;
        lastPiece = Vector2.one * -1;
    }
}
