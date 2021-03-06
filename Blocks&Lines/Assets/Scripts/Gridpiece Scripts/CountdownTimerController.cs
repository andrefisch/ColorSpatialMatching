﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimerController : MonoBehaviour {

	public Sprite[] countdownBlocks;

	[HideInInspector]
	public GridpieceController blockgpc;

	private SpriteRenderer sr;

	private float startTime = 3;

	private Color startCol;
	private Color endCol;

	void Awake() {
		sr = GetComponent<SpriteRenderer>();
		startCol = sr.color;
		endCol = new Color(sr.color.r * .175f, sr.color.g * .175f, sr.color.b * .175f, 1);
	}

	// Use this for initialization
	void Start () {
		if (blockgpc != null) {
			startTime = blockgpc.countdown;	
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (blockgpc != null && !GlobalVariables.gameOver) {
			float amount = blockgpc.countdown / startTime;
			if (amount < 0)
				amount = 0;
			int index = countdownBlocks.Length - ((int)(countdownBlocks.Length * amount) + 1);

			sr.sprite = countdownBlocks[index];

			//This one will lighten the block over time
			sr.color = Color.Lerp(startCol, endCol, amount);

            // remove the timer visuals if the timer is turned off
            if (blockgpc && !blockgpc.hasCountdown)
            {
                Destroy (this.gameObject);
            }

			//This one will darken the block over time
			//sr.color = Color.Lerp(endCol, startCol, amount);
		}
	}
}
