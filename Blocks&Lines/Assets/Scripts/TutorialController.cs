﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour {

    public PlaygridController pgc;
    public static int tutorialPart;
    public Text tutorialText;
    public CanvasGroup canvas;

	private bool lockoutBool;

    private int counter = 0;
    private bool counterOn = false;
    // Use this for initialization
    void Start () {
        tutorialPart = 0;
		canvas.alpha = 0;
        StartCoroutine(RunTutorial());    
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown("k"))
            StartCoroutine(FadeCanvas(true));
        if (Input.GetKeyDown("j"))
            StartCoroutine(FadeCanvas(false));
    }

    public IEnumerator RunTutorial()
    {
        yield return null;
        yield return null;
        AllCollidersOff();
        if (tutorialPart == 0)
        {
			tutorialText.text = "Welcome to Blocks and Lines! The goal of this game is to get as many points as you can before you lose";
            StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
            yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            tutorialText.text = "Over time new rows will be added from the bottom. If a block gets pushed past the green line at the top the game will end";
            StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
            yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            tutorialText.text = "Points are scored by matching blocks of the same color in several different ways. The first way is to match two adjacent blocks. Try matching the two PINK blocks by selecting them";
            StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            // yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            // StartCoroutine(FadeCanvas(false));
            GameObject o1 = pgc.gridObjects[4, 3];
            GameObject o2 = pgc.gridObjects[5, 3];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null && !pgc.startCounting));
            tutorialPart = 1;
        }
        if (tutorialPart == 1)
        {
            Debug.Log("Tutorial Part 1");
			tutorialText.text = "The next way to match is if the two blocks could be connected by a horizontal or vertical line. Try selecting the two GREEN blocks";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            GameObject o1 = pgc.gridObjects[3, 5];
            GameObject o2 = pgc.gridObjects[8, 5];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null && !pgc.startCounting));
            tutorialPart = 2;
        }
        if (tutorialPart == 2)
        {
            Debug.Log("Tutorial Part 2");
			tutorialText.text = "The third way to match is if the two blocks are on a right angle to each other. Try connecting the two YELLOW blocks";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
            yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            GameObject o1 = pgc.gridObjects[4, 3];
            GameObject o2 = pgc.gridObjects[5, 2];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null && !pgc.startCounting));
            tutorialPart = 3;
        }
        if (tutorialPart == 3)
        {
            Debug.Log("Tutorial Part 3");
			tutorialText.text = "When a block falls so that it is touching a block of the same color that it wasn't touching before a combo happens. Combos will get you more points so try to make combos whenever you can!";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
            yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
            StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            tutorialText.text = "The last type of match can be the hardest to see: two blocks also match if they are on two right angles to each other. Try clicking on the BLUE blocks to demonstrate";
            StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            GameObject o1 = pgc.gridObjects[1, 1];
            GameObject o2 = pgc.gridObjects[8, 4];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null && !pgc.startCounting));
            tutorialPart = 4;
        }
        if (tutorialPart == 4)
        {
            Debug.Log("Tutorial Part 4");
			tutorialText.text = "Distance is not a factor when making matches: the blocks can be one space away or ten spaces away. Now that you know how to make matches can you see a combo on the screen that will get you a lot of points? Give it a try!";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => !lockoutBool);
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            GameObject o1 = pgc.gridObjects[1, 1];
            GameObject o2 = pgc.gridObjects[8, 3];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null && !pgc.startCounting));
            tutorialPart = 5;
        }
        if (tutorialPart == 5)
        {
            Debug.Log("Tutorial Part 5");
			tutorialText.text = "A new row is added either when the timer runs out or when a there are no blocks above the halfway point";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            /*
            GameObject o1 = pgc.gridObjects[1, 2];
            GameObject o2 = pgc.gridObjects[8, 3];
            o1.GetComponent<Collider2D>().enabled = true;
            o2.GetComponent<Collider2D>().enabled = true;
            yield return new WaitUntil(() => (o1 == null));
            */
            tutorialPart = 6;
        }
        if (tutorialPart == 6)
        {
            Debug.Log("Tutorial Part 6");
			tutorialText.text = "As time goes on more colors and bigger blocks will appear. These blocks match and combo just like the little ones but are worth more points.";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
			tutorialText.text = "Most white blocks have special effects but none can be selected. You'll have to make a match next to most of them to get them to disappear. Make sure you remove the blocks with timers before their nasty effects trigger!";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
			tutorialText.text = "And that's basically it! Now lets try a real game!";
			StartCoroutine(FadeCanvas(true));
			yield return new WaitUntil(() => (Input.GetMouseButtonDown(0)));
			StartCoroutine(FadeCanvas(false));
			yield return new WaitUntil(() => !lockoutBool);
            SceneManager.LoadScene("ArcadeMode");
        }
        yield return null;
    }

    public IEnumerator FadeCanvas(bool fadeIn)
    {
		if (!lockoutBool) {
			lockoutBool = true;
			for (float i = fadeIn ? 0 : 1; fadeIn ? i < 1 : i > 0; i += 2 * (fadeIn ? Time.deltaTime : -Time.deltaTime)) {
				canvas.alpha = i;
				yield return null;
			} 
			canvas.alpha = fadeIn ? 1 : 0;
			lockoutBool = false;
		}
    }

    private void AllCollidersOff()
    {
        // Set up the actual pieces
        for (int i = (int)pgc.gridSize.x; i >= 1; i--) 
        {
            for (int j = (int)pgc.gridSize.y; j >= 1; j--) 
            {
                if (pgc.gridObjects[i, j]) 
                {
                    pgc.gridObjects[i, j].GetComponent<Collider2D>().enabled = false;
                }
            }
        }
    }
}
