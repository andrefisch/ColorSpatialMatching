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
        gridObjects = new GameObject[(int)gridSize.x + extraX, (int)gridSize.y + extraY];


        gridPositions = new Vector3[(int)gridSize.x, (int)gridSize.y];
        objectControllers = new GridpieceController[(int)gridSize.x + extraX, (int)gridSize.y];

        // set up Grid Piece positions
        // Could probably do a better version where they're even around 0 instead of spaced out as I have them currently
        for (int i = 0; i < gridSize.x; i++) {
            for (int j = 0; j < gridSize.y; j++) {
                gridPositions[i, j] = new Vector3( i * gridSpacing.x, j * gridSpacing.y, 0);
            }
        } 

        // Set up the actual pieces
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j <= gridSize.y - 1; j++) {
                GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[i - 1, j], Quaternion.identity);
                objectControllers[i - 1, j] = go.GetComponent<GridpieceController>();
                objectControllers[i - 1, j].type = (int)Mathf.Floor(Random.Range(1, 4.99999999f));
                gridObjects[i, j] = go;
            }
        }
    }

    // do the two blocks match?
    // UNTESTED
    void Match(GameObject[,] board, int x1, int y1, int x2, int y2)
    {
        // if the blocks arent the same type no more checking needs to be done
        if (board[x1, y1] == board[x2, y2])
        {
            // if the blocks are adjacent we are done and no more checking needs to happen
            if (Adjacent(board, x1, y1, x2, y2))
            {
                board[x1, y1] = null;
                board[x2, y2] = null;
            }
            // otherwise we compare match trails
            else
            {
                CheckMatchTrails(board, CreateMatchTrail(board, x1, y1), CreateMatchTrail(board, x2, y2));
            }
        }
    }
    

    // are the two blocks adjacent?
    // UNTESTED
    bool Adjacent(GameObject[,] board, int x1, int y1, int x2, int y2){
        if ((Mathf.Abs(x1 - x2) == 1 && Mathf.Abs(y1 - y2) != 1) || (Mathf.Abs(x1 - x2) != 1 && Mathf.Abs(y1 - y2) == 1))
        {
            if (DEBUG)
            {
                Debug.Log("Now comparing " + board[x1, y1] + " and " + board[x2, y2]);
            }
            if (board[x1, y1] == board[x2, y2])
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
                    if (board[x1, y] != null)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int y = y2 + 1; y < y1; y++)
                {
                    if (board[x1, y] != null)
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
                    if (board[x, y1] != null)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int x = x2 + 1; x < x2; x++)
                {
                    if (board[x, y1] != null)
                    {
                        return output;
                    }
                }
            }
            output = true;
        }
        return output;
    }

    // THE DIRECTIONS NEED TO BE CHANGED
    // creates a trail of -1's from all possible directions of input block
    // UNTESTED
    List<Vector2> CreateMatchTrail(GameObject[,] board, int x1, int y1)
    {
        List<Vector2> output = new List<Vector2>();
        // create a trail in up to 4 possible directions for both blocks
        // check down only if we are not at the bottom row
        if (x1 + 1 < gridSize.y + extraY)
        {
            if (DEBUG)
            {
                Debug.Log ("now checking down");
            }
            for (int i = x1 + 1; i < (gridSize.y + extraY); i++)
            {
                if (DEBUG)
                {
                    Debug.Log ("Now checking " + i + ", " + y1);
                }
                if (board[i, y1] == null)
                {
                    board[i, y1].GetComponent<GridpieceController>().type = -1;
                    output.Add(new Vector2(i, y1));
                }
                else
                {
                    break;
                }
            }
        }
        // check up
        if (DEBUG)
        {
            Debug.Log ("now checking up");
        }
        for (int i = x1 - 1; i > -1; i--)
        {
            if (DEBUG)
            {
                Debug.Log ("Now checking" + i + ", " + y1);
            }
            if (board[i, y1] == null)
            {
                board[i, y1].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(i, y1));
            }
            else
            {
                break;
            }
        }
        // check right
        if (DEBUG)
        {
            Debug.Log("now checking right");
        }
        for (int i = y1 + 1; i < gridSize.x + extraX; i++)
        {
            if (DEBUG)
            {
                Debug.Log ("Now checking" + x1 + ", " + i);
            }
            if (board[x1, i] == null)
            {
                board[x1, i].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(x1, i));
            }
            else
            {
                break;
            }
        }
        // check left
        if (DEBUG)
        {
            Debug.Log("now checking left");
        }
        for (int i = y1 - 1; i > -1; i--)
        {
            if (DEBUG)
            {
                Debug.Log("Now checking" + x1 + ", " + i);
            }
            if (board[x1, i] == null)
            {
                board[x1, i].GetComponent<GridpieceController>().type = -1;
                output.Add(new Vector2(x1, i));
            }
            else
            {
                break;
            }
        }
        return output;
    }


    // checks to see if there is a straightShot between any -1 in list1 and list2
    // UNTESTED
    bool CheckMatchTrails(GameObject[,] board, List<Vector2> list1, List<Vector2> list2)
    {
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
        return false;
    }

    // remove all -1's from the board
    // UNTESTED
    void MatchTrailCleanup(GameObject[,] board)
    {
        for (int i = 0; i <= gridSize.x + extraX; i++)
        {
            for (int j = 0; j <= gridSize.y + extraY; j++)
            {
                if (board[i, j].GetComponent<GridpieceController>().type == -1)
                {
                    board[i, j] = null;
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        bool highlightedPiece = false;
        bool selectedPiece = false;
        for (int i = 1; i <= gridSize.x; i++) {
            for (int j = 0; j <= gridSize.y - 1; j++) {
                if (objectControllers[i - 1, j].highlighted) {
                    highlighter.transform.position = gridPositions[i - 1, j];
                    highlightedPiece = true;
                    if (DEBUG){
                        Debug.Log("Dimensions of highlighted piece: " + i + ", " + j);
                    }
                }
                if (objectControllers[i - 1, j].selected) {
                    selector.transform.position = gridPositions[i - 1, j];
                    if (currentPiece != null)
                    {
                        lastPiece = currentPiece;
                    }
                    selectedPiece = true;
                    // CHECK FOR A MATCH
                    if (lastPiece != null)
                    {
                        Match(gridObjects, (int)currentPiece.x, (int)currentPiece.y, (int)lastPiece.x, (int)lastPiece.y);
                    }
                    if (DEBUG){
                        Debug.Log("Dimensions of selected piece: " + i + ", " + j);
                    }
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
                    for (int j = 0; j <= gridSize.y - 1; j++) {
                        // FOR ANDREW:  Based on your algorithm, this part might screw with it because when you select a new object, it goes through and deselects all the existing objects before selecting the new one
                        // This is because I don't keep track well enough of the selected piece.  Probably should track it better throughout
                        objectControllers[i - 1, j].selected = false;
                    }
                }
                hit.collider.GetComponent < GridpieceController>().selected = true;
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
                for (int i = 1; i <= gridSize.x; i++) {
                    for (int j = 0; j <= gridSize.y - 1; j++)
                        objectControllers[i - 1, j].selected = false;
                }
            }
        }
    }
}
