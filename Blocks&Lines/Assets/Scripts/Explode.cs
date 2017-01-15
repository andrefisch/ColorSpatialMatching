using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour {

	public GameObject other;

	public bool destroyOtherImmediately;
	public float otherTimeLife = 1f;
	public float thisTimeLife = 1f;
    // public Color color;

    void Awake() 
    {
    }

	// Use this for initialization
	void Start () {

        // this.GetComponent<ParticleSystem>().startColor = gameObject.GetComponent<SpriteRenderer>().color;
        // color = Color.red;
        // this.GetComponent<ParticleSystem>().startColor = color;
		StartCoroutine("PartExpl"); // This is for playing around with a particle explosion

	}
	
	// Update is called once per frame
	void Update () {

	}

	public IEnumerator PartExpl() {
		yield return null;
		 if (other) {
			
			if (destroyOtherImmediately)
				GameObject.DestroyImmediate(other, true);
			else {
				thisTimeLife = thisTimeLife - otherTimeLife;
				yield return new WaitForSeconds(otherTimeLife);
				GameObject.DestroyImmediate(other, true);
			}
		}
		yield return new WaitForSeconds(thisTimeLife);
		GameObject.Destroy(this.gameObject);
	}
}
