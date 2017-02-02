using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboDisplayer : MonoBehaviour {

	public PlaygridController pgc;

	public SpriteRenderer[] srs;

	private int lastCombo;
	private int currentCombo;

	//private bool lockout;

	// Use this for initialization
	void Start () {
		//lockout = false;

		currentCombo = pgc.combos;
		lastCombo = currentCombo;
	}
	
	// Update is called once per frame
	void Update () {

		currentCombo = pgc.combos;

		if (currentCombo != lastCombo && currentCombo > lastCombo) {
			StartCoroutine("DisplayCombo", currentCombo);
		}
		lastCombo = currentCombo;
	}


	private IEnumerator DisplayCombo(int comboShow) {


		yield return null;
	}
}
