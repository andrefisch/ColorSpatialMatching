﻿using UnityEngine;
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

    public Vector2 currentPiece;
    public Vector2 lastPiece;

    // This is how you do a 2D array in C# ---  int[][] is an array of arrays
    public GameObject[,] gridObjects;

    private GridpieceController[,] objectControllers;
    private Vector3[,] gridPositions;

    // Use this for initialization
    void Start () {
        currentPiece = new Vector2(-1, -1);
        lastPiece = new Vector2(-1, -1);

        gridObjects = new GameObject[(int)gridSize.x + extraX, (int)gridSize.y + extraY];


		gridPositions = new Vector3[(int)gridSize.x + extraX, (int)gridSize.y + extraY];
		objectControllers = new GridpieceController[(int)gridSize.x + extraX, (int)gridSize.y + extraY];

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
                GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i, j], Quaternion.identity);
                objectControllers[i - 1, j] = go.GetComponent<GridpieceController>();
                objectControllers[i - 1, j].type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
                objectControllers[i - 1, j].dimX = i;
                objectControllers[i - 1, j].dimY = j;
                gridObjects[i, j] = go;
            }
        }

		// Set up the edge pieces
		// Go from 0 to size on the left edge
		for (int i=0; i<gridSize.y; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[0, i], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[0, i] = go;
		}
		// Go from 0 to size on the right edge
		for (int i=0; i<gridSize.y; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[(int)gridSize.x + extraX - 1, i], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[(int)gridSize.x + extraX - 1, i] = go;
		}
		// Go from 0 to size on the top
		for (int i=0; i<gridSize.x + extraX; i++) {
			GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i, (int)gridSize.y + extraY - 1], Quaternion.identity);
			go.GetComponent<GridpieceController>().type = 0;
			gridObjects[i, (int)gridSize.y + extraY - 1] = go;
		}

    }

    // do the two blocks match?
    // UNTESTED
    bool Match(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        if (DEBUG)
        {
            Debug.Log("---------------------------------------------------------------");
        }
        bool match = false;
        // make sure the pieces are all valid pieces
        if (x1 == 0 && x1 == gridSize.x + extraX - 1 &&
            x2 == 0 && x2 == gridSize.x + extraX - 1 &&
            y1 == gridSize.y + extraY - 1 &&
            y2 == gridSize.y + extraY - 1)
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
                        Debug.Log("Not adjacent so checking match trails");
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
            gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
            gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().type = 0;
            gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().sr.color = Color.gray;
        }
        MatchTrailCleanup(board);
        return match;
    }
    

    // are the two blocks adjacent?
    // UNTESTED
    bool Adjacent(GameObject[,] board, int x1, int y1, int x2, int y2){
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
    // UNTESTED
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
    // UNTESTED
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

    // check if both tiles are adjacent to a -1. if they are its a match
    bool check90(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        bool match1 = false;
        bool match2 = false;
        if (board[x1 - 1, y1].GetComponent<GridpieceController>().type == -1 ||
            board[x1 + 1, y1].GetComponent<GridpieceController>().type == -1 ||
            (y1 > 0 && board[x1, y1 - 1].GetComponent<GridpieceController>().type == -1) ||
            board[x1, y1 + 1].GetComponent<GridpieceController>().type == -1)
        {
            match1 = true;
        }
        if (board[x2 - 1, y2].GetComponent<GridpieceController>().type == -1 ||
            board[x2 + 1, y2].GetComponent<GridpieceController>().type == -1 ||
            (y2 > 0 && board[x2, y2 - 1].GetComponent<GridpieceController>().type == -1) ||
            board[x2, y2 + 1].GetComponent<GridpieceController>().type == -1)
        {
            match2 = true;
        }
        return match1 && match2;
    }

    // checks to see if there is a straightShot between any -1 in list1 and list2
    // UNTESTED
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
    // UNTESTED
    void MatchTrailCleanup(GameObject[,] board)
    {
        for (int i = 0; i < gridSize.x + extraX; i++)
        {
            for (int j = 0; j < gridSize.y + extraY; j++)
            {
                if (board[i, j].GetComponent<GridpieceController>().type == -1)
                {
                    board[i, j].GetComponent<GridpieceController>().type = 0;
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        // WHAT ARE THE CURRENT AND LAST PIECE
        if (DEBUG)
        {
            // Debug.Log("Current piece: " + currentPiece.x + ", " + currentPiece.y);
            // Debug.Log("Last piece: " + lastPiece.x + ", " + lastPiece.y);
        }

        bool highlightedPiece = false;
        bool selectedPiece = false;
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j <= gridSize.y - 1; j++) {
                if (objectControllers[i - 1, j].highlighted) {
                    highlighter.transform.position = gridPositions[i, j];
                    highlightedPiece = true;
                    if (DEBUG){
                        // Debug.Log("Dimensions of highlighted piece: " + i + ", " + j);
                    }
                }
                if (objectControllers[i - 1, j].selected) {
                    selector.transform.position = gridPositions[i, j];
                    selectedPiece = true;
                }
            }
        }

        // WHAT ARE THE CURRENT AND LAST PIECE
        if (DEBUG)
        {
            // Debug.Log("Current piece: " + currentPiece.x + ", " + currentPiece.y);
            // Debug.Log("Last piece: " + lastPiece.x + ", " + lastPiece.y);
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
                    for (int j = 0; j <= gridSize.y - 1; j++) {
                        // FOR ANDREW:  Based on your algorithm, this part might screw with it because when you select a new object, it goes through and deselects all the existing objects before selecting the new one
                        // This is because I don't keep track well enough of the selected piece.  Probably should track it better throughout
                        objectControllers[i - 1, j].selected = false;
                    }
                }
                hit.collider.GetComponent <GridpieceController>().selected = true;
                if (currentPiece.x != -1 && currentPiece.y != -1)
                {
                    lastPiece.x = currentPiece.x;
                    lastPiece.y = currentPiece.y;
                }
                currentPiece.x = hit.collider.GetComponent <GridpieceController>().dimX;
                currentPiece.y = hit.collider.GetComponent <GridpieceController>().dimY;
                // CHECK FOR A MATCH
                if (DEBUG)
                {
                    Debug.Log("Dimensions of selected piece: " + currentPiece.x + ", " + currentPiece.y);
                    Debug.Log("Dimensions of last selected piece: " + lastPiece.x + ", " + lastPiece.y);
                }
                if (lastPiece.x != -1)
                {
                    if (Match(gridObjects, (int)currentPiece.x, (int)currentPiece.y, (int)lastPiece.x, (int)lastPiece.y))
                    {
                        Debug.Log("THEY MATCH!");
                    }
                    else
                    {
                        Debug.Log("NO MATCH...");
                    }
                }
            }
        }
        else {
            for (int i = 1; i <= gridSize.x; i++) {
                for (int j = 0; j <= gridSize.y - 1; j++) {
                    objectControllers[i - 1, j].highlighted = false;
                }
            }
            // If you click away, it deselects any piece
            if (Input.GetMouseButtonDown(0)) {
                currentPiece = new Vector2(-1, -1);
                lastPiece = new Vector2(-1, -1);
                for (int i = 1; i <= gridSize.x; i++) {
                    for (int j = 0; j <= gridSize.y - 1; j++)
                        objectControllers[i - 1, j].selected = false;
                }
            }
        }
    }
}
