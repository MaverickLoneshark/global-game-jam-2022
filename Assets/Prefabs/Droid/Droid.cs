using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Droid : MonoBehaviour {
	[SerializeField]
	private Sprite holdingSprite;
	public SpriteRenderer droidSprite;
	public int segmentIndex;
	public float offsetX, dropDelayRefTime;

	private void Awake() {
		if (!droidSprite) {
			droidSprite = GetComponent<SpriteRenderer>();
		}
	}

	private void Start() {
		droidSprite.sprite = holdingSprite;
		droidSprite.enabled = false;
	}

	private void OnDestroy() {
		Destroy(gameObject);
	}

	private void OnEnable() {
		Start();
	}
}
