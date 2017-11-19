using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalGridPieceController : GridpieceController {

	public Sprite[] animalSprites;


	public override void SetColor() {
		if (blockType != REG_BLOCK || blockColor == BAD || blockColor == WHITE || blockColor == EDGE) {
			base.SetColor();
			sr.sprite = animalSprites[0];
		}
		else {
			if (sr == null)
				sr = GetComponent<SpriteRenderer>();

			if (blockColor == EDGE) {
				sr.sprite = animalSprites[0];
				sr.color = Color.clear;
				GetComponent<BoxCollider2D>().enabled = false;
			}

			sr.color = Color.white;
			sr.sprite = animalSprites[blockColor];

			/*
			else if (blockColor == RED) {
				sr.color = new Color(1.0f, 0f, 0f);
			}
			else if (blockColor == BLUE) {
				sr.color = new Color(0f, 0f, 1.0f);
			}
			else if (blockColor == GREEN) {
				sr.color = new Color(0f, 1.0f, 0f);
			}
			else if (blockColor == YELLOW) {
				sr.color = new Color(1.0f, 1.0f, 0f);
			}
			else if (blockColor == ORANGE) {
				sr.color = new Color(1.0f, 127.0f / 255.0f, 0f);
			}
			else if (blockColor == INDIGO) {
				// else if (blockColor == PURPLE)
				// sr.color = new Color(138.0f / 255.0f, 43.0f / 255.0f, 226.0f / 255.0f);
				sr.color = new Color(100.0f / 255.0f, 0f / 255.0f, 75.0f / 255.0f);
			}
			else if (blockColor == VIOLET) {
				// else if (blockColor == MAGENTA)
				// sr.color = Color.magenta;
				sr.color = new Color(148.0f / 255.0f, 0f / 255.0f, 211.0f / 255.0f);
			}
			else if (blockColor == CYAN) {
				sr.color = Color.cyan;
			}
			else if (blockColor == MAGENTA) {
				// else if (blockColor == SAND)
				// sr.color = new Color(166.0f / 255.0f, 145.0f / 255.0f, 80.0f / 255.0f);
				sr.color = Color.magenta;
			}
			*/

				
			setColor = true;
		}
	}	


	protected override void SetType() {
		if (blockType != REG_BLOCK)
			base.SetType();
		setType = true;
	}

	public override void SetSize() {
		if (size == ONExONE)
			transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
		else if (size == ONExTWO)
			transform.localScale = new Vector3(0.35f, 0.625f, 1);
		else if (size == TWOxONE)
			transform.localScale = new Vector3(0.625f, 0.35f, 1);
		else if (size == TWOxTWO)
			transform.localScale = new Vector3(.625f, .625f, 1);
		setSize = true;
	}



}
