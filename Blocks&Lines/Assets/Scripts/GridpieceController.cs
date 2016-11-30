using UnityEngine;
using System.Collections;

public class GridpieceController : MonoBehaviour {

	public const int EDGE = 0;
	public const int RED = 1;
	public const int BLUE = 2;
	public const int GREEN = 3;
	public const int YELLOW = 4;

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

		sr = GetComponent<SpriteRenderer>();

	}
	
	// Update is called once per frame
	void Update () {
	
		if (!setColor) {
			if (type == EDGE) {
				//sr.color = new Color(1, 0, 0, .5f);
				sr.color = Color.gray;
			}
			else if (type == RED)
				sr.color = Color.red;
			else if (type == BLUE)
				sr.color = Color.blue;
			else if (type == GREEN)
				sr.color = Color.green;
			else if (type == YELLOW)
				sr.color = Color.yellow;
		
			setColor = true;
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
}
