﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class PlaygridController : MonoBehaviour {

    public bool DECOMBOS;
    public bool FREEZE;
    public bool usesTimer = true;
    public GameObject gridPiece;
    // 1x1 is 0, 1x2 is 1, 2x1 is 2, 2x2 is 3
    public GameObject[] highlighters;
    public GameObject[] selectors;

	public Canvas saveCanvas;

	public GameObject topLine;

    // SCORE STUFF
    public Text scoreText;
    public int colorLevel;
    public int shapeLevel;
    public int[] colorThresholds;
    public float[] sizeFrequencies;
    public int[] newLineThresholds;
    public int[] newLineSpeeds;
    public int score;
    public int combos;
    public int comboScore;
    public int matchesMade;
    public int blocksDestroyed;
    public Color edgeColor;

    // WHITE BLOCKS CAN COMBO?
    public bool whiteCombo = false;

    // HOW MANY EXTRA ROWS AND COLUMNS THERE ARE
    public const int extraX = 2;
    public const int extraY = 2;

	public bool loadSavedBoard;
	public bool useSpecificGrid;
	public string specificGridFileName;
    public Vector2 gridSize;
    public Vector2 gridSpacing;

    public bool includeBigPieces;
    public bool includeGoodSpecialBlocks;
    public bool includeBadSpecialBlocks;
    public bool keepHalfFull;

    // COUNTER TO KEEP TRACK OF WHEN THE BLOCKS FALL
    public bool startCounting;
    public int newLineCounter;
    public int newLineInterval;
    // private int newLineBuffer;
    private int processingCounter;
    private int processingInterval;
    public bool whiteOut;
    private int whiteOutCounter;
    private int whiteOutTimer;
    public bool pause;
    private int pauseCounter;
    private int pauseTimer;
    public int lastNote;

    //public GridpieceController gpc;
    // MATCHING VARIABLES
    public Camera mainCamera;
    private LineRenderer line;
    private Vector2 currentPiece;
    private Vector2 lastPiece;
    private List<Vector2> matchTrack;
    private List<Vector2> lineTrack;

    // This is how you do a 2D array in C# ---  int[][] is an array of arrays
    public GameObject[,] gridObjects;

    // Keep track of which objects fell down for combo checking
    public List<GameObject> movedObjects;
    public List<GameObject> objectsToProcess;
    public HashSet<int> recentlyDestroyed;

    private Vector3[,] gridPositions;

    //  This is to help keep track of the highlighted piece's position so that it doesn't get messed up if you don't happen to move the mouse off a piece between frames 
    private Vector2 highlightedPiece;
    private GridpieceController highlightedGridpiece;

	// For the buffer time 
	private bool justAddedRow;
	private bool selectionBuffer;
	private Coroutine selectionbufferCo;
	private const float NEWLINE_SELECTION_BUFFER_TIME = .25f;


    // Use this for initialization
    void Start(){
        FREEZE = false;
        DECOMBOS = false;
        // includeBigPieces = true;
        // SCORE STUFF
        // colorLevel = 4;
        // shapeLevel = 1;
        score = 0;
        combos = 1;
        comboScore = 0;
        matchesMade = 0;
        blocksDestroyed = 0;

        edgeColor = Color.clear;

        startCounting = false;
        newLineCounter = 0;
        newLineInterval = newLineSpeeds[0];
        processingCounter = 0;
        processingInterval = 20;
        whiteOut = false;
        whiteOutCounter = 0;
        pause = false;
        pauseCounter = 0;

        recentlyDestroyed = new HashSet<int>();

        // mainCamera = GameObject.FindGameObjectWithTag("Camera").GetComponent<Camera>();
        line = gameObject.GetComponent<LineRenderer>();

        currentPiece = new Vector2(-1, -1);
        lastPiece = new Vector2(-1, -1);
        matchTrack = new List<Vector2>();
        lineTrack = new List<Vector2>();

        // Set up the board
		LoadPlayBoard();
		Vector3 linePos = gridPositions[(int)(gridSize.x / 2), (int)(gridSize.y - 1)] + new Vector3(1, .85f);
		topLine.transform.position = linePos;
    }

    // Update at a fixed interval so we can count accurately
    // NO KNOWN BUGS
    void FixedUpdate()
    {
        bool DEFIXEDUPDATE = false;
        UpdateScore();
        if (!GlobalVariables.gameOver)
        {
            if (!pause && usesTimer)
            {
                newLineCounter++;
            }
            // When time is stopped we do not add new rows
            if (!pause && !FREEZE && newLineCounter >= newLineInterval)
            {
                if (AlmostLost())
                {
                    if (combos == 1)
                    {
                        AddRow(-1, true);
                    }
                }
                else
                {
                    AddRow(-1, true);
                }
            } 
            // TODO: make this happen on a delay
            if (keepHalfFull && !AtLeastHalfFull() && !AlmostLost())
            {
                AddRow(-1, true);
            }
            if (combos == 1)
            {
                lastNote = 0;
            }
            if (startCounting)
            {
                processingCounter++;
            }
            if (!FREEZE && movedObjects.Count == 0 && processingCounter >= processingInterval)
            {
                CheckHorizontalSpecial();
                MovePiecesDown();
                if (movedObjects.Count == 0)
                {
                    combos = 1;
                }
            }
            else if (!FREEZE && movedObjects.Count > 0 && processingCounter >= processingInterval)
            {
                ProcessCombos();
            }
            // What color blocks are we using
            if (colorThresholds.Count() > 0 && score > colorThresholds[0])
            {
                if (colorLevel < 5)
                {
                    colorLevel = 5;
                }
            }
            if (colorThresholds.Count() > 1 && score > colorThresholds[1])
            {
                if (colorLevel < 6)
                {
                    colorLevel = 6;
                }
                if (shapeLevel < 2)
                {
                    shapeLevel = 2;
                }
            }
            if (colorThresholds.Count() > 2 && score > colorThresholds[2])
            {
                if (colorLevel < 7)
                {
                    colorLevel = 7;
                }
            }
            if (colorThresholds.Count() > 3 && score > colorThresholds[3])
            {
                if (colorLevel < 8)
                {
                    colorLevel = 8;
                }
                if (shapeLevel < 4)
                {
                    shapeLevel = 4;
                }
            }
            if (colorThresholds.Count() > 4 && score > colorThresholds[4])
            {
                if (colorLevel < 9)
                {
                    colorLevel = 9;
                }
            }
            for (int i = 0; i < newLineThresholds.Count(); i++)
            {
                if (score >= newLineThresholds[i])
                {
                    newLineInterval = newLineSpeeds[i];
                }
            }
            if (whiteOut)
            {
                whiteOutCounter++;
            }
            if (whiteOutCounter > whiteOutTimer)
            {
                Repaint();
            }
            if (pause)
            {
                pauseCounter++;
            }
            if (pauseCounter > pauseTimer)
            {
                this.GetComponent<AudioSourceController>().PlayThawSound();
                pause = false;
                pauseCounter = 0;
            }
        }
    }   

    // Update is called once per frame
    // NO KNOWN BUGS
    void Update() {


		bool DEUPDATE = false;
        CheckPieces();
       

		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown("s") && !Input.GetKey(KeyCode.Space))
			SavePlayBoard();

		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown("s") && Input.GetKey(KeyCode.Space))
			ActivateSaveCanvas();

		if (Input.GetKey(KeyCode.Escape))
            if (FREEZE)
                FREEZE = false;
            else
                FREEZE = true;


        // check to see if there are any null blocks
		/*
		 * For Andrew:  I commented all these out because if we were to type a name into the game for a save game file, we would hit some keys and change the board
		 * Therefore we should not leave these around and comment them out when not using them
		 * 
         */
        if (Input.GetKeyDown("a")) {
            if (!AlmostLost())
            {
                AddRow(-1, true);
            }
        }
        if (Input.GetKeyDown("g")){
            AddRow(3, true);
        }

        // FOR DEBUGGING MAKES SURE THERE ARE NO BLANK SPACES IN THE GRID
        if (Input.GetKeyDown("c")) {
            CheckPieces();
        }
		// for debugging, check a certain method or line specifically
		if (Input.GetKeyDown("x")) {
			//RemovePieceAtPosition(0, (int)gridSize.y);
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

        if (Input.GetKeyDown("h")){
            Debug.Log("blockColor of the current block is " + gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().blockColor);
        }
        if (Input.GetKeyDown("i"))
        {
            Debug.Log("Block " + (int)currentPiece.x + ", " + (int)currentPiece.y + " is in the top row: " + (currentPiece.y == gridSize.y));
        }
        if (Input.GetKeyDown("j")){
            Whiteout();
        }
        if (Input.GetKeyDown("k")){
            Repaint();
        }
		if (GlobalVariables.gameOver)
        {
            if (Input.GetKeyDown("r"))
            {
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
        }

		if (!GlobalVariables.gameOver) {
			// First we check to see if any special blocks have activated
			for (int i = 1; i < gridSize.x + extraX; i++) 
            {
				for (int j = 1; j < gridSize.y + extraY; j++) 
                {
					if (gridObjects[i, j]) 
                    {
						GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
						if (gpc.countdown <= 0) 
                        {
							ActivateTimerSpecial(gpc.dimX, gpc.dimY);
						}
					}
				}
			}
			// This part deals with the highlighting and selecting of objects
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).x, GlobalVariables.cam.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
			// We need these so that the highlighter and selector part knows the size of the piece we're dealing with so that it can use the proper image
			int gpcHighlightSize = -1;
			highlightedPiece = Vector2.one * -1;
			if (hit.collider != null) 
            {
                /*
                // when we select another piece make sure we whitewash the previous piece only if we are in whiteout;
                if (whiteOut && highlightedGridpiece)
                {
                    highlightedGridpiece.Colorwash(Color.white, true);
                }
                */
				highlightedGridpiece = hit.collider.gameObject.GetComponent<GridpieceController>();
				if (selectionBuffer && highlightedGridpiece.dimY < gridSize.y - 1 && !(highlightedGridpiece.size == GridpieceController.ONExTWO || highlightedGridpiece.size == GridpieceController.TWOxONE || highlightedGridpiece.size == GridpieceController.TWOxTWO)) 
                {
					highlightedGridpiece = gridObjects[highlightedGridpiece.dimX, highlightedGridpiece.dimY + 1].GetComponent<GridpieceController>();
				}

                gpcHighlightSize = highlightedGridpiece.size;
                // HIGHLIGHT BLOCK UNDER MOUSE IN PC VERSION ONLY
				if ((highlightedGridpiece.blockColor != 0 && highlightedGridpiece.blockColor != GridpieceController.WHITE) && (highlightedGridpiece.dimX >= 1 && highlightedGridpiece.dimX <= gridSize.x && highlightedGridpiece.dimY >= 0 && highlightedGridpiece.dimY <= gridSize.y)) 
                {
					highlightedPiece.x = highlightedGridpiece.dimX;
					highlightedPiece.y = highlightedGridpiece.dimY;
				}

				if (Input.GetMouseButtonDown(0)) 
                {
                    // When we select a piece, if we are in whiteout, whiteout all other pieces
                    if (whiteOut)
                    {
                        Whiteout();
                    }
					// First deselect any selected piece
					for (int i = 1; i <= gridSize.x; i++) 
                    {
						for (int j = 1; j <= gridSize.y; j++) 
                        {
							if (gridObjects[i, j]) 
                            {
								gridObjects[i, j].GetComponent<GridpieceController>().selected = false;
							}
						}
					}
                    // SELECT PIECE
					// Select only pieces that aren't 0 or white and are in the playable grid
					if ((highlightedGridpiece.blockColor != 0 && highlightedGridpiece.blockColor != GridpieceController.WHITE) && (highlightedGridpiece.dimX >= 1 && highlightedGridpiece.dimX <= gridSize.x && highlightedGridpiece.dimY >= 1 && highlightedGridpiece.dimY <= gridSize.y)) 
                    {
						// if it's not new, toggle the selection
						if (highlightedGridpiece.dimX == currentPiece.x && highlightedGridpiece.dimY == currentPiece.y) 
                        {
							highlightedGridpiece.selected = false;
							ResetCurrentLastPieces();
						}
						else 
                        {
							highlightedGridpiece.selected = true;
							if (DEUPDATE) 
                            {
								Debug.Log("Grid spaces of this block are:");
								Vector2[] gridSpaces = highlightedGridpiece.GetComponent<GridpieceController>().GetPositions();
								for (int i = 0; i < gridSpaces.Length; i++) 
                                {
									Debug.Log((int)gridSpaces[i].x + ", " + (int)gridSpaces[i].y);
								}
							}

							if (currentPiece.x != -1 && currentPiece.y != -1) 
                            {
								lastPiece.x = currentPiece.x;
								lastPiece.y = currentPiece.y;
							}
                            // If we are in whiteout, repaint all blocks of the same color
                            if (whiteOut)
                            {
                                // Repaint the selected block
                                int selectedColor = highlightedGridpiece.blockColor;
                                // highlightedGridpiece.Repaint();
                                // Repaint all other blocks of the same color
                                for (int i = 1; i <= gridSize.x; i++) 
                                {
                                    for (int j = 1; j <= gridSize.y; j++)
                                    {
                                        if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().blockColor == selectedColor)
                                        {
                                            gridObjects[i, j].GetComponent<GridpieceController>().Repaint();
                                        }
                                    }
                                }
                            }
							// Make sure the current piece is the one selected
							currentPiece.x = highlightedGridpiece.dimX;
							currentPiece.y = highlightedGridpiece.dimY;
						}

						if (DEUPDATE) 
                        {
							// Debug.Log("----------------------Clicked Playgrid Piece----------------------------");
							// Debug.Log("Dimensions of selected piece: " + currentPiece.x + ", " + currentPiece.y);
							// Debug.Log("Dimensions of last selected piece: " + lastPiece.x + ", " + lastPiece.y);
						}
					}
				}

			}
			else 
            {
				gpcHighlightSize = -1;

				// If you click away, it deselects any piece
				if (Input.GetMouseButtonDown(0)) 
                {
					ResetCurrentLastPieces();
					for (int i = 1; i <= gridSize.x; i++) 
                    {
						for (int j = 1; j <= gridSize.y; j++)
                        {
							if (gridObjects[i, j])
                            {
								gridObjects[i, j].GetComponent<GridpieceController>().selected = false;
                            }
                        }
					}
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
							
							if (gpc.size == GridpieceController.ONExONE) {
								selectors[GridpieceController.ONExONE].transform.position = gridPositions[i, j];
								selectors[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
							}
							else if (gpc.size == GridpieceController.ONExTWO) {
								selectors[GridpieceController.ONExTWO].transform.position = Vector3.Lerp(gridPositions[gpc.dimX, gpc.dimY], gridPositions[gpc.dimX, gpc.dimY - 1], 0.5f);
								selectors[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.TWOxONE].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
							}
							else if (gpc.size == GridpieceController.TWOxONE) {
								selectors[GridpieceController.TWOxONE].transform.position = Vector3.Lerp(gridPositions[gpc.dimX, gpc.dimY], gridPositions[gpc.dimX - 1, gpc.dimY], 0.5f);
								selectors[GridpieceController.ONExTWO].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.ONExONE].transform.position = new Vector3(-10, 10, 0);
								selectors[GridpieceController.TWOxTWO].transform.position = new Vector3(-10, 10, 0);
							}
							else if (gpc.size == GridpieceController.TWOxTWO) {
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
		else {
			ResetCurrentLastPieces();
			highlighters[0].transform.position = new Vector3(-10, 10, 0);
			highlighters[1].transform.position = new Vector3(-10, 10, 0);
			highlighters[2].transform.position = new Vector3(-10, 10, 0);
			highlighters[3].transform.position = new Vector3(-10, 10, 0);
			selectors[0].transform.position = new Vector3(-10, 10, 0);
			selectors[1].transform.position = new Vector3(-10, 10, 0);
			selectors[2].transform.position = new Vector3(-10, 10, 0);
			selectors[3].transform.position = new Vector3(-10, 10, 0);
		}
    }

    // Score the block based on size
    // NO KNOWN BUGS
    int ScoreBlock(int x, int y)
    {
        bool DESCOREBLOCK = false;
        GridpieceController piece = gridObjects[x, y].GetComponent<GridpieceController>();
        int toAdd = 0;
        // Pieces are worth exactly how many gridspaces they take up
        // Small pieces are worth a point each
        if (piece.size == 0)
        {
            toAdd = 1;
        }
        // Large pieces are worth 4 each
        else if (piece.size == 3)
        {
            toAdd = 4;
        }
        // Long pieces are always worth 2
        else
        {
            toAdd = 2;
        }
        if (DESCOREBLOCK)
        {
            // Debug.Log("Value to add: " + toAdd);
            // Debug.Log("Combo value: " + combos);
        }
        // Obviously add the score
        score += (toAdd * combos);
        if (DESCOREBLOCK)
        {
            // Debug.Log("Score to add is: " + (toAdd * combos));
        }
        // Increment blocksDestroyed variable
        blocksDestroyed++;
        return toAdd;
    }

    void UpdateScore ()
    {
        // scoreText.text = "Score: " + score.ToString();
        scoreText.text = "Matches: " + matchesMade + "\nScore: " + score.ToString() + "\nBlocks Destroyed: " + blocksDestroyed.ToString() + "\nCombos: " + combos.ToString() + "\nMoved Objects: " + movedObjects.Count.ToString() + "\nColorlevel: " + colorLevel + "\nShapeLevel: " + shapeLevel + "\nAddRow Timing: " + newLineInterval + "\nNumber of Blocks: " + CheckPieces();
        // scoreText.text = score.ToString() + ", " + combos.ToString() + ", " + movedObjects.Count.ToString();
    }

    // Do the two blocks match?
    // NO KNOWN BUGS
    bool Match(int x1, int y1, int x2, int y2)
    {
        matchTrack.Clear();
        bool DEMATCH = false;
        if (DEMATCH)
        {
            Debug.Log("----------------------------Running Matching Algorithm-----------------------------------");
        }
        bool match = false;
        // Make sure the pieces are both valid pieces
        if (x1 >= 0 && x1 <= gridSize.x + extraX - 1 &&
            x2 >= 0 && x2 <= gridSize.x + extraX - 1 &&
            y1 >= 0 && y1 <= gridSize.y + extraY - 1 &&
            y2 >= 0 && y2 <= gridSize.y + extraY - 1)
        {
            // If the blocks arent the same blockColor no more checking needs to be done
            if (gridObjects[x1, y1] && gridObjects[x2, y2] && gridObjects[x1, y1].GetComponent<GridpieceController>().blockColor == gridObjects[x2, y2].GetComponent<GridpieceController>().blockColor)
            {
                Vector2[] object1 = gridObjects[x1, y1].GetComponent<GridpieceController>().GetPositions();
                Vector2[] object2 = gridObjects[x2, y2].GetComponent<GridpieceController>().GetPositions();
                if (DEMATCH)
                {
                    // Debug.Log("Now checking adjacency of " + x1 + ", " + y1 + " and " + x2 + ", " + y2);
                }
                // If the blocks are adjacent we are done and no more checking needs to happen
                for (int i = 0; i < object1.Length; i++)
                {
                    for (int j = 0; j < object2.Length; j++)
                    {
                        // Check adjacency
                        if (Adjacent((int)object1[i].x, (int)object1[i].y, (int)object2[j].x, (int)object2[j].y))
                        {
                            if (DEMATCH)
                            {
                                Debug.Log("They are ADJACENT so we have a match!");
                            }
                            match = true;
                        }
                    }
                }
                // Check if there is a straightshot from block 1 to block 2
                if (!match)
                {
                    for (int i = 0; i < object1.Length; i++)
                    {
                        for (int j = 0; j < object2.Length; j++)
                        {
                            // Then we check to see if there is a straight shot from A to B
                            if (StraightShot((int)object1[i].x, (int)object1[i].y, (int)object2[j].x, (int)object2[j].y))
                            {
                                currentPiece = new Vector2((int)object1[i].x, (int)object1[i].y);
                                lastPiece = new Vector2((int)object2[j].x, (int)object2[j].y);
                                // HighlightMatchTrack();
                                if (DEMATCH)
                                {
                                    Debug.Log("There is a STRAIGHTSHOT so we have a match!");
                                }
                                match = true;
                            }
                        }
                    }
                }
                // Check if we can match with 1 or two right angles
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
                    if (DEMATCH)
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
                    if (DEMATCH && match)
                    {
                        Debug.Log("MATCHTRAILS line up so we have a match!");
                    }
                }
                if (match)
                {
                    // Here is where we would display userfeedback stuff with the block
                    matchesMade++;
                    // Get the colors of the blocks before we remove them
                    int currentBlockSize = gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().size;
                    Color currentColor = gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().sr.color;
                    int color = gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().blockColor;
                    int currentBlockType = gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().blockType;
                    int lastBlockSize = gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().size;
                    int lastBlockType = gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().blockType;
                    // Make sure we set the pieces to 0 and gray
                    if (lastNote == 0)
                    {
                        this.GetComponent<AudioSourceController>().PlayNote(color);
                        lastNote = color;
                    }
                    else
                    {
                        this.GetComponent<AudioSourceController>().PlayNote(lastNote);
                    }
                    SoftRemovePieceAtPosition((int)object1[0].x, (int)object1[0].y, currentBlockSize, 3, true);
                    for (int i = 0; i < object1.Length; i++)
                    {
                        if (DEMATCH)
                        {
                            Debug.Log("Now removing 1st piece at " + (int)object1[i].x + ", " + (int)object1[i].y);
                        }
                        if (i == 0)
                        {
                            RemovePieceAtPosition((int)object1[i].x, (int)object1[i].y);
                        }
                        AddPieceAtPosition((int)object1[i].x, (int)object1[i].y, 0, GridpieceController.ONExONE);
                    }
                    // Make sure we deselect and unhighlight last piece
                    gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().selected = false;
                    SoftRemovePieceAtPosition((int)object2[0].x, (int)object2[0].y, lastBlockSize, 3, true);
                    // for (int i = object2.Length - 1; i >= 0; i--)
                    for (int i = 0; i < object2.Length; i++)
                    {
                        if (DEMATCH)
                        {
                            Debug.Log("Now removing 2nd piece " + (int)object2[i].x + ", " + (int)object2[i].y);
                        }
                        if (i == 0)
                        {
                            RemovePieceAtPosition((int)object2[i].x, (int)object2[i].y);
                        }
                        AddPieceAtPosition((int)object2[i].x, (int)object2[i].y, 0, GridpieceController.ONExONE);
                    }
                    // Activate special blocks around the matched blocks
                    ActivateSpecial((int)currentPiece.x, (int)currentPiece.y, currentBlockSize, currentColor, currentBlockType, color);
                    ActivateSpecial((int)lastPiece.x, (int)lastPiece.y, lastBlockSize, currentColor, lastBlockType, color);
                    // Highlight the match track
                    HighlightMatchTrack();
                    // Make sure we reset current piece
                    currentPiece.x = -1;
                    currentPiece.y = -1;
                    // Start the clock
                    startCounting = true;
                    if (combos < 10)
                    {
                        combos++;
                        lastNote++;
                    }
                    if (DECOMBOS)
                    {
                        Debug.Log("Combos incrimented in MATCH. COMBO IS: " + combos);
                    }
                }
            }
        }
        // Reset the last piece so that we aren't checking for a match every frame
        lastPiece.x = -1;
        lastPiece.y = -1;
        MatchTrailCleanup();
        return match;
    }

    // Are the two blocks adjacent and the same color?
    // NO KNOWN BUGS
    bool Adjacent(int x1, int y1, int x2, int y2)
    {
        bool DEADJACENT = false;
        if (CheckAdjacency(x1, y1, x2, y2))
        {
            if (DEADJACENT)
            {
                // Debug.Log("Now comparing " + x1 + "; " + y1 + " and " + x2 + ", " + y2);
            }
            if (gridObjects[x1, y1] && gridObjects[x2, y2] && gridObjects[x1, y1].GetComponent<GridpieceController>().blockColor == gridObjects[x2, y2].GetComponent<GridpieceController>().blockColor)
            {
                currentPiece = new Vector2(x1, y1);
                lastPiece = new Vector2(x2, y2);
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

    // Are the two vector2's adjacent?
    // NO KNOWN BUGS
    bool CheckAdjacency(int x1, int y1, int x2, int y2)
    {
        return (Mathf.Abs(x1 - x2) == 1 && Mathf.Abs(y1 - y2) == 0) || (Mathf.Abs(x1 - x2) == 0 && Mathf.Abs(y1 - y2) == 1);
    }

    bool CheckHorizontalAdjacency(int x1, int y1, int x2, int y2)
    {
        return (Mathf.Abs(x1 - x2) == 1 && Mathf.Abs(y1 - y2) == 0);
    }

    bool CheckVerticalAdjacency(int x1, int y1, int x2, int y2)
    {
        return (Mathf.Abs(x1 - x2) == 0 && Mathf.Abs(y1 - y2) == 1);
    }

    // What are start and end points as well as turning points of the line
    // NO KNOWN BUGS
    void CreateLineTrack()
    {
        bool DECREATELINETRACK = false;
        // add first selected piece
        if (DECREATELINETRACK)
        {
            Debug.Log("Just added START point " + currentPiece.x + ", " + currentPiece.y);
        }
        lineTrack.Clear();
        lineTrack.Add(currentPiece);
        // add turning point piece(s)
        // NO KNOWN BUGS WITH THIS LOOP
        foreach (Vector2 coordinate1 in matchTrack)
        {
            bool horizontal = false;
            bool vertical = false;
            foreach (Vector2 coordinate2 in matchTrack)
            {
                if (CheckVerticalAdjacency((int)coordinate1.x, (int)coordinate1.y, (int)coordinate2.x, (int)coordinate2.y))
                {
                    vertical = true;
                }
                if (CheckHorizontalAdjacency((int)coordinate1.x, (int)coordinate1.y, (int)coordinate2.x, (int)coordinate2.y))
                {
                    horizontal = true;
                }
            }
            if (horizontal && vertical)
            {
                if (DECREATELINETRACK)
                {
                    Debug.Log("Just added TURNING point " + coordinate1.x + ", " + coordinate1.y);
                }
                lineTrack.Add(coordinate1);
            }
        }
        // add last selected piece
        if (DECREATELINETRACK)
        {
            Debug.Log("Just added START point " + lastPiece.x + ", " + lastPiece.y);
        }
        lineTrack.Add(lastPiece);
        lineTrack = lineTrack.Distinct().ToList();
        if (DECREATELINETRACK)
        {
            foreach (Vector2 coordinate in lineTrack)
            {
                Debug.Log(coordinate.x + ", " + coordinate.y);
            }
        }
        ResetCurrentLastPieces();
    }

    // THE DIRECTIONS NEED TO BE CHANGED
    // Creates a trail of -1's from all possible directions of input block
    // NO KNOWN BUGS
    // - NOT UPDATED FOR MULTIPLE SIZES
    List<Vector2> CreateMatchTrail(int x1, int y1)
    {
        bool DECREATEMATCHTRAIL = false;
        List<Vector2> output = new List<Vector2>();
        // Create a trail in up to 4 possible directions for both blocks
        // Check down only if we are not at the bottom row
        if (DECREATEMATCHTRAIL)
        { 
            Debug.Log ("now checking down from " + x1 + ", " + y1); 
        }
        if (y1 > 1)
        {
            for (int i = y1 - 1; i > 0; i--)
            {
                if (DECREATEMATCHTRAIL)
                {
                    // Debug.Log ("Now checking " + x1 + ", " + i);
                }
                if (gridObjects[x1, i] && gridObjects[x1, i].GetComponent<GridpieceController>().blockColor == 0)
                {
                    if (DECREATEMATCHTRAIL)
                    {
                        // Debug.Log("Added a -1!");
                    }
                    gridObjects[x1, i].GetComponent<GridpieceController>().blockColor = -1; 
                    output.Add(new Vector2(x1, i)); 
                }
                else
                {
                    break;
                }
            }
        }
        // Check up
        if (DECREATEMATCHTRAIL)
        {
            Debug.Log ("now checking up from " + x1 + ", " + y1);
        }
        for (int i = y1 + 1; i < gridSize.y + extraY; i++)
        {
            if (DECREATEMATCHTRAIL)
            {
                // Debug.Log ("Now checking " + x1 + ", " + i);
            }
            if (gridObjects[x1, i] && gridObjects[x1, i].GetComponent<GridpieceController>().blockColor == 0)
            {
                if (DECREATEMATCHTRAIL)
                {
                    // Debug.Log("Added a -1!");
                }
                gridObjects[x1, i].GetComponent<GridpieceController>().blockColor = -1;
                output.Add(new Vector2(x1, i));
            }
            else
            {
                break;
            }
        }
        // Check right
        if (DECREATEMATCHTRAIL)
        {
            Debug.Log("now checking right from " + x1 + ", " + y1);
        }
        if (y1 > 0)
        {
            for (int i = x1 + 1; i < gridSize.x + extraX; i++)
            {
                if (DECREATEMATCHTRAIL)
                {
                    // Debug.Log ("Now checking " + i + ", " + y1);
                }
                if (gridObjects[i, y1] && gridObjects[i, y1].GetComponent<GridpieceController>().blockColor == 0)
                {
                    if (DECREATEMATCHTRAIL)
                    {
                        // Debug.Log("Added a -1!");
                    }
                    gridObjects[i, y1].GetComponent<GridpieceController>().blockColor = -1;
                    output.Add(new Vector2(i, y1));
                }
                else
                {
                    break;
                }
            }
        }
        // Check left
        if (DECREATEMATCHTRAIL)
        {
            Debug.Log("now checking left from " + x1 + ", " + y1);
        }
        if (y1 > 0)
        {
            for (int i = x1 - 1; i > -1; i--)
            {
                if (DECREATEMATCHTRAIL)
                {
                    // Debug.Log("Now checking " + x1 + ", " + i);
                }
                if (gridObjects[i, y1] && gridObjects[i, y1].GetComponent<GridpieceController>().blockColor == 0)
                {
                    if (DECREATEMATCHTRAIL)
                    {
                        // Debug.Log("Added a -1!");
                    }
                    gridObjects[i, y1].GetComponent<GridpieceController>().blockColor = -1;
                    output.Add(new Vector2(i, y1));
                }
                else
                {
                    break;
                }
            }
        }
        // Add the block itself to the list of blocks to check
        if (gridObjects[x1, y1])
        {
            output.AddRange(gridObjects[x1, y1].GetComponent<GridpieceController>().GetPositions());
        }
        // output.Add(new Vector2(x1, y1));
        return output;
    }

    // Checks to see if there is anything but 0's between the two spots
    // will even work with indeces that are one out of bounds so we can
    // test blocks on the edge against each other
    // NO KNOWN BUGS
    bool StraightShot(int x1, int y1, int x2, int y2)
    {
        bool DESTRAIGHTSHOT = false;
        bool output = false;
        // Keep track of the coordinates we move through to get the match
        List<Vector2> straightShotTrack = new List<Vector2>();
        Vector2 one = new Vector2(x1, y1);
        Vector2 two = new Vector2(x2, y2);
        straightShotTrack.Add(one);
        straightShotTrack.Add(two);
        if (x1 == x2)
        {
            if (y2 > y1)
            {
                for (int y = y1 + 1; y < y2; y++)
                {
                    Vector2 temp = new Vector2(x1, y);
                    straightShotTrack.Add(temp);
                    if (gridObjects[x1, y] && gridObjects[x1, y].GetComponent<GridpieceController>().blockColor != 0
                                           && gridObjects[x1, y].GetComponent<GridpieceController>().blockColor != -1)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int y = y2 + 1; y < y1; y++)
                {
                    Vector2 temp = new Vector2(x1, y);
                    straightShotTrack.Add(temp);
                    if (gridObjects[x1, y] && gridObjects[x1, y].GetComponent<GridpieceController>().blockColor != 0
                                           && gridObjects[x1, y].GetComponent<GridpieceController>().blockColor != -1)
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
                    Vector2 temp = new Vector2(x, y1);
                    straightShotTrack.Add(temp);
                    if (gridObjects[x, y1] && gridObjects[x, y1].GetComponent<GridpieceController>().blockColor != 0
                                           && gridObjects[x, y1].GetComponent<GridpieceController>().blockColor != -1)
                    {
                        return output;
                    }
                }
            }
            else
            {
                for (int x = x2 + 1; x < x1; x++)
                {
                    Vector2 temp = new Vector2(x, y1);
                    straightShotTrack.Add(temp);
                    if (gridObjects[x, y1] && gridObjects[x, y1].GetComponent<GridpieceController>().blockColor != 0
                                           && gridObjects[x, y1].GetComponent<GridpieceController>().blockColor != -1)
                    {
                        return output;
                    }
                }
            }
            if (DESTRAIGHTSHOT)
            {
                // Debug.Log("There is a HORIZONTAL STRAIGHTSHOT");
            }
            output = true;
        }
        if (output)
        {
            if (DESTRAIGHTSHOT)
            {
                Debug.Log("Added " + straightShotTrack.Count + " coordinates to match track");
            }
            matchTrack.AddRange(straightShotTrack);
        }
        return output;
    }


    // Checks to see if there is a straightShot between any -1 in list1 and list2
    // NO KNOWN BUGS
    bool CheckMatchTrails(List<Vector2> list1, List<Vector2> list2)
    {
        //list1 is currentPiece, list2 is lastPiece
        bool DECHECKMATCHTRAILS = false;
        bool match = false;
        if (DECHECKMATCHTRAILS)
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
                    if (DECHECKMATCHTRAILS)
                    {
                        Debug.Log("Two ends of trail: " + list1[i].x + ", " + list1[i].y + " and " +  list1[i].x + ", " + list1[i].y);
                        Debug.Log("Current piece: " + currentPiece.x + ", " + currentPiece.y + " and Last piece: " +  lastPiece.x + ", " + lastPiece.y);
                    }
                    // Need to do a straightshot from the first coordinate to current block to get matchTrack
                    if (DECHECKMATCHTRAILS)
                    {
                        Debug.Log("Now processing match trails for current piece");
                    }
                    List<Vector2> temp = new List<Vector2>();
                    temp.AddRange(gridObjects[(int)currentPiece.x, (int)currentPiece.y].GetComponent<GridpieceController>().GetPositions());
                    for (int k = 0; k < temp.Count; k++)
                    {
                        if (StraightShot((int)list1[i].x, (int)list1[i].y, (int)temp[k].x, (int)temp[k].y))
                        {
                            currentPiece = temp[k];
                        }
                    }
                    // And to do a straightshot from the second coordinate to last block to fill matchTrack
                    if (DECHECKMATCHTRAILS)
                    {
                        Debug.Log("Now processing match trails for last piece");
                    }
                    temp.Clear();
                    temp.AddRange(gridObjects[(int)lastPiece.x, (int)lastPiece.y].GetComponent<GridpieceController>().GetPositions());
                    for (int k = 0; k < temp.Count; k++)
                    {
                        if (StraightShot((int)list2[j].x, (int)list2[j].y, (int)temp[k].x, (int)temp[k].y))
                        {
                            lastPiece = temp[k];
                        }
                    }
                    // HighlightMatchTrack();
                    return true;
                }
            }
        }
        return match;
    }

    // Changes the color of the match track and/or draws match line
    // NO KNOWN BUGS
    void HighlightMatchTrack()
    {
        bool DEHIGHLIGHTMATCHTRACK = false;
        CreateLineTrack();
        foreach (Vector2 coordinate in matchTrack)
        {
            int i = (int)coordinate.x;
            int j = (int)coordinate.y;
            if (DEHIGHLIGHTMATCHTRACK)
            {
                // Debug.Log("Now highlighting this block: " + i + ", " + j);
            }
            if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().sr.color == edgeColor)
            {
                // gridObjects[i, j].GetComponent<GridpieceController>().blockColor = -2;
                // gridObjects[i, j].GetComponent<GridpieceController>().sr.color = Color.black;
            }
        }
        line.positionCount = (lineTrack.Count);
        if (DEHIGHLIGHTMATCHTRACK)
        {
            // Debug.Log("Number of positions in line renderer: " + line.numPositions);
        }
        for (int z = 1; z < lineTrack.Count; z++)
        {
            if (DEHIGHLIGHTMATCHTRACK)
            {
                // Debug.Log("Current position in loop is " + z);
            }
            Transform target1 = gridObjects[(int)lineTrack[z].x, (int)lineTrack[z].y].GetComponent<Transform>();
            Transform target2 = gridObjects[(int)lineTrack[z - 1].x, (int)lineTrack[z - 1].y].GetComponent<Transform>();
            // Draw the actual line
            if (DEHIGHLIGHTMATCHTRACK)
            {
                Debug.Log("Now drawing line between " + (int)lineTrack[z].x + ", " + (int)lineTrack[z].y + " and " + (int)lineTrack[z - 1].x + ", " + (int)lineTrack[z - 1].y);
            }
            DrawLine(target1.position, target2.position, Color.white, z);
        }
    }

    // Draw a line
    // NO KNOWN BUGS
    void DrawLine(Vector3 start, Vector3 end, Color color, int position, float duration = 0.2f)
    {
        bool DEDRAWLINE = false;
        if (DEDRAWLINE)
        {
            Debug.Log("Two positions we are setting are " + position + ", " + (position - 1));
            Debug.Log("Drawing a line between " + start + " and " + end);
        }
        line.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive (Soft)"));
        line.startColor = color;
        line.endColor = color;
        line.startWidth = 0.4f;
        line.endWidth = 0.4f;
        line.SetPosition(position, start);
        line.SetPosition(position - 1, end);
    }

    // Changes back the match track to default color
    // NO KNOWN BUGS
    void UnhighlightMatchTrack()
    {
        // Remove the line
        line.positionCount = 0;
        lineTrack.Clear();
        for (int i = 0; i < gridSize.x + extraX; i++)
        {
            for (int j = 0; j < gridSize.y + extraY; j++)
            {
                if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().blockColor == -2)
                {
                    gridObjects[i, j].GetComponent<GridpieceController>().blockColor = 0;
                    gridObjects[i, j].GetComponent<GridpieceController>().sr.color = edgeColor;
                }
            }
        }
    }

    // Remove all -1's from the board
    // NO KNOWN BUGS
    void MatchTrailCleanup()
    {
        for (int i = 0; i < gridSize.x + extraX; i++)
        {
            for (int j = 0; j < gridSize.y + extraY; j++)
            {
                if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().blockColor == -1)
                {
                    gridObjects[i, j].GetComponent<GridpieceController>().blockColor = 0;
                }
            }
        }
    }

    void RememberNeighbors(GridpieceController gpc)
    {
        bool DEREMEMBERNEIGHBORS = false;
        int x = gpc.dimX;
        int y = gpc.dimY;
        int size = gpc.size;
        if (DEREMEMBERNEIGHBORS)
        {
            Debug.Log("Now adding neighbors for " + x + ", " + y);
        }
        // Add adjacent pieces to nextToThis list
        // For ONExONE
        if (size == GridpieceController.ONExONE)
        {
            // Check left
            if (gridObjects[x - 1, y] && gridObjects[x - 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y]);
            }
            // Check right
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y]);
            }
            // Check up
            if (gridObjects[x, y + 1] && gridObjects[x, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y + 1]);
            }
            // Check down
            if (y > 1 && gridObjects[x, y - 1] && gridObjects[x, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y - 1]);
            }
        }
        else if (size == GridpieceController.ONExTWO)
        {
            // Check top left
            if (gridObjects[x - 1, y] && gridObjects[x - 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y]);
            }
            // Check top right
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y]);
            }
            // Check bottom left
            if (gridObjects[x - 1, y - 1] && gridObjects[x - 1, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y - 1]);
            }
            // Check bottom right
            if (gridObjects[x + 1, y - 1] && gridObjects[x + 1, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y - 1]);
            }
            // Check up
            if (gridObjects[x, y + 1] && gridObjects[x, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y + 1]);
            }
            // Check down
            if (y > 1 && gridObjects[x, y - 2] && gridObjects[x, y - 2].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y - 2]);
            }
        }
        else if (size == GridpieceController.TWOxONE)
        {
            // Check top left
            if (gridObjects[x - 1, y + 1] && gridObjects[x - 1, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y + 1]);
            }
            // Check top right
            if (gridObjects[x, y + 1] && gridObjects[x, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a top right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y + 1]);
            }
            // Check bottom left
            if (y > 1 && gridObjects[x - 1, y - 1] && gridObjects[x - 1, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y - 1]);
            }
            // Check bottom right
            if (y > 1 && gridObjects[x, y - 1] && gridObjects[x, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a bottom right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y - 1]);
            }
            // Check right
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a right neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y]);
            }
            // Check left
            if (gridObjects[x - 2, y] && gridObjects[x - 2, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a left neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 2, y]);
            }
        }
        else if (size == GridpieceController.TWOxTWO)
        {
            // Check up from NE
            if (gridObjects[x, y + 1] && gridObjects[x, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added an up NE neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y + 1]);
            }
            // Check right from NE
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a right NE neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y]);
            }
            // Check right from SE
            if (y > 1 && gridObjects[x + 1, y - 1] && gridObjects[x + 1, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a right SE neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x + 1, y - 1]);
            }
            // Check down from SE
            if (y > 2 && gridObjects[x, y - 2] && gridObjects[x, y - 2].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a down SE neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x, y - 2]);
            }
            // Check down from SW
            if (y > 2 && gridObjects[x - 1, y - 2] && gridObjects[x - 1, y - 2].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a down SW neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y - 2]);
            }
            // Check left from SW
            if (y > 1 && gridObjects[x - 2, y - 1] && gridObjects[x - 2, y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a left SW neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 2, y - 1]);
            }
            // Check left from NW
            if (gridObjects[x - 2, y] && gridObjects[x - 2, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added a left NW neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 2, y]);
            }
            // Check up from NW
            if (gridObjects[x - 1, y + 1] && gridObjects[x - 1, y + 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMEMBERNEIGHBORS)
                {
                    Debug.Log("Added an up NW neighbor");
                }
                gpc.nextToThis.Add(gridObjects[x - 1, y + 1]);
            }
        }
    }

    // Move all blocks that can be moved down as far as possible
    // Keep track of which blocks moved so that we can combo later
    // NO KNOWN BUGS
    void MovePiecesDown()
    {
        bool DEMOVEPIECESDOWN = false;
        if (DEMOVEPIECESDOWN){
            Debug.Log("-------------------------- Moving Pieces Down ----------------------------");
        }
        movedObjects.Clear();
        UnhighlightMatchTrack();
        // First remove all the grey pieces in the playable grid
        for (int i = 1; i < gridSize.x + extraX; i++) 
        {
            for (int j = 1; j < gridSize.y + extraY; j++) 
            {
                if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().sr.color == edgeColor)
                {
                    // First get rid of the piece
                    RemovePieceAtPosition(i, j);
                }
            }
        }
        // Get all neighbors before we move any of the pieces
        for (int i = 1; i < gridSize.x + extraX; i++) 
        {
            // We start at y = 2 because you can't move down if y = 1
            for (int j = 2; j < gridSize.y + extraY; j++) 
            {
                if (gridObjects[i, j]) {
                    RememberNeighbors(gridObjects[i, j].GetComponent<GridpieceController>());
                }
            }
        }
        // Once we have the neighbors, move pieces down
        // Iterate through the grid
        for (int z = 0; z < 3; z++)
        {
            for (int i = 1; i < gridSize.x + extraX; i++) 
            {
                // We start at y = 2 because you can't move down if y = 1
                for (int j = 2; j < gridSize.y + extraY; j++) 
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
            // If nothing moved this iteration reset the combo count
            /*
            if (movedObjects.Count == 0)
            {
                combos = 1;
            }
            */
            if (DEMOVEPIECESDOWN)
            {
                Debug.Log(movedObjects.Count + " game pieces moved in this move");
            }
        }
        if (DEMOVEPIECESDOWN)
        {
            Debug.Log(movedObjects.Count + " game pieces just moved");
        }
        // Finally, fill the board back up with grey pieces
        for (int i = 1; i < gridSize.x + extraX; i++) 
        {
            for (int j = 1; j < gridSize.y + extraY; j++) 
            {
                if (gridObjects[i, j] == null)
                {
                    AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                }
            }
        }
        processingCounter = 0;
        if (movedObjects.Count == 0)
        {
            startCounting = false;
        }
        // ProcessCombos();
    }

    // Check if the given block is the same blockColor as the block in the given direction
    // Directions:
    // 0 is left
    // 1 is right
    // 2 is down
    public bool CheckDirection(int x, int y, int direction)
    {
        bool DECHECKDIRECTION = false;
        int blockColor = gridObjects[x, y].GetComponent<GridpieceController>().blockColor;
        bool combo = false;
        List<GameObject> neighbors = gridObjects[x, y].GetComponent<GridpieceController>().nextToThis;
        if (DECHECKDIRECTION)
        {
            // Debug.Log("Now processing a block of blockColor " + blockColor + " at coordinates " + x + ", " + y);
        }
        // Check left
        if (direction == 0)
        {
            if (DECHECKDIRECTION)
            {
                // Debug.Log("LEFT check: " + (x - 1) + ", " + y + " and " + x + ", " + y);
            }
            if (gridObjects[x - 1, y] && gridObjects[x - 1, y].GetComponent<GridpieceController>().blockColor == blockColor)
            {
                // It can only be a combo if the block to the left is not in the list
                int id = gridObjects[x - 1, y].GetComponent<GridpieceController>().blockId;
                bool wasNeighbor = false;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (neighbors[i] && neighbors[i].GetComponent<GridpieceController>().blockId == id)
                    {
                        wasNeighbor = true;
                    }
                }
                if (!wasNeighbor)
                {
                    if (DECHECKDIRECTION)
                    {
                        Debug.Log("THESE TWO MATCH! " + (x - 1) + ", " + y + " and " + x + ", " + y);
                        Debug.Log("How many neighbors does this block have? " + neighbors.Count);
                        Debug.Log("Id left block: " + id);
                        for (int i = 0; i < neighbors.Count; i++)
                        {
                            // Debug.Log("Id of old neighbor: " + neighbors[i].GetComponent<GridpieceController>().blockId);
                        }
                    }
                    combo = true;
                    if (!recentlyDestroyed.Contains(id))
                    {
                        comboScore += ScoreBlock(x - 1, y);
                        recentlyDestroyed.Add(id);
                        //gridObjects[x - 1, y].GetComponent<GridpieceController>().Explode(gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color);
                        if (gridObjects[x - 1, y].GetComponent<GridpieceController>().size == GridpieceController.ONExONE)
                        {
                            gridObjects[x - 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color, 1, combos);
                        }
                        else if (gridObjects[x - 1, y].GetComponent<GridpieceController>().size == GridpieceController.TWOxTWO)
                        {
                            gridObjects[x - 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color, 4, combos);
                        }
                        else 
                        {
                            gridObjects[x - 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color, 2, combos);
                        }
                    }
                    
                    int blockSize = gridObjects[x - 1, y].GetComponent<GridpieceController>().size;
                    Color color = gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color;
                    int colorNum = gridObjects[x - 1, y].GetComponent<GridpieceController>().blockColor;
                    int blockType = gridObjects[x - 1, y].GetComponent<GridpieceController>().blockType;
					gridObjects[x - 1, y].GetComponent<GridpieceController>().ShockWave(color , 3);
                    ActivateSpecial(x - 1, y, blockSize, color, blockType, colorNum);
                    gridObjects[x - 1, y].GetComponent<GridpieceController>().sr.color = edgeColor;
                }
            }
        }
        // Check right
        else if (direction == 1)
        {
            if (DECHECKDIRECTION)
            {
                // Debug.Log("RIGHT check: " + (x + 1) + ", " + y + " and " + x + ", " + y);
            }
            if (gridObjects[x + 1, y] && gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor == blockColor)
            {
                // It can only be a combo if the block to the left is not in the list
                int id = gridObjects[x + 1, y].GetComponent<GridpieceController>().blockId;
                bool wasNeighbor = false;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (neighbors[i] && neighbors[i].GetComponent<GridpieceController>().blockId == id)
                    {
                        wasNeighbor = true;
                    }
                }
                if (!wasNeighbor)
                {
                    if (DECHECKDIRECTION)
                    {
                        Debug.Log("THESE TWO MATCH! " + (x + 1) + ", " + y + " and " + x + ", " + y);
                        Debug.Log("How many neighbors does this block have? " + neighbors.Count);
                        Debug.Log("Id right block: " + id);
                        for (int i = 0; i < neighbors.Count; i++)
                        {
                            // Debug.Log("Id of old neighbor: " + neighbors[i].GetComponent<GridpieceController>().blockId);
                        }
                    }
                    combo = true;
                    if (!recentlyDestroyed.Contains(id))
                    {
                        comboScore += ScoreBlock(x + 1, y);
                        recentlyDestroyed.Add(id);
                        //gridObjects[x + 1, y].GetComponent<GridpieceController>().Explode(gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color);
                        if (gridObjects[x + 1, y].GetComponent<GridpieceController>().size == GridpieceController.ONExONE)
                        {
                            gridObjects[x + 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color, 1, combos);
                        }
                        else if (gridObjects[x + 1, y].GetComponent<GridpieceController>().size == GridpieceController.TWOxTWO)
                        {
                            gridObjects[x + 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color, 4, combos);
                        }
                        else 
                        {
                            gridObjects[x + 1, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color, 2, combos);
                        }
                    }
                    int blockSize = gridObjects[x - 1, y].GetComponent<GridpieceController>().size;
                    Color color = gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color;
                    int colorNum = gridObjects[x + 1, y].GetComponent<GridpieceController>().blockColor;
                    int blockType = gridObjects[x + 1, y].GetComponent<GridpieceController>().blockType;
					gridObjects[x + 1, y].GetComponent<GridpieceController>().ShockWave(color , 3);
                    ActivateSpecial(x + 1, y, blockSize, color, blockType, colorNum);
                    gridObjects[x + 1, y].GetComponent<GridpieceController>().sr.color = edgeColor;
                }
            }
        }
        // Check down
        else if (direction == 2)
        {
            if (y > 1)
            {
                
                if (DECHECKDIRECTION)
                {
                    // Debug.Log("DOWN check: " + x + ", " + (y - 1) + " and " + x + ", " + y);
                }
                if (gridObjects[x, y - 1] && gridObjects[x, y - 1].GetComponent<GridpieceController>().blockColor == blockColor)
                {
                    if (DECHECKDIRECTION)
                    {
                        Debug.Log("Checking " + x + ", " + (y - 1) + " and " + x + ", " + y);
                    }
                    // It can only be a combo if the block to the left is not in the list
                    int id = gridObjects[x, y - 1].GetComponent<GridpieceController>().blockId;
                    bool wasNeighbor = false;
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        if (neighbors[i] && neighbors[i].GetComponent<GridpieceController>().blockId == id)
                        {
                            wasNeighbor = true;
                        }
                    }
                    if (!wasNeighbor)
                    {
                        if (DECHECKDIRECTION)
                        {
                            Debug.Log("THESE TWO MATCH! " + x + ", " + (y - 1) + " and " + x + ", " + y);
                            Debug.Log("How many neighbors does this block have? " + neighbors.Count);
                            Debug.Log("Id bottom block: " + id);
                            for (int i = 0; i < neighbors.Count; i++)
                            {
                                // Debug.Log("Id of old neighbor: " + neighbors[i].GetComponent<GridpieceController>().blockId);
                            }
                        }
                        combo = true;
                        if (!recentlyDestroyed.Contains(id))
                        {
                            comboScore += ScoreBlock(x, y - 1);
                            recentlyDestroyed.Add(id);
                            //gridObjects[x, y - 1].GetComponent<GridpieceController>().Explode(gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color);
                            if (gridObjects[x, y - 1].GetComponent<GridpieceController>().size == GridpieceController.ONExONE)
                            {
                                gridObjects[x, y - 1].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color, 1, combos);
                            }
                            else if (gridObjects[x, y - 1].GetComponent<GridpieceController>().size == GridpieceController.TWOxTWO)
                            {
                                gridObjects[x, y - 1].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color, 4, combos);
                            }
                            else 
                            {
                                gridObjects[x, y - 1].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color, 2, combos);
                            }
                        }
                        int blockSize = gridObjects[x - 1, y].GetComponent<GridpieceController>().size;
                        Color color = gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color;
                        int blockType = gridObjects[x, y - 1].GetComponent<GridpieceController>().blockType;
                        gridObjects[x, y - 1].GetComponent<GridpieceController>().ShockWave(color , 3);
                        ActivateSpecial(x, y - 1, blockSize, color, blockType, blockColor);
                        gridObjects[x, y - 1].GetComponent<GridpieceController>().sr.color = edgeColor;
                    }
                }
            }
        }
        // UpdateScore();
        return combo;
    }

    // Process the combos
    // NO KNOWN BUGS
    public void ProcessCombos()
    {
        bool DEPROCESSCOMBOS = false;
        CheckHorizontalSpecial();
        if (DEPROCESSCOMBOS)
        {
            Debug.Log("--------------------- NOW PROCESSING COMBOS ---------------------");
            Debug.Log("How many items are in movedObjects?: " + movedObjects.Count);
        }
        // Clear recently destroyed list
        recentlyDestroyed.Clear();
        // First move items from movedObjects to objectsToProcess
        objectsToProcess.Clear();
        for (int i = 0; i < movedObjects.Count; i++)
        {
            if (whiteCombo)
            {
                // White blocks can combo
                if (movedObjects[i])
                {
                    objectsToProcess.Add(movedObjects[i]);
                }
            }
            else
            {
                // White blocks cannot combo
                if (movedObjects[i] && movedObjects[i].GetComponent<GridpieceController>().blockColor != GridpieceController.WHITE)
                {
                    objectsToProcess.Add(movedObjects[i]);
                }
            }
        }
        movedObjects.Clear();
        if (DEPROCESSCOMBOS)
        {
            Debug.Log("How many items are in objectsToProcess?: " + objectsToProcess.Count);
        }
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
            if (objectsToProcess[i])
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
                            CheckDirection(x, y, 2) ; // down
                }
                else if (size == GridpieceController.ONExTWO)
                {
                    Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                    combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 0) | // left from top
                            CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from top
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 0) | // left from bottom
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 1) | // right from bottom
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 2) ; // down from bottom
                }
                else if (size == GridpieceController.TWOxONE)
                {
                    Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                    combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from right
                            CheckDirection((int)locations[0].x, (int)locations[0].y, 2) | // down from right
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 0) | // left from left
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 2) ; // down from left
                }
                else if (size == GridpieceController.TWOxTWO)
                {
                    Vector2[] locations = gridObjects[x, y].GetComponent<GridpieceController>().GetPositions();
                    combo = CheckDirection((int)locations[0].x, (int)locations[0].y, 1) | // right from upright
                            CheckDirection((int)locations[2].x, (int)locations[2].y, 1) | // right from downright
                            CheckDirection((int)locations[2].x, (int)locations[2].y, 2) | // down from downright
                            CheckDirection((int)locations[3].x, (int)locations[3].y, 2) | // down from downleft
                            CheckDirection((int)locations[3].x, (int)locations[3].y, 0) | // left from downleft
                            CheckDirection((int)locations[1].x, (int)locations[1].y, 0) ; // left from upleft
                }
                // If any of the blocks comboed with the main block remove it too
                if (combo)
                {
                    // If anything has comboed keep track of it
                    anyCombo = true;
                    // gridObjects[x, y].GetComponent<GridpieceController>().blockColor = 0;
                if (!recentlyDestroyed.Contains(gridObjects[x, y].GetComponent<GridpieceController>().blockId) && gridObjects[x, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
                    {
                        ScoreBlock(x, y);
                        recentlyDestroyed.Add(gridObjects[x, y].GetComponent<GridpieceController>().blockId);
                        //gridObjects[x, y].GetComponent<GridpieceController>().Explode(gridObjects[x, y].GetComponent<GridpieceController>().sr.color);
                        if (gridObjects[x, y].GetComponent<GridpieceController>().size == GridpieceController.ONExONE)
                        {
                            gridObjects[x, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y].GetComponent<GridpieceController>().sr.color, 1, combos);     
                        }
                        else if (gridObjects[x, y].GetComponent<GridpieceController>().size == GridpieceController.TWOxTWO)
                        {
                            gridObjects[x, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y].GetComponent<GridpieceController>().sr.color, 4, combos);
                        }
                        else 
                        {
                            gridObjects[x, y].GetComponent<GridpieceController>().ScoreExplode(gridObjects[x, y].GetComponent<GridpieceController>().sr.color, 2, combos);
                        }
                    }
                    int blockSize = gridObjects[x, y].GetComponent<GridpieceController>().size;
                    Color color = gridObjects[x, y].GetComponent<GridpieceController>().sr.color;
                    int colorNum = gridObjects[x, y].GetComponent<GridpieceController>().blockColor;
                    int blockType = gridObjects[x, y].GetComponent<GridpieceController>().blockType;
                    this.GetComponent<AudioSourceController>().PlayNote(lastNote);
                    gridObjects[x, y].GetComponent<GridpieceController>().ShockWave(color , 3);
                    ActivateSpecial(x, y, blockSize, color, blockType, colorNum);
                    gridObjects[x, y].GetComponent<GridpieceController>().sr.color = edgeColor;
                    // Here is where we will do userfeedback stuff for combo
                    // DisplayComboScore(val)
                }
                else
                {
                    if (DEPROCESSCOMBOS)
                    {
                        Debug.Log("There was no combo this loop");
                    }
                }
            }
        }
        objectsToProcess.Clear();
        if (anyCombo)
        {
            if (DEPROCESSCOMBOS)
            {
                Debug.Log("Anycombo is true!");
            }
            // Refresh the board
            // Remove all the grey pieces in the playable grid
            for (int i = 1; i < gridSize.x + extraX; i++) 
            {
                for (int j = 1; j < gridSize.y + extraY; j++) 
                {
                    if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().sr.color == edgeColor)
                    {
                        // First get rid of the piece
                        RemovePieceAtPosition(i, j);
                    }
                }
            }
            // Finally, fill the board back up with grey pieces
            for (int i = 1; i < gridSize.x + extraX; i++) 
            {
                for (int j = 1; j < gridSize.y + extraY; j++) 
                {
                    if (gridObjects[i, j] == null)
                    {
                        AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                    }
                }
            }
            // If combo is less than 10, increment
            if (combos < 10)
            {
                combos++;
                lastNote++;
                if (DECOMBOS)
                {
                    Debug.Log("Combos incrimented in PROCESSCOMBOS. COMBO IS: " + combos);
                }
            }
            processingCounter = 0;
            // MovePiecesDown();
        }
        // If there were no combos we return the combo multiplier to 0
        else
        {
            if (DEPROCESSCOMBOS)
            {
                Debug.Log("Anycombo is false!");
            }
            combos = 1;
            if (DEPROCESSCOMBOS)
            {
                Debug.Log("Combos is: " + combos);
            }
        }
        for (int i = 1; i < gridSize.x + extraX; i++) 
        {
            for (int j = 1; j < gridSize.y + extraY; j++) 
            {
                if (gridObjects[i, j])
                {
                    gridObjects[i, j].GetComponent<GridpieceController>().nextToThis.Clear();
                }
            }
        }
    }

    // Go through board and check to see if any horizontal pieces are in same row
    //
    void CheckHorizontalSpecial()
    {
        bool DECHECKHORIZONTALSPECIAL = true;
        int firstX = -1;
        int secondX = -1;
        if (DECHECKHORIZONTALSPECIAL)
        {
            Debug.Log("NOW CHECKING HORIZONTAL SPECIAL");
        }
        // go through the grid one row at a time trying to find horizontal clear block
        for (int i = 1; i < gridSize.x + extraX; i++)
        {
            for (int j = 1; j < gridSize.y + extraY; j++)
            {
                // if we find a piece start from other side trying to find a second piece
                if (gridObjects[i, j] && gridObjects[i, j].GetComponent<GridpieceController>().blockType == GridpieceController.HORIZ_CLEAR_BLOCK)
                {
                    firstX = i;
                    for (int k = (int)gridSize.x + extraX - 1; k > i; k--)
                    {
                        if (gridObjects[k, j] && gridObjects[k, j].GetComponent<GridpieceController>().blockType == GridpieceController.HORIZ_CLEAR_BLOCK)
                        {
                            secondX = k;
                            // now that we have both points we want to delete from, remove relevant part of row
                            if (DECHECKHORIZONTALSPECIAL)
                            {
                                Debug.Log("now calling RemoveRow from coordinates " + firstX + ", " + j + " to "+ secondX + ", " + j);
                            }
                            RemoveRow(firstX, secondX, j);
                        }
                    }
                }
            }
        }
    }

    // Activate special blocks as they are activated
    // mark v
    void ActivateSpecial(int x, int y, int blockSize, Color color, int blockType, int colorNum)
    {
        ActivateMatchSpecial(x, y, color, blockType, colorNum);
        if (blockSize == GridpieceController.ONExONE)
        {
            // CORRECT
            ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // EAST
            ActivateAdjacentSpecial(x - 1, y, color, colorNum);     // WEST
            ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTH
            ActivateAdjacentSpecial(x, y - 1, color, colorNum);     // SOUTH
        }
        else if (blockSize == GridpieceController.ONExTWO)
        {
            // CORRECT
            ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // NORTH EAST
            ActivateAdjacentSpecial(x - 1, y, color, colorNum);     // NORTH WEST
            ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTH NORTH
            ActivateAdjacentSpecial(x + 1, y - 1, color, colorNum); // SOUTH EAST
            ActivateAdjacentSpecial(x - 1, y - 1, color, colorNum); // SOUTH WEST
            ActivateAdjacentSpecial(x, y - 2, color, colorNum);     // SOUTH SOUTH
        }
        else if (blockSize == GridpieceController.TWOxONE)
        {
            // CORRECT
            ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // EAST EAST
            ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // EAST NORTH
            ActivateAdjacentSpecial(x, y - 1, color, colorNum);     // EAST SOUTH
            ActivateAdjacentSpecial(x - 2, y, color, colorNum);     // WEST WEST
            ActivateAdjacentSpecial(x - 1, y + 1, color, colorNum); // WEST NORTH
            ActivateAdjacentSpecial(x - 1, y - 1, color, colorNum); // WEST SOUTH
        }
        else if (blockSize == GridpieceController.TWOxTWO)
        {
            // CORRECT
            ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // NORTHEAST EAST
            ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTHEAST NORTH
            ActivateAdjacentSpecial(x - 1, y + 1, color, colorNum); // NORTHWEST NORTH
            ActivateAdjacentSpecial(x - 2, y, color, colorNum);     // NORTHWEST WEST
            ActivateAdjacentSpecial(x - 2, y - 1, color, colorNum); // SOUTHWEST WEST 
            ActivateAdjacentSpecial(x - 1, y - 2, color, colorNum); // SOUTHWEST SOUTH 
            ActivateAdjacentSpecial(x, y - 2, color, colorNum);     // SOUTHEAST SOUTH
            ActivateAdjacentSpecial(x + 1, y - 1, color, colorNum); // SOUTHEAST EAST
        }
    }

    // Removes given block and activates it
    /*
    TYPES OF SPECIAL BLOCKS
    - 0: normal
    - 1: remove all blocks of one color
    - 2: remove a column
    - 3: remove a row
    - 4: remove a column and a row
    */
    // Activate the special block if it was used in a match or a combo
    // NO KNOWN BUGS
    void ActivateMatchSpecial(int x, int y, Color color, int blockType, int colorNum)
    {
        bool DEACTIVATEMATCHSPECIAL = false;
        if (x > 0 && x < gridSize.x + extraX && y > 0 && y < gridSize.y + extraY)
        {
            if (gridObjects[x, y])
            {
                // remove all blocks in this column
                if (blockType == GridpieceController.VERT_CLEAR_BLOCK)
                {
                    if (DEACTIVATEMATCHSPECIAL)
                    {
                        Debug.Log("VERTICAL clear block activated");
                    }
                    RemoveColumn(x);
                }
                // remove all blocks in this row and column
                else if (blockType == GridpieceController.PLUS_CLEAR_BLOCK)
                {
                    if (DEACTIVATEMATCHSPECIAL)
                    {
                        Debug.Log("PLUS clear block activated");
                    }
                    RemoveRowAndColumn(x, y);
                }
                else if (blockType == GridpieceController.CLOCK_BLOCK)
                {
                    if (DEACTIVATEMATCHSPECIAL)
                    {
                        Debug.Log("STOP TIME block activated");
                    }
                    StopTime(x, y, color);
                }
                else if (blockType == GridpieceController.BOMB_BLOCK)
                {
                    if (DEACTIVATEMATCHSPECIAL)
                    {
                        Debug.Log("BOMB block activated");
                    }
                    BombGoBoom(x, y, color);
                }
            }
        }
    }

    // Remove special blocks that were next to the matches or combos
    // NO KNOWN BUGS
    void ActivateAdjacentSpecial(int x, int y, Color color, int colorNum)
    {
        bool DEACTIVATEADJACENTSPECIAL = false;
        if (DEACTIVATEADJACENTSPECIAL)
        {
            Debug.Log("Block coordinates are: " + x + ", " + y);
        }
        if (x > 0 && x < gridSize.x + extraX && y > 0 && y < gridSize.y + extraY)
        {
            if (gridObjects[x, y])
            {
                // remove all blocks of one color
                if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.REMOVE_ONE_COLOR_BLOCK)
                {
                    if (colorNum != GridpieceController.WHITE)
                    {
                        RemoveOneColor(x, y, color);
                    }
                }
                // Colorwash the board
                else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.COLORWASH_BLOCK)
                {
                    if (colorNum != GridpieceController.WHITE)
                    {
                        PaintOneColor(x, y, color, colorNum);
                    }
                }
                // THIS BLOCK HAS NO EFFECT ANYWAY
                // remove the sad block
                else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.WASTE_OF_SPACE_BLOCK)
                {
                    SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 3, false);
                }
                // REMOVE THE BAD BLOCKS WITH NO EFFECT IF WE MATCH NEXT TO THEM IN TIME
                // remove row up block
                else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.UP_BLOCK)
                {
                    SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 3, false);
                }
                // remove angry block
                else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.WHITEWASH_BLOCK)
                {
                    SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 3, false);
                }
            }
        }
    }

    public void ActivateTimerSpecial(int x, int y)
    {
        bool DEACTIVATETIMERSPECIAL = false;
        if (x > 0 && x < gridSize.x + extraX && y > 0 && y < gridSize.y + extraY)
        {
            // Adds another row
			if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.UP_BLOCK) {
				if (DEACTIVATETIMERSPECIAL) {
					Debug.Log("Removed the UP block");
				}
				SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 5, false);
                MovePiecesDown();
				AddRow(-1, false);
			}
            // Whitewash the board
            else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.WHITEWASH_BLOCK) {
				if (DEACTIVATETIMERSPECIAL) {
					Debug.Log("Removed the WHITEWASH block");
				}
				SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 21, false);
				Whiteout();
			}
			else if (gridObjects[x, y].GetComponent<GridpieceController>().blockType == GridpieceController.BUBBLES_BLOCK) {
				if (DEACTIVATETIMERSPECIAL) {
					Debug.Log("Removed the BUBBLE block");
					SoftRemovePieceAtPosition(x, y, GridpieceController.ONExONE, 21, false);

					//TODO: What should happen for the bubble block?
				}
			
			}
        }
        // Make sure to move all the pieces after the block disappears
        MovePiecesDown();
    }

    // Removes row of highlighted piece
    // NO KNOWN BUGS
    void RemoveRow(int x1, int x2, int y)
    {
        // Save currentPiece and lastPiece and restore them after
        Vector2 iCurrentPiece = currentPiece;
        Vector2 iLastPiece = lastPiece;
        bool DEREMOVEROW = true;
        if (DEREMOVEROW)
        {
            Debug.Log("Now REMOVINGROW from coordinates " + x1 + ", " + y + " to "+ x2 + ", " + y);
        }
        // Turn all objects in the row gray and empty
        for (int x = x1; x <= x2; x++)
        {
            if (gridObjects[x, y].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
            {
                if (DEREMOVEROW)
                {
                    Debug.Log("Now Removing block " + x + ", " + y + " " + (gridObjects[x, y].GetComponent<GridpieceController>().blockColor));
                }
                SoftRemovePieceAtPosition(x, y, gridObjects[x, y].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        matchTrack = new List<Vector2>();
        currentPiece = new Vector2(x1, y);
        lastPiece = new Vector2(x2, y);
        HighlightMatchTrack();
        startCounting = true;
        // Restore currentPiece and lastPiece
        currentPiece = iCurrentPiece;
        lastPiece = iLastPiece;
        /*
        combos++;
        if (DECOMBOS)
        {
            Debug.Log("Combos incrimented in REMOVEROW. COMBO IS: " + combos);
        }
        */
    }

    // Removes column of highlighted piece
    // NO KNOWN BUGS
    void RemoveColumn(int x)
    {
        // Turn all objects in the row gray and empty
        for (int y = 1; y < gridSize.y + extraY; y++)
        {
            if (gridObjects[x, y].GetComponent<GridpieceController>().sr.color != edgeColor)
            {
                SoftRemovePieceAtPosition(x, y, gridObjects[x, y].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        startCounting = true;
        combos++;
        if (DECOMBOS)
        {
            Debug.Log("Combos incrimented in REMOVECOLUMN. COMBO IS: " + combos);
        }
    }

    // Removes both row and column of highlighted piece
    // NO KNOWN BUGS
    void RemoveRowAndColumn(int i, int j)
    {
        // Turn all objects in the column gray and empty
        for (int x = 1; x < gridSize.x + extraX; x++)
        {
            if (gridObjects[x, j].GetComponent<GridpieceController>().sr.color != edgeColor)
            {
                SoftRemovePieceAtPosition(x, j, gridObjects[x, j].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        // Turn all objects in the row gray and empty
        for (int y = 1; y < gridSize.y + extraY; y++)
        {
            if (gridObjects[i, y].GetComponent<GridpieceController>().sr.color != edgeColor)
            {
                SoftRemovePieceAtPosition(i, y, gridObjects[i, y].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        startCounting = true;
        combos++;
        if (DECOMBOS)
        {
            Debug.Log("Combos incrimented in REMOVEROWANDCOLUMN. COMBO IS: " + combos);
        }
    }

    // Remove all blocks that are the color of the matched block
    // NO KNOWN BUGS
    void RemoveOneColor(int x, int y, Color color)
    {
        if (gridObjects[x, y])
        {
            GridpieceController gpc = gridObjects[x, y].GetComponent<GridpieceController>();
            gpc.Colorwash(edgeColor, true);
            gpc.ShockWave(color, 21);
        }
        if (color != edgeColor)
        {
            for (int i = 1; i < gridSize.x + extraX; i++)
            {
                for (int j = 1; j < gridSize.y + extraY; j++)
                {
                    if (color == gridObjects[i, j].GetComponent<GridpieceController>().sr.color)
                    {
                        SoftRemovePieceAtPosition(i, j, gridObjects[i, j].GetComponent<GridpieceController>().size, 3, false);
                    }
                }
            }
            startCounting = true;
            combos++;
            if (DECOMBOS)
            {
                Debug.Log("Combos incrimented in REMOVEONECOLOR. COMBO IS: " + combos);
            }
        }
    }

    // Changes all blocks on the board to white, vanilla blocks
    // DOES NOT CHANGE THE SPRITE TYPE BACK TO ORIGINAL
    void Whiteout()
    {
        bool DEWHITEOUT = false;
        whiteOut = true;
        whiteOutCounter = 0;
        whiteOutTimer = newLineInterval;
        if (DEWHITEOUT)
        {
            Debug.Log("Are we whited out?: " + whiteOut);
        }
        for (int i = 1; i < gridSize.x + extraX; i++)
        {
            for (int j = 1; j < gridSize.y + extraY; j++)
            {
                GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
                if (gpc.blockColor != GridpieceController.EDGE)
                {
                    gpc.ClearBlockType();
                    gpc.Colorwash(Color.white, true);
                }
            }
        }
        MovePiecesDown();
    }

    // Changes all white blocks back
    // NO KNOWN BUGS
    void Repaint()
    {
        bool DEREPAINT = false;
        whiteOut = false;
        whiteOutCounter = 0;
        if (DEREPAINT)
        {
            Debug.Log("Are we whited out?: " + whiteOut);
        }
        for (int i = 1; i < gridSize.x + extraX; i++)
        {
            for (int j = 1; j < gridSize.y + extraY; j++)
            {
                GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
				if (gpc.blockColor != GridpieceController.EDGE)
                {
                    gpc.ClearBlockType();
					gpc.Repaint();
                }
            }
        }
    }

    // Detonates THE BOMB
    void BombGoBoom(int x, int y, Color color)
    {
        this.GetComponent<AudioSourceController>().PlayBombSound();
        if (gridObjects[x, y])
        {
            gridObjects[x, y].GetComponent<GridpieceController>().ShockWave(color, 6);
            SoftRemovePieceAtPosition(x, y, gridObjects[x, y].GetComponent<GridpieceController>().size, 6, false);
        }
        // Bombs cant be around the edge 
        // - 8 blocks that are immediately surrounding the bomb dont need to be checked
        // - Two up, down, left, and right do need to be checked 
        if (gridObjects[x - 1, y])
        {
            SoftRemovePieceAtPosition(x - 1, y, gridObjects[x - 1, y].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x + 1, y])
        {
            SoftRemovePieceAtPosition(x + 1, y, gridObjects[x + 1, y].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x, y - 1])
        {
            SoftRemovePieceAtPosition(x, y - 1, gridObjects[x, y - 1].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x, y + 1])
        {
            SoftRemovePieceAtPosition(x, y + 1, gridObjects[x, y + 1].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x - 1, y - 1])
        {
            SoftRemovePieceAtPosition(x - 1, y - 1, gridObjects[x - 1, y - 1].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x - 1, y + 1])
        {
            SoftRemovePieceAtPosition(x - 1, y + 1, gridObjects[x - 1, y + 1].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x + 1, y - 1])
        {
            SoftRemovePieceAtPosition(x + 1, y - 1, gridObjects[x + 1, y - 1].GetComponent<GridpieceController>().size, 3, false);
        }
        if (gridObjects[x + 1, y + 1])
        {
            SoftRemovePieceAtPosition(x + 1, y + 1, gridObjects[x + 1, y + 1].GetComponent<GridpieceController>().size, 3, false);
        }
        // Only remove the block two spaces to the left if we are to the right of column 1
        if (x > 1)
        {
            if (gridObjects[x - 2, y])
            {
                SoftRemovePieceAtPosition(x - 2, y, gridObjects[x - 2, y].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        // Only remove the block two spaces to the right if we are to the left of last playable column
        if (x < gridSize.x)
        {
            if (gridObjects[x + 2, y])
            {
                SoftRemovePieceAtPosition(x + 2, y, gridObjects[x + 2, y].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        // Only remove the block two spaces down if we are above row 1
        if (y > 1)
        {
            if (gridObjects[x, y - 2])
            {
                SoftRemovePieceAtPosition(x, y - 2, gridObjects[x, y - 2].GetComponent<GridpieceController>().size, 3, false);
            }
        }
        // Only remove the block two spaces up if we are below top playable row
        if (y < gridSize.y)
        {
            if (gridObjects[x, y + 2])
            {
                SoftRemovePieceAtPosition(x, y + 2, gridObjects[x, y + 2].GetComponent<GridpieceController>().size, 3, false);
            }
        }
    }

    // Freezes all effects, including adding new rows, block countdowns, and the whiteout clock
    // DOES NOT PAUSE SPECIAL BLOCKS THAT ARE CREATED AFTER THE FREEZE
    void StopTime(int x, int y, Color color)
    {
        this.GetComponent<AudioSourceController>().PlayFreezeSound();
        pause = true;
        pauseCounter = 0;
        pauseTimer = newLineInterval * 2;
        if (gridObjects[x, y])
        {
            gridObjects[x, y].GetComponent<GridpieceController>().ShockWave(color, 21);
        }
    }

    // Changes all blocks on the board to the color of the matched/comboed adjacent block
    // NO KNOWN BUGS
    void PaintOneColor(int x, int y, Color color, int colorNum)
    {
        if (gridObjects[x, y])
        {
            GridpieceController gpc = gridObjects[x, y].GetComponent<GridpieceController>();
            gpc.ShockWave(color, 21);
            gpc.blockType = 0;
            gpc.blockColor = 0;
            gpc.sr.color = edgeColor;
            for (int i = 1; i < gridSize.x + extraX; i++)
            {
                for (int j = 1; j < gridSize.y + extraY; j++)
                {
                    if (gridObjects[i, j].GetComponent<GridpieceController>().sr.color != edgeColor)
                    {
                        gridObjects[i, j].GetComponent<GridpieceController>().ClearBlockType();
                        gridObjects[i, j].GetComponent<GridpieceController>().hasCountdown = false;
                        gridObjects[i, j].GetComponent<GridpieceController>().sr.color = color;
                        gridObjects[i, j].GetComponent<GridpieceController>().blockColor = colorNum;
                    }
                }
            }
        }
    }

    // Checks each grid position to see if there is a blank block there
    // NO KNOWN BUGS
    int CheckPieces()
    {
        bool DECHECKPIECES = false;
        int count = 0;
        for (int i = 0; i < gridSize.x + extraX; i++) 
        {
            for (int j = 0; j < gridSize.y + extraY; j++) 
            {
                if (!gridObjects[i, j])
                {
                    if (DECHECKPIECES)
                    {
                        Debug.Log("Missing a piece at " + i + ", " + j);
                    }
                }
                else
                {
                    if (gridObjects[i, j].GetComponent<GridpieceController>().blockColor <= 0)
                    {
                        gridObjects[i, j].GetComponent<GridpieceController>().blockColor = 0;
                    }
                    count++;
                }
            }
        }
        if (DECHECKPIECES)
        {
            Debug.Log(count + " valid pieces on the game board right now");
        }
        return count;
    }

    // Adds a new row to bottom
    // NO KNOWN BUGS
	// UPDATED FOR MULTIPLE SIZES
    void AddRow(int color, bool resetTimer)
    {
		justAddedRow = true;
        bool DEADDROW = false;
        // Reset the new line counter when we add a new line
        if (resetTimer)
        {
            newLineCounter = 0;
        }
        if (DEADDROW)
        {
            Debug.Log("Are we whited out in addrow?: " + whiteOut);
        }
        // Add a row
		if (!GlobalVariables.gameOver)
        {
            // FIRST CHECK TO SEE IF WE HAVE LOST THE GAME
            CheckGameOver();
            // REMOVE ALL PLACEHOLDER PIECES FIRST
            for (int i = 1; i < gridSize.x + extraX; i++) 
            {
                // START ON j=0 BECAUSE WE WANT TO REMOVE THE BOTTOM ROW IN ORDER TO CREATE BIGGER PIECES
                // USED THE TERNARY OPERATOR TO DECIDE WHETHER TO USE BIG PIECES IN THE ROW METHOD
                for (int j = includeBigPieces ? 0 : 1; j < gridSize.y + extraY; j++) 
                {
                    if (gridObjects[i, j])
                    {
                        GridpieceController gpc = gridObjects[i, j].GetComponent<GridpieceController>();
                        if (gpc.blockColor == 0)
                        {
                            RemovePieceAtPosition(i, j);
                        }
                    }
                }
            }
            // MOVE ALL PIECES UP
            // We iterate through the grid horizontally instead of vertically for this one.
            for (int i = (int)(gridSize.y)-1; i >= 1; i--) 
            {
                for (int j = (int)(gridSize.x); j >= 1; j--) 
                {
                    if (gridObjects[j, i]) 
                    {
                        GridpieceController gpc = gridObjects[j, i].GetComponent<GridpieceController>();
                        MovePieceToPosition(gridObjects[j, i], j, i + 1);
                        // If the piece was a big piece, we skip it next time by moving the i counter over once more so it doesn't move it up twice
                        if (gpc.size == GridpieceController.TWOxTWO)
                        {
                            j--;
                        }
                    }
                }
            }
            // Bring the selected piece up
            if (currentPiece.x != -1 && currentPiece.y < gridSize.y) {
                currentPiece.y += 1;
            }
            // ADD BOTTOM ROW
            for (int i = (int)gridSize.x; i > 0; i--)
            {
                if (!includeBigPieces)
                {
                    AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                }
                else 
                {
                    float val = UnityEngine.Random.value;
                    // At shapeLevel 1, only create small blocks
                    if (shapeLevel == 1)
                    {
                        AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                    }
                    // Then we add the huge ones
                    else if (shapeLevel == 2)
                    {
                        if (val < sizeFrequencies[2])
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                        }
                        else 
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.TWOxTWO);
                        }
                    }
                    // Then we add the skinny/long blocks
                    else if (shapeLevel == 4)
                    {
                        if (val < sizeFrequencies[0])
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                        }
                        else if (val < sizeFrequencies[1])
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.ONExTWO);
                        }
                        else if (val < sizeFrequencies[2])
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.TWOxONE);
                        }
                        else
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.TWOxTWO);
                        }
                    }
                }
            }
            // Since there is sometimes a block that is missing check through and fill empty spaces
            for (int i = (int)gridSize.x; i > 0; i--)
            {
                if (!gridObjects[i, 1])
                {
                    if (shapeLevel < 4)
                    {
                        AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                    }
                    else
                    {
                        float val = UnityEngine.Random.value;
                        if (val < sizeFrequencies[2])
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.ONExONE);
                        }
                        else
                        {
                            AddPieceAtPosition(i, 1, color, GridpieceController.ONExTWO);
                        }
                    }
                }
            }
            // ADD IN ALL PLACEHOLDER PIECES AGAIN
            for (int i = 1; i < gridSize.x + extraX; i++) 
            {
                for (int j = includeBigPieces ? 0 : 1; j < gridSize.y + extraY; j++) 
                {
                    if (!gridObjects[i, j])
                    {
                        AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                    }
                }
            }
            // If we are in whiteout mode, all of the blocks should be white but keep their real color
            if (whiteOut)
            {
                for (int i = 1; i < gridSize.x + extraX; i++)
                {
                    if (gridObjects[i, 1] && gridObjects[i, 1].GetComponent<GridpieceController>().blockColor != 0)
                    {
                        gridObjects[i, 1].GetComponent<GridpieceController>().sr.color = Color.white;
                    }
                }
            }
            // CheckHorizontalSpecial();
        }
    }

    // Removes all characteristics of block at position
    // NO KNOWN BUGS
    // DOES NOT TAKE SPECIAL EFFECTS OF SPECIAL PIECES INTO ACCOUNT
    public void SoftRemovePieceAtPosition(int x, int y, int blockSize, int shockwaveSize, bool getPoints)
    {
        bool DESOFTREMOVEPIECEATPOSITION = true;
        if (gridObjects[x, y])
        {
            GridpieceController gpc = gridObjects[x, y].GetComponent<GridpieceController>();
            if (DESOFTREMOVEPIECEATPOSITION)
            {
                Debug.Log("Color of current block " + x + ", " + y + " is: " + gpc.blockColor);
            }
            if (gpc.blockColor > 0)
            {
                if (getPoints)
                {
                    ScoreBlock(x, y);
                    if (gpc.size == GridpieceController.ONExONE)
                    {
                        gpc.ScoreExplode(gpc.sr.color, 1, combos);
                    }
                    else if (gpc.size == GridpieceController.TWOxTWO)
                    {
                        gpc.ScoreExplode(gpc.sr.color, 4, combos);
                    }
                    else 
                    {
                        gpc.ScoreExplode(gpc.sr.color, 2, combos);
                    }
                }
                gpc.ShockWave(gpc.sr.color, shockwaveSize);
                gpc.blockType = 0;
                int colorNum = gpc.blockColor;
                gpc.blockColor = 0;
                Color color = gpc.sr.color;
                gpc.sr.color = edgeColor;
                if (DESOFTREMOVEPIECEATPOSITION)
                {
                    Debug.Log("Now removing piece at " + x + ", " + y);
                }
                if (blockSize == GridpieceController.ONExONE)
                {
                    // CORRECT
                    ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // EAST
                    ActivateAdjacentSpecial(x - 1, y, color, colorNum);     // WEST
                    ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTH
                    ActivateAdjacentSpecial(x, y - 1, color, colorNum);     // SOUTH
                }
                else if (blockSize == GridpieceController.ONExTWO)
                {
                    // CORRECT
                    ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // NORTH EAST
                    ActivateAdjacentSpecial(x - 1, y, color, colorNum);     // NORTH WEST
                    ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTH NORTH
                    ActivateAdjacentSpecial(x + 1, y - 1, color, colorNum); // SOUTH EAST
                    ActivateAdjacentSpecial(x - 1, y - 1, color, colorNum); // SOUTH WEST
                    ActivateAdjacentSpecial(x, y - 2, color, colorNum);     // SOUTH SOUTH
                }
                else if (blockSize == GridpieceController.TWOxONE)
                {
                    // CORRECT
                    ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // EAST EAST
                    ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // EAST NORTH
                    ActivateAdjacentSpecial(x, y - 1, color, colorNum);     // EAST SOUTH
                    ActivateAdjacentSpecial(x - 2, y, color, colorNum);     // WEST WEST
                    ActivateAdjacentSpecial(x - 1, y + 1, color, colorNum); // WEST NORTH
                    ActivateAdjacentSpecial(x - 1, y - 1, color, colorNum); // WEST SOUTH
                }
                else if (blockSize == GridpieceController.TWOxTWO)
                {
                    // CORRECT
                    ActivateAdjacentSpecial(x + 1, y, color, colorNum);     // NORTHEAST EAST
                    ActivateAdjacentSpecial(x, y + 1, color, colorNum);     // NORTHEAST NORTH
                    ActivateAdjacentSpecial(x - 1, y + 1, color, colorNum); // NORTHWEST NORTH
                    ActivateAdjacentSpecial(x - 2, y, color, colorNum);     // NORTHWEST WEST
                    ActivateAdjacentSpecial(x - 2, y - 1, color, colorNum); // SOUTHWEST WEST 
                    ActivateAdjacentSpecial(x - 1, y - 2, color, colorNum); // SOUTHWEST SOUTH 
                    ActivateAdjacentSpecial(x, y - 2, color, colorNum);     // SOUTHEAST SOUTH
                    ActivateAdjacentSpecial(x + 1, y - 1, color, colorNum); // SOUTHEAST EAST
                }
            }
            else
            {
                if (DESOFTREMOVEPIECEATPOSITION)
                {
                    Debug.Log("Did not remove the piece at " + x + ", " + y);
                }
            }
        }
    }

    // Removes any piece - UPDATED FOR MULTIPLE SIZES
    public void RemovePieceAtPosition(int x, int y) {
        bool DEREMOVEPIECEATPOSITION = false;
        //if (DEREMOVEPIECEATPOSITION)
        // Debug.Log("Removing block at space " + x + ", " + y);
		if (x < 0 || y < 0)
        {
            if (DEREMOVEPIECEATPOSITION)
            {
                Debug.Log("Warning (RemovePieceAtPosition): Attempting to remove a block outside the playgrid - nothing done");
            }
        }
        else if (gridObjects[x, y]) {
            GridpieceController gpc = gridObjects[x, y].GetComponent<GridpieceController>();
            int xPos = gpc.dimX;
            int yPos = gpc.dimY;
            // Causes explosion if block isn't grey
            if (gpc.sr.color != edgeColor)
            {
                //gpc.Explode(gpc.sr.color);

				if (gpc.size == GridpieceController.ONExONE)
                {
					// gpc.ScoreExplode(gpc.sr.color, 1, combos);
                }
				else if (gpc.size == GridpieceController.TWOxTWO)
                {
					// gpc.ScoreExplode(gpc.sr.color, 4, combos);
                }
				else 
                {
					// gpc.ScoreExplode(gpc.sr.color, 2, combos);
                }
                gpc.ShockWave(gpc.sr.color, 3);
            }
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
        else if (DEREMOVEPIECEATPOSITION)
			Debug.LogWarning("Warning (RemovePieceAtPosition): Attempting to remove a block that isn't there");
    }

    // Add a piece - UPDATED FOR MULTIPLE SIZES
    // - if the num supplied isnegative we choose a number randomly
    // - otherwise the blockColor is the num we supplied
    // - if size supplied is negative, we choose a size randomly based on what can fit in the space provided
    // - otherwise the size is the size we supplied
    // - Gives warning and does nothing if the block specified can't fit in the space provided
    public GameObject AddPieceAtPosition(int x, int y, int num, int size) {
        bool DEADDPIECEATPOSITION = false;
		if (size == GridpieceController.TWOxTWO && (gridObjects[x, y] || gridObjects[x - 1, y] || gridObjects[x, y - 1] || gridObjects[x - 1, y - 1])) {
            if (DEADDPIECEATPOSITION)
				Debug.LogWarning("Warning (AddPieceAtPosition):  Attempting to add a 2x2 block in a place where it won't fit - nothing done");
            return null;
        }
        else if (size == GridpieceController.ONExTWO && (gridObjects[x, y] || gridObjects[x, y - 1])) {
            if (DEADDPIECEATPOSITION)
				Debug.LogWarning("Warning (AddPieceAtPosition):  Attempting to add a 1x2 block in a place where it won't fit - nothing done");
            return null;
        }
        else if (size == GridpieceController.TWOxONE && (gridObjects[x, y] || gridObjects[x - 1, y])) {
            if (DEADDPIECEATPOSITION)
				Debug.LogWarning("Warning (AddPieceAtPosition):  Attempting to add a 2x1 block in a place where it won't fit - nothing done");
            return null;
        }
        else if ((size == GridpieceController.ONExONE || size < 0) && (gridObjects[x, y])) {
            if (DEADDPIECEATPOSITION)
				Debug.LogWarning("Warning (AddPieceAtPosition):  Attempting to add a 1x1 or random block in a place where it won't fit - nothing done");
            return null;
        }
        else {
            GameObject go = (GameObject)Instantiate(gridPiece, gridPositions[x, y], Quaternion.identity);
            GridpieceController gpc = go.GetComponent<GridpieceController>();
			gpc.pgc = this;
            if (num < 0) {
				gpc.blockColor = (int)Mathf.Floor(UnityEngine.Random.Range(1, (1 + colorLevel - 0.00000001f)));
            }
            else {
                gpc.blockColor = num;
            }
            gpc.SetColor();
            if (size < 0) {
				if (!gridObjects[x - 1, y] && !gridObjects[x, y - 1] && !gridObjects[x - 1, y - 1]) {
					float val = UnityEngine.Random.value;
					if (val < 0.25f)
						gpc.size = GridpieceController.ONExONE;
					else if (val < 0.5f)
						gpc.size = GridpieceController.ONExTWO;
					else if (val < 0.75f)
						gpc.size = GridpieceController.TWOxONE;
					else
						gpc.size = GridpieceController.TWOxTWO;
				}
				else if (!gridObjects[x - 1, y] && !gridObjects[x, y - 1]) {
					float val = UnityEngine.Random.value;
					if (val < 0.333333f)
						gpc.size = GridpieceController.ONExONE;
					else if (val < 0.666666f)
						gpc.size = GridpieceController.ONExTWO;
					else
						gpc.size = GridpieceController.TWOxONE;
				}
				else if (!gridObjects[x, y - 1]) {
					if (UnityEngine.Random.value < 0.5f)
						gpc.size = GridpieceController.ONExONE;
					else
						gpc.size = GridpieceController.ONExTWO;
				}
				else if (!gridObjects[x - 1, y]) {
					if (UnityEngine.Random.value < 0.5f)
						gpc.size = GridpieceController.ONExONE;
					else
						gpc.size = GridpieceController.TWOxONE;
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
            if (!whiteOut && gpc.size == GridpieceController.ONExONE) 
            {
                if (colorLevel > 5 && gpc.blockColor > 1)
                {
                    float val = UnityEngine.Random.value;
                    // 15% chance of a special block being created 
                    if (val < 0.2)
                    {
                        float type = UnityEngine.Random.value;
                        if (type < 0.4 && (includeGoodSpecialBlocks || includeBadSpecialBlocks))
                        {
                            gpc.blockType = GridpieceController.HORIZ_CLEAR_BLOCK;
                        }
                        else if (includeGoodSpecialBlocks && type < 0.45)
                        {
                            gpc.blockType = GridpieceController.REMOVE_ONE_COLOR_BLOCK;
                        }
                        else if (includeGoodSpecialBlocks && type < 0.50)
                        {
                            gpc.blockType = GridpieceController.VERT_CLEAR_BLOCK;
                        }
                        else if (includeGoodSpecialBlocks && type < 0.55)
                        {
                            gpc.blockType = GridpieceController.PLUS_CLEAR_BLOCK;
                        }
                        else if (includeBadSpecialBlocks && type < 0.60)
                        {
                            gpc.blockType = GridpieceController.UP_BLOCK;             // BAD
                        }
                        else if (includeGoodSpecialBlocks && type < 0.70)
                        {
                            gpc.blockType = GridpieceController.BOMB_BLOCK;
                        }
                        else if (includeGoodSpecialBlocks && type < 0.75)
                        {
                            gpc.blockType = GridpieceController.CLOCK_BLOCK;
                        }
                        else if (includeBadSpecialBlocks && type < 0.80)
                        {
                            gpc.blockType = GridpieceController.WHITEWASH_BLOCK;          // BAD
                        }
                        else if (includeBadSpecialBlocks && type < 0.90)
                        {
                            gpc.blockType = GridpieceController.WASTE_OF_SPACE_BLOCK;            // BAD
                        }
                        else if (includeGoodSpecialBlocks && type < 0.93)
                        {
                            gpc.blockType = GridpieceController.COLORWASH_BLOCK;
                        }
                    }
                }
            }
            else if (gpc.size == GridpieceController.ONExTWO) {
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
            //if (DEADDPIECEATPOSITION)
            //	Debug.Log("Created Piece at position: " + x + ", " + y);
            return go;
        }
    }

	public GameObject AddPieceAtPosition(int x, int y, int num, int size, int type) {
		/*
		if (num == GridpieceController.EDGE && type != 0) {
			Debug.LogWarning("Warning (AddPieceAtPosition2): attempting to make an edge piece with a special block characteristic.  Creating regular block");
			return AddPieceAtPosition(x, y, num, size);
		}
		else 
		*/
		if (type < 0 || type > GridpieceController.NUM_TYPES_SPECIAL_BLOCKS) {
			Debug.LogWarning("Warning (AddPieceAtPosition2): attempting to make a special block type that doesn't exist.  Creating regular block");
			return AddPieceAtPosition(x, y, num, size);
		}

		GameObject go = AddPieceAtPosition(x, y, num, size);
		GridpieceController gpc = go.GetComponent<GridpieceController>();
		gpc.blockType = type;
		return go;
	}

    // UPDATED FOR MULTIPLE SIZES
    public void MovePieceToPosition(GameObject piece, int x, int y) {
        bool DEMOVEPIECETOPOSITION = false;
        if (piece == null)
        {
            Debug.LogWarning("Warning (MovePieceToPosition): piece given does not exist - nothing done");
        }
        else 
        {
            GridpieceController gpc = piece.GetComponent<GridpieceController>();
            int pieceSize = gpc.size;
            if ((y < 0 || y >= gridSize.y + extraY) || ((pieceSize == GridpieceController.ONExTWO || pieceSize == GridpieceController.TWOxTWO) && y < 1))
            {
                if (DEMOVEPIECETOPOSITION)
                {
					Debug.LogWarning("Warning (MovePieceToPosition): trying to move piece below or above grid Y limits - nothing done");
                }
            }
            else if ((x < 1 || x >= gridSize.x + extraX) || ((pieceSize == GridpieceController.TWOxONE || pieceSize == GridpieceController.TWOxTWO) && x < 2))
            {
                if (DEMOVEPIECETOPOSITION)
                {
					Debug.LogWarning("Warning (MovePieceToPosition): trying to move piece below or above grid X limits - nothing done");
                }
            }
            else if (gridObjects[x, y] != null && !gridObjects[x, y].Equals(piece))
            {
                if (DEMOVEPIECETOPOSITION)
                {
					Debug.LogWarning("Warning (MovePieceToPosition): trying to move piece into occupied space " + x + ", " + y + " - nothing done");
                }
            }
            else if( (pieceSize == GridpieceController.ONExTWO && gridObjects[x, y - 1] && !gridObjects[x, y - 1].Equals(piece)) || 
                    (pieceSize == GridpieceController.TWOxONE && gridObjects[x - 1, y] && !gridObjects[x - 1, y].Equals(piece)) || 
                    (pieceSize == GridpieceController.TWOxTWO && 
                     ( (gridObjects[x - 1, y] && !gridObjects[x - 1, y].Equals(piece)) || 
                       (gridObjects[x, y - 1] && !gridObjects[x, y - 1].Equals(piece)) || 
                       (gridObjects[x - 1, y - 1] && !gridObjects[x - 1, y-1].Equals(piece)) ) ) )
            {
                if (DEMOVEPIECETOPOSITION)
                {
					Debug.LogWarning("Warning (MovePieceToPosition): piece of given size cannot fit into space provided because another block is in the way - nothing done");
                }
            }
            else 
            {
                if (DEMOVEPIECETOPOSITION)
                    Debug.Log("Moving Piece: [" + gpc.dimX + ", " + gpc.dimY + "] to [" + x + ", " + y + "]");

                Vector2[] originalPositions = gpc.GetPositions();
                for (int i = 0; i < originalPositions.Length; i++)
                    gridObjects[(int)originalPositions[i].x, (int)originalPositions[i].y] = null;
                gpc.dimX = x;
                gpc.dimY = y;
                Vector2[] newPositions = gpc.GetPositions();
                for (int i = 0; i < newPositions.Length; i++)
                    gridObjects[(int)newPositions[i].x, (int)newPositions[i].y] = piece;

                //StartCoroutine(MovePiece(piece, pieceSize, x, y, true));
                // Can swap the line above for the line below in order to create a moving piece
                StartCoroutine(MovePiece(piece, pieceSize, x, y, false));

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
        float timeForPieceToMove = 0.1f;
        Vector3 startPos = piece.transform.position;
        Vector3 endPos;
        if (pieceSize == GridpieceController.ONExONE)
            endPos = gridPositions[x, y];
        else if (pieceSize == GridpieceController.ONExTWO)
            endPos = (gridPositions[x, y] + gridPositions[x, y - 1]) / 2;
        else if (pieceSize == GridpieceController.TWOxONE)
            endPos = (gridPositions[x, y] + gridPositions[x - 1, y]) / 2;
        else
            endPos = (gridPositions[x, y] + gridPositions[x - 1, y - 1]) / 2;

        if (!instantly) {
			for (float i = 0; i <= timeForPieceToMove && piece != null; i += Time.deltaTime) {
                float counter = i / timeForPieceToMove;
                piece.transform.position = Vector3.Lerp(startPos, endPos, counter);
                yield return null;
            }
        }
		if (piece != null)
        	piece.transform.position = endPos;

		if (selectionbufferCo == null && justAddedRow) {
			selectionbufferCo = StartCoroutine(SelectionBufferTimer());
		}
		
    }

	private IEnumerator SelectionBufferTimer() {
		bool DESELECTIONBUFFER = false;

		if (DESELECTIONBUFFER)
			Debug.Log("Starting Buffer");
		selectionBuffer = true;
		yield return new WaitForSeconds(NEWLINE_SELECTION_BUFFER_TIME);
		selectionBuffer = false;
		justAddedRow = false;
		selectionbufferCo = null;
		if (DESELECTIONBUFFER)
			Debug.Log("Ending Buffer");
	}

    public void ResetCurrentLastPieces() {
        currentPiece = Vector2.one * -1;
        lastPiece = Vector2.one * -1;
    }

    // If there is a block in the top row return true
    // NO KNOWN BUGS
    public bool AlmostLost()
    {
		for (int i = 1; i < gridSize.x + extraX; i++)
		{
			if (gridObjects[i, (int)gridSize.y - 1] && gridObjects[i, (int)gridSize.y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
			{
                return true;
			}
		}
        return false;
    }

    // If there is at least one block in the middle row return true
    // NO KNOWN BUGS
    public bool AtLeastHalfFull()
    {
		for (int i = 1; i < gridSize.x + extraX; i++)
		{
            for (float j = gridSize.y / 2; j < gridSize.y + extraY; j++)
            {
                if (gridObjects[i, (int)j] && gridObjects[i, (int)j].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
                {
                    return true;
                }
            }
		}
        return false;
    }

	private void CheckGameOver() {
		for (int i = 1; i < gridSize.x + extraX; i++)
		{
			if (gridObjects[i, (int)gridSize.y - 1] && gridObjects[i, (int)gridSize.y - 1].GetComponent<GridpieceController>().blockColor != GridpieceController.EDGE)
			{
				GlobalVariables.gameOver = true;
				break;
			}
		}
	}

	private void LoadPlayBoard() {
		bool DELOADPLAYBOARD = false;
		if (loadSavedBoard) {
			LoadSavedBoard();
		}
		else if (useSpecificGrid) {
			TextAsset boardFile = (TextAsset)Resources.Load(specificGridFileName);
			if (boardFile == null) {
				Debug.LogError("Error (LoadPlayBoard): 'Using Specific Game Board' is checked but the file '" + specificGridFileName + "' doesn't exist in the Resources Folder -- Filling Board Randomly");
				SetUpGridEdgePieces(true);
				FillHalfBoardRandom();
			}
			else {
				string boardString = boardFile.text.Trim();
				char[] delimiters = { ' ', '\n', '\t', ',' };
				string[] boardPieces = boardString.Split(delimiters);
				if (boardPieces.Length < 2) {
					Debug.LogWarning("Error (LoadPlayBoard): Given file not correct syntax - does not have at least two numbers to designate the width and height of the board -- Filling Board Randomly");
					SetUpGridEdgePieces(true);
					FillHalfBoardRandom();
				}
				else {
					int boardWidth = 0;
					int boardHeight = 0;
					bool boardXProper = int.TryParse(boardPieces[0], out boardWidth);
					bool boardYProper = int.TryParse(boardPieces[1], out boardHeight);
					if (!boardXProper) {
						Debug.LogWarning("Error (LoadPlayBoard): Given file not correct syntax - first element in file (corresponding to X width of board) must be integer value -- Filling Board Randomly");
						SetUpGridEdgePieces(true);
						FillHalfBoardRandom();
					}
					else if (!boardYProper) {
						Debug.LogWarning("Error (LoadPlayBoard): Given file not correct syntax - second element in file (corresponding to Y height of board) must be integer value -- Filling Board Randomly");
						SetUpGridEdgePieces(true);
						FillHalfBoardRandom();
					}
					else {
						if (DELOADPLAYBOARD) {
							Debug.Log("Board Width is: " + boardWidth + "\nBoard Height is: " + boardHeight);
							foreach (string s in boardPieces) {
								if (s.Length == 0)
									Debug.Log("Empty");
								else
									Debug.Log("'" + s + "' -" + s.Length);
							}
						}
						gridSize.x = boardWidth;
						gridSize.y = boardHeight;
						SetUpGridEdgePieces(false);
						int numGarbageVals = 0;
						for (int i = 0; i < boardPieces.Length; i++) {
							if (boardPieces[i].Length == 0 || (int)boardPieces[i][0] < 48 || ((int)boardPieces[i][0] > 57 && (int)boardPieces[i][0] < 97))
								numGarbageVals++;
						}
						string[] onlyPieces = new string[boardPieces.Length - 2 - numGarbageVals];
						//print(numGarbageVals);
						for (int i = 2, j = 0; i < boardPieces.Length; i++) {
							if (boardPieces[i].Length != 0 && ((int)boardPieces[i][0] >= 48 && (int)boardPieces[i][0] <= 57) || ((int)boardPieces[i][0] >= 97 && (int)boardPieces[i][0] <= 122)) {
								onlyPieces[j] = boardPieces[i];
								j++;
							}
						}
						if (numGarbageVals > 0)
							Debug.LogWarning("Warning (LoadPlayboard): There were garbage values in the loaded file provided.  Please ensure that there is no extra whitespace in between the three-letter pieces and that all pieces follow the correct syntax.");
						
						FillBoardBasedOnStringArray(onlyPieces);
					}
				}
			}
		}
		else {
			SetUpGridEdgePieces(true);
			FillHalfBoardRandom();
		}
	}

	private void SavePlayBoard() {
		bool DESAVEPLAYBOARD = false;
		string dataPath = string.Format("{0}/SavedGameBoard.dat", Application.persistentDataPath);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream;

		/*==== The following lines are where I update what things are to be saved ====*/
		SavedBoard sBoard = new SavedBoard();
		sBoard.score = this.score;
		sBoard.boardX = (int)this.gridSize.x;
		sBoard.boardY = (int)this.gridSize.y;
		sBoard.boardPieces = StringifyBoard();

		/*==== End ====*/

		try {
			if (File.Exists(dataPath)) {
				File.WriteAllText(dataPath, string.Empty);
				fileStream = File.Open(dataPath, FileMode.Open);
			}
			else {
				fileStream = File.Create(dataPath);
			}

			binaryFormatter.Serialize(fileStream, sBoard);
			fileStream.Close();

			if (DESAVEPLAYBOARD)
				Debug.Log("Saved Play Board!");
		}
		catch (Exception e) {
			//PlatformSafeMessage("Failed to Save: " + e.Message);
			Debug.LogError("Failed To Save: " + e.Message);
		}
	}

	private bool LoadSavedBoard() {
		string dataPath = string.Format("{0}/SavedGameBoard.dat", Application.persistentDataPath);

		try {
			if (File.Exists(dataPath)) {
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = File.Open(dataPath, FileMode.Open);

				/*==== The following lines are where I update what things are to be loaded ====*/
				SavedBoard newBoard = (SavedBoard)binaryFormatter.Deserialize(fileStream);
				this.score = newBoard.score;
				this.gridSize.x = newBoard.boardX;
				this.gridSize.y = newBoard.boardY;
				string[] pieces = newBoard.boardPieces;

				SetUpGridEdgePieces(false);
				FillBoardBasedOnStringArray(pieces);

				fileStream.Close();
				return true;
			}
			else {
				Debug.LogError("Error (LoadSavedBoard): File given to load board from was not found.  Please Save a Board to be able to load a board. -- Creating Random Board");
				SetUpGridEdgePieces(true);
				FillHalfBoardRandom();
				return false;
			}
		}
		catch (Exception e) {
			//PlatformSafeMessage("Failed to Load: " + e.Message);
			Debug.Log("Failed To Load: " + e.Message);
			return false;
		}
	}

	private void SetUpGridEdgePieces(bool includeBottomRow) {
		if (gridSize.x < 1) {
			Debug.LogWarning("Error (SetUpGridEdgePieces): Given gridSize.x is less than 1 block wide -- Setting the value to default 8");
			gridSize.x = 8;
		}
		if (gridSize.y < 2) {
			Debug.LogWarning("Error (SetUpGridEdgePieces): Given gridSize.y is less than 2 blocks tall -- Setting the value to default 8");
			gridSize.y = 8;
		}

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
			if (includeBottomRow)
				AddPieceAtPosition(i, 0, 0, GridpieceController.ONExONE);
		}
	}

    private void FillHalfBoardRandom() 
    {
        for (int i = (int)gridSize.x; i >= 1; i--) 
        {
            for (int j = (int)gridSize.y; j >= 1; j--) 
            {
                if (!gridObjects[i, j]) 
                {
                    // Add blocks normally to the bottom half of the grid, we dont want to start completely full
                    if (i >= 1 && j > 0 && j <= gridSize.y / 2)
                    {
                        AddPieceAtPosition(i, j, -1, GridpieceController.ONExONE, 0);
                    }
                    // Add clear blocks above the half
                    else if (j > (gridSize.y / 2))
                    {
                        AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
                    } 
                    else 
                    {
                        AddPieceAtPosition(i, j, -1, GridpieceController.ONExONE);
                    }
                }
            }
        }
    }

	private void FillBoardBasedOnStringArray(string[] pieces) {
		if ((gridSize.x) * (gridSize.y) > pieces.Length)
			Debug.LogWarning("Warning (FillBoardBasedOnStringArray): Board is larger than String Array given -- Board will only be partially filled");

		for (int i = (int)gridSize.y; i >= 1; i--) {
			for (int j = (int)gridSize.x; j >= 1; j--) {
				if (gridObjects[j, i] == null && ((j + (int)(gridSize.x) * ((int)gridSize.y - i)) - 1) < pieces.Length) {
					
					string piece = pieces[(j + (int)(gridSize.x) * ((int)gridSize.y - i)) - 1];

					int blockType;
					if ((int)piece[0] - 48 >= 0 && (int)piece[0] - 57 <= 9)
						blockType = (int)piece[0] - 48;
					else if ((int)piece[0] - 97 >= 0 && (int)piece[0] - 122 <= 25)
						blockType = (int)piece[0] - 87;
					else
						blockType = -1;

					int blockColor = (int)piece[1] - 48;
					char blockSize = piece[2];
					if (blockType < 0 || blockType > GridpieceController.NUM_TYPES_SPECIAL_BLOCKS) {
						Debug.LogWarning("Warning (FillBoardBasedOnStringArray): Block Type at position (" + j + ", " + i + ") is not correct integer value/lowercase letter or too large for amount of special blocks specified in the GridpieceController Script. -- assigning Block Type to be regular");
						blockType = 0;
					}
					if (blockColor < 0 || blockColor > 9) {
						Debug.LogWarning("Warning (FillBoardBasedOnStringArray): Block Color at position (" + j + ", " + i + ") is not correct integer value -- assigning Block Color to be Clear/Edge piece");
						blockColor = 0;
					}
					if (!(blockSize == 'A' || blockSize == 'B' || blockSize == 'C' || blockSize == 'D')) {
						Debug.LogWarning("Warning (FillBoardBasedOnStringArray): Block Size at position (" + j + ", " + i + ") is not correct letter value (A-1x1 B-1x2 C-2x1 D-2x2) -- assigning Block Size to be A-1x1");
						blockSize = 'A';
					}

					if ((blockSize == 'B' || blockSize == 'C' || blockSize == 'D') && blockType != 0) {
						Debug.LogWarning("Warning (FillBoardBasedOnStringArray): Block Size at position (" + j + ", " + i + ") is set to a larger block (B, C, D) and doesn't have type 0 -- Setting type to 0");
						blockType = 0;
					} 

					if (blockColor == 0)
						AddPieceAtPosition(j, i, 0, GridpieceController.ONExONE, blockType);
					else {
						if (blockSize == 'A')
							AddPieceAtPosition(j, i, blockColor, GridpieceController.ONExONE, blockType);
						else if (blockSize == 'B')
							AddPieceAtPosition(j, i, blockColor, GridpieceController.ONExTWO, blockType);
						else if (blockSize == 'C')
							AddPieceAtPosition(j, i, blockColor, GridpieceController.TWOxONE, blockType);
						else
							AddPieceAtPosition(j, i, blockColor, GridpieceController.TWOxTWO, blockType);
					}
				}
			}
		}
		FillEmptySpaces();
	}

	private void FillEmptySpaces() {
		for (int i = 0; i < gridSize.x + extraX; i++) {
			for (int j = 0; j < gridSize.y + extraY; j++) {
				if (gridObjects[i, j] == null)
					AddPieceAtPosition(i, j, 0, GridpieceController.ONExONE);
			}
		}
	}

	public string[] StringifyBoard() {
		int arrSize = (int)(gridSize.x * gridSize.y);
		string[] board = new string[arrSize];
		for (int i = (int)gridSize.y, index = 0; i >= 1; i--) {
			for (int j = 1; j < gridSize.x + extraX; j++) {
				GridpieceController gpc = gridObjects[j, i].GetComponent<GridpieceController>();

				int type = gpc.blockType;
				int color = gpc.blockColor;
				char size;
				if (gpc.size == GridpieceController.ONExONE)
					size = 'A';
				else if (gpc.size == GridpieceController.ONExTWO)
					size = 'B';
				else if (gpc.size == GridpieceController.TWOxONE)
					size = 'C';
				else
					size = 'D';

				string piece;
				if (type < 10)
					piece = type + "" + color + "" + size;
				else 
					piece = ((char)(type + 87)) + "" + color + "" + size;
				board[index] = piece;
				index++;
			}
		}
		return board;
	}

	private void ActivateSaveCanvas() {
		if (saveCanvas != null) {
			saveCanvas.gameObject.SetActive(true);
			print("Enter File Name and click 'Save' to save board layout.  Leave file name blank and click 'Save' to unpause game and not save.");
			Time.timeScale = 0;
		}
	}
}

[Serializable]
class SavedBoard {
	public int score;
	public int boardX;
	public int boardY;
	public string[] boardPieces;
}
