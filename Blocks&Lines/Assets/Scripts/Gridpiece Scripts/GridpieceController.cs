using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class GridpieceController : MonoBehaviour {

    public static int blockCount;
    public int blockId;
    public GameObject explosion;
    public GameObject shockWave;

	public GameObject timer;

	public PlaygridController pgc;

    public const int BAD = -1;
	public const int EDGE = 0;
	public const int RED = 1;
    public const int ORANGE = 2;
    public const int YELLOW = 3;
	public const int GREEN = 4;
    public const int BLUE = 5;
    public const int VIOLET = 6;
    public const int INDIGO = 7;
    public const int MAGENTA = 8;
    public const int CYAN = 9;
	public const int WHITE = 10;

	public int blockColor;

	public const int ONExONE = 0;
	public const int ONExTWO = 1;
	public const int TWOxONE = 2;
	public const int TWOxTWO = 3;

	public int size;

    public int dimX = -100;
    public int dimY = -100;
    public List<GameObject> nextToThis;

	// If we create more special block types, update this number
	public const int NUM_TYPES_SPECIAL_BLOCKS = 11;

	public const int REG_BLOCK = 0;
	public const int REMOVE_ONE_COLOR_BLOCK = 1; // Done
	public const int VERT_CLEAR_BLOCK = 2; // Done
	public const int HORIZ_CLEAR_BLOCK = 3; // Done 
	public const int PLUS_CLEAR_BLOCK = 4; // Done
    public const int UP_BLOCK = 5; // Done
	public const int WASTE_OF_SPACE_BLOCK = 6; // Done
	public const int WHITEWASH_BLOCK = 7; // Done
	public const int BOMB_BLOCK = 8; // DONE
	public const int BUBBLES_BLOCK = 9;
	public const int CLOCK_BLOCK = 10; // DONE
	public const int COLORWASH_BLOCK = 11; // Done
    
    public float countdown;
    public bool hasCountdown = false;

	public int blockType;

	public bool selected;

	public Material[] oneSizeScoreMats;
	public Material[] twoSizeScoreMats;
	public Material[] fourSizeScoreMats;

	public Sprite[] blockShapeSprites;
	//public Vector2 placeInGrid;

	protected bool setColor;
	protected bool setSize;
	protected bool setType = false;
	public SpriteRenderer sr;

	// Use this for initialization
	void Start () {

        countdown = Random.Range(6, 11);
        blockCount++;
        blockId = blockCount;
		if (sr == null) {
			sr = GetComponent<SpriteRenderer>();
        }
	}

	// Update is called once per frame
	void Update () {
	
		if (!setType) {
			SetType();
		}
		if (!setColor) {
			SetColor();
		}
		if (hasCountdown && !pgc.pause && !GlobalVariables.gameOver)
        {
            countdown -= Time.deltaTime;
        }
		if (!setSize) {
			SetSize();
		}

	}


	public virtual void SetColor() {
		if (sr == null)
			sr = GetComponent<SpriteRenderer>();

		if (blockColor == EDGE) {
			//sr.color = new Color(1, 0, 0, .5f);
			// sr.color = Color.gray;
			sr.color = Color.clear;
			GetComponent<BoxCollider2D>().enabled = false;
		}
        else if (blockColor == BAD)
        {
            sr.color = Color.black;
        }
		else if (blockColor == RED)
        {
            sr.color = new Color(1.0f, 0f, 0f);
        }
		else if (blockColor == BLUE)
        {
			sr.color = new Color(0f, 0f, 1.0f);
        }
		else if (blockColor == GREEN)
        {
			sr.color = new Color(0f, 1.0f, 0f);
        }
		else if (blockColor == YELLOW)
        {
            sr.color = new Color(1.0f, 1.0f, 0f);
        }
        else if (blockColor == ORANGE)
        {
            sr.color = new Color(1.0f, 127.0f / 255.0f, 0f);
        }
        else if (blockColor == INDIGO)
        // else if (blockColor == PURPLE)
        {
            // sr.color = new Color(138.0f / 255.0f, 43.0f / 255.0f, 226.0f / 255.0f);
            sr.color = new Color(100.0f / 255.0f, 0f / 255.0f, 75.0f / 255.0f);
        }
        else if (blockColor == VIOLET)
        // else if (blockColor == MAGENTA)
        {
            // sr.color = Color.magenta;
            sr.color = new Color(148.0f / 255.0f, 0f / 255.0f, 211.0f / 255.0f);
        }
        else if (blockColor == CYAN)
        {
            sr.color = Color.cyan;
        }
        else if (blockColor == MAGENTA)
        // else if (blockColor == SAND)
        {
            // sr.color = new Color(166.0f / 255.0f, 145.0f / 255.0f, 80.0f / 255.0f);
            sr.color = Color.magenta;
        }

		setColor = true;
	}

	protected virtual void SetType(){
		if (sr == null)
			sr = GetComponent<SpriteRenderer>();

		if (blockType != REG_BLOCK) 
        {
            // MAKE SURE THE PROPER BLOCKS ARE WHITE AND UNSELECTABLE
            if (blockType == REMOVE_ONE_COLOR_BLOCK || blockType == HORIZ_CLEAR_BLOCK || blockType == UP_BLOCK || blockType == WASTE_OF_SPACE_BLOCK || blockType == WHITEWASH_BLOCK || blockType == COLORWASH_BLOCK || blockType == BUBBLES_BLOCK)
            {
                blockColor = 10;
                sr.color = Color.white;
                setColor = true;
            }
			// Make sure that special blocks with color actually have those colors
			else if (blockColor == 0 || blockColor > 10){
				Debug.LogWarning("Warning (SetType): Block in space " + dimX + " " + dimY + " is set to a special type that needs a color but has no color or too high a color-- Setting color to Red");
				blockColor = RED;
				sr.color = Color.red;
				setColor = true;
			}
            // MAKE SURE COUNT DOWN BLOCKS HAVE AN ACTIVE TIMER
            if (blockType == UP_BLOCK || blockType == WHITEWASH_BLOCK || blockType == BUBBLES_BLOCK)
            {
                hasCountdown = true;
				GameObject go = Instantiate(timer, transform.position, Quaternion.identity);
				go.transform.parent = this.transform;
				go.GetComponent<CountdownTimerController>().blockgpc = this;
            }
		}
			
		if (blockType < blockShapeSprites.Length){
			sr.sprite = blockShapeSprites[blockType];
        }
		setType = true;
	}


	public virtual void SetSize() {
		if (size == ONExONE)
			transform.localScale = new Vector3(0.5f, 0.3f, .3f);
		else if (size == ONExTWO)
			transform.localScale = new Vector3(0.5f, 0.6f, 1);
		else if (size == TWOxONE)
			transform.localScale = new Vector3(1f, 0.3f, 1);
		else if (size == TWOxTWO)
			transform.localScale = new Vector3(1f, 0.6f, 1);
		setSize = true;
	}

	// Returns an array of positions of the block based on the size and the dimX and dimY
	public Vector2[] GetPositions() {
		Vector2[] returner = new Vector2[0];
		if (size == ONExONE) {
			returner = new Vector2[1];
			returner[0] = new Vector2(dimX, dimY);
		}
		else if (size == ONExTWO) {
			returner = new Vector2[2];
			returner[0] = new Vector2(dimX, dimY);
			returner[1] = new Vector2(dimX, dimY - 1);
		}
		else if (size == TWOxONE) {
			returner = new Vector2[2];
			returner[0] = new Vector2(dimX, dimY);
			returner[1] = new Vector2(dimX - 1, dimY);
		}
		else if (size == TWOxTWO) {
			returner = new Vector2[4];
			returner[0] = new Vector2(dimX, dimY);
			returner[1] = new Vector2(dimX - 1, dimY);
            returner[2] = new Vector2(dimX, dimY - 1);
			returner[3] = new Vector2(dimX - 1, dimY - 1);
		}
		return returner;
	}


	// Returns true if block is tall and partially on the grid, false if otherwise
	public bool IsPartiallyOn() {
		return (size == ONExTWO || size == TWOxTWO) && dimY == 1;
	}

    public GameObject Explode(Color color) {
        GameObject go = (GameObject)Instantiate(explosion, transform.position, Quaternion.identity);
	    ParticleSystem ps = go.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.startColor = color;
		go.transform.Find("ScorePart").gameObject.SetActive(false);
        return go;
    }

	public GameObject ScoreExplode(Color color, int scoreAmount, int comboLevel) {
		if (comboLevel > 10)
        {
			comboLevel = 10;
        }
		if (comboLevel < 1)
        {
			comboLevel = 1;
        }
		GameObject go = (GameObject)Instantiate(explosion, transform.position, Quaternion.identity);
		if (scoreAmount == 1)
        {
			go.transform.Find("ScorePart").GetComponent<Renderer>().material = oneSizeScoreMats[comboLevel - 1];
        }
		else if (scoreAmount == 2)
        {
			go.transform.Find("ScorePart").GetComponent<Renderer>().material = twoSizeScoreMats[comboLevel - 1];
        }
		else if (scoreAmount == 4)
        {
			go.transform.Find("ScorePart").GetComponent<Renderer>().material = fourSizeScoreMats[comboLevel - 1];
        }
		else
        {
			Debug.LogError("Error (ScoreExplode): setting a scoreAmount that isn't 1, 2 or 4 -- Nothing done");
        }

	    ParticleSystem ps = go.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.startColor = color;
		return go;
	}


    public GameObject ShockWave(Color color, int maxRadius)
    {
        GameObject go = (GameObject)Instantiate(shockWave, transform.position, Quaternion.identity);
        (go.GetComponent<Circle>() as Circle).color = color;
        (go.GetComponent<Circle>() as Circle).maxRadius = maxRadius;
        // go.GetComponent<LineRenderer>().SetColors(color, color);
        return go;
    }

	public void ClearBlockType() {
		blockType = REG_BLOCK;
		sr.sprite = blockShapeSprites[REG_BLOCK];
        hasCountdown = false;
		if (blockColor > 9)
			blockColor = Random.Range(1, 10);
		SetColor();
	}

	public void Colorwash(Color color, bool washOut) {
		sr.color = color;
        if (washOut)
        {
            sr.sprite = blockShapeSprites[REG_BLOCK];
            blockType = REG_BLOCK;
        }
	}

	public void Repaint() {
		SetType();
		if (blockColor > 9)
        {
            blockColor = Random.Range(1, 10);
        }
		SetColor();
	}
}
