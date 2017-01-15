using UnityEngine;
using System.Collections;

public class GridpieceController : MonoBehaviour {

    public static int blockCount;
    public int blockId;
    public GameObject explosion;
    public GameObject shockWave;

	public const int EDGE = 0;
	public const int RED = 1;
	public const int BLUE = 2;
	public const int GREEN = 3;
    public const int YELLOW = 4;
    public const int ORANGE = 5;
    public const int PURPLE = 6;
    public const int MAGENTA = 7;
    public const int CYAN = 8;
    public const int AQUA = 9;

	public int type;

	public const int ONExONE = 0;
	public const int ONExTWO = 1;
	public const int TWOxONE = 2;
	public const int TWOxTWO = 3;

	public int size;

    public int dimX = -100;
    public int dimY = -100;

	public bool selected;

	//public Vector2 placeInGrid;

	private bool setColor;
	private bool setSize;
	public SpriteRenderer sr;

	// Use this for initialization
	void Start () {
        blockCount++;
        blockId = blockCount;
		sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (!setColor) {
			SetColor();
		}

		if (!setSize) {
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
	}


	public void SetColor() {
		sr = GetComponent<SpriteRenderer>();
		if (type == EDGE) {
			//sr.color = new Color(1, 0, 0, .5f);
			//sr.color = Color.gray;
			sr.color = Color.clear;
			GetComponent<BoxCollider2D>().enabled = false;
		}
		else if (type == RED)
			sr.color = Color.red;
		else if (type == BLUE)
			sr.color = Color.blue;
		else if (type == GREEN)
			sr.color = Color.green;
		else if (type == YELLOW){
            sr.color = new Color(1.0f, 1.0f, 0f);
        }
        else if (type == ORANGE){
            sr.color = new Color(1.0f, 140.0f / 255.0f, 0f);
        }
        else if (type == PURPLE){
            sr.color = new Color(138.0f / 255.0f, 43.0f / 255.0f, 226.0f / 255.0f);
        }
        else if (type == MAGENTA)
            sr.color = Color.magenta;
        else if (type == CYAN)
            sr.color = Color.cyan;
        else if (type == AQUA)
            sr.color = new Color(0f, 1f, 1f);

		setColor = true;
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

    public GameObject Explode(Color color)
    {
        GameObject go = (GameObject)Instantiate(explosion, transform.position, Quaternion.identity);
        go.GetComponent<ParticleSystem>().startColor = color;
        return go;
    }

    public GameObject ShockWave(Color color)
    {
        GameObject go = (GameObject)Instantiate(shockWave, transform.position, Quaternion.identity);
        (go.GetComponent<Circle>() as Circle).color = color;
        // go.GetComponent<LineRenderer>().SetColors(color, color);
        return go;
    }
}
