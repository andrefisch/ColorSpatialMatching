using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBubbleExplosion : MonoBehaviour {

	public Material[] particleMats;

	void Awake() {
		//StartCoroutine(ParticleColors());
		GetComponent<Renderer>().material = particleMats[Random.Range(0, particleMats.Length)];
		StartCoroutine(RemoveAfterSeconds(1f));
	}


	public IEnumerator RemoveAfterSeconds(float sec) {
		yield return new WaitForSeconds(sec);
		GameObject.Destroy(this.gameObject);
	}
}
