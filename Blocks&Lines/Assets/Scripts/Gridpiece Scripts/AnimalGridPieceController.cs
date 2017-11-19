using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalGridPieceController : GridpieceController {

	public Sprite[] animalSprites;

	public GameObject specialBlockBackground;

	private AnimalSpecialBackgroundController specialBlockBG;

	public override void SetColor() {

		if (sr == null)
			sr = GetComponent<SpriteRenderer>();

		if (blockColor == EDGE) {
			sr.sprite = animalSprites[0];
			sr.color = Color.clear;
			GetComponent<BoxCollider2D>().enabled = false;
		}
		else {

			sr.color = Color.white;
			sr.sprite = animalSprites[blockColor];

			if (specialBlockBG != null) {
				specialBlockBG.color = this.blockColor;
				specialBlockBG.SetColor();
			}

		}
		setColor = true;

	}	


	protected override void SetType() {
		if (sr == null)
			sr = GetComponent<SpriteRenderer>();

		if (blockType != REG_BLOCK) {
			// These special blocks have no color

			if (blockType < blockShapeSprites.Length) {
				sr.sprite = blockShapeSprites[blockType];
			}
			else {
				Debug.LogWarning("Warning (SetType): Block in location " + dimX + " " + dimY + " is set to a special type that exceeds the amount of sprites in array for special blocks -- Setting type to Reg Block");
				blockType = REG_BLOCK;
				setType = true;
				return;
			}


			if (blockType == REMOVE_ONE_COLOR_BLOCK || blockType == HORIZ_CLEAR_BLOCK || blockType == UP_BLOCK || blockType == WASTE_OF_SPACE_BLOCK || blockType == WHITEWASH_BLOCK || blockType == COLORWASH_BLOCK || blockType == BUBBLES_BLOCK)
			{
				blockColor = 10;
				sr.color = Color.white;
			}
				
			// Make sure that special blocks with color actually have those colors
			else if (blockColor == 0 || blockColor > 10){
				Debug.LogWarning("Warning (SetType): Block in location " + dimX + " " + dimY + " is set to a special type that needs a color but has no color or too high a color-- Setting color to Red");
				blockColor = RED;
			}

			// Give special blocks with color backgrounds
			if (blockType == VERT_CLEAR_BLOCK || blockType == PLUS_CLEAR_BLOCK || blockType == BOMB_BLOCK || blockType == CLOCK_BLOCK) {
				GameObject go = Instantiate(specialBlockBackground, transform.position, Quaternion.identity);
				go.transform.parent = this.transform;
				specialBlockBG = go.GetComponent<AnimalSpecialBackgroundController>();
				specialBlockBG.color = this.blockColor;
				specialBlockBG.SetColor();
			}


			// MAKE SURE COUNT DOWN BLOCKS HAVE AN ACTIVE TIMER
			if (blockType == UP_BLOCK || blockType == WHITEWASH_BLOCK || blockType == BUBBLES_BLOCK)
			{
				hasCountdown = true;
				GameObject go = Instantiate(timer, transform.position, Quaternion.identity);
				go.transform.parent = this.transform;
				go.GetComponent<CountdownTimerController>().blockgpc = this;
			}
			setType = true;
		}
		else {
			setType = true;
		}
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
