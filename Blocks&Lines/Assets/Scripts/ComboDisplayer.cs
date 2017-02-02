using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboDisplayer : MonoBehaviour {

	public PlaygridController pgc;

	public SpriteRenderer[] srs;

	public float timeComboFadeIn = .75f;
	public float timeComboFadeOut = 0.35f;

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

		if (currentCombo != lastCombo && currentCombo > lastCombo && currentCombo >= 3) {
			StartCoroutine("DisplayCombo", currentCombo);
		}
		lastCombo = currentCombo;
	}


	private IEnumerator DisplayCombo(int comboShow) {
		int comboRend = comboShow - 3;
		if (comboRend >= srs.Length)
			comboRend = srs.Length - 1;
		else if (comboShow < 3)
			comboRend = 0;

		for (float i = 0; i < timeComboFadeIn; i += Time.deltaTime) {

			srs[comboRend].color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, i / timeComboFadeIn);
			srs[comboRend].transform.localPosition = Vector3.Lerp(Vector3.zero, Vector3.up * 2, i / timeComboFadeIn);
			yield return null;
		}

		for (float i = 0; i < timeComboFadeOut; i += Time.deltaTime) {
			srs[comboRend].color = Color.Lerp(Color.white, Color.clear, i / timeComboFadeOut);
			srs[comboRend].transform.localPosition = Vector3.Lerp(Vector3.up * 2, Vector3.up, i / timeComboFadeOut);
			yield return null;
		}

		srs[comboRend].color = Color.clear;

	}
}
