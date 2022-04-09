using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Droid : MonoBehaviour {
	public SpriteRenderer droidSprite;
	public int segmentIndex;
	public float offsetX, dropDelayRefTime;

	private void Awake() {
		if (!droidSprite) {
			droidSprite = GetComponent<SpriteRenderer>();
		}
	}

	private void Start() {
		droidSprite.enabled = false;
	}

	private void OnDestroy() {
		Destroy(gameObject);
	}
}
