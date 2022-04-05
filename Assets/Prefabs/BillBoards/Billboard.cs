using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Billboard : MonoBehaviour {
	public int segmentIndex;
	public int offsetX;

	private SpriteRenderer spriteRend;
	public Sprite sprite {
		get {
			return spriteRend.sprite;
		}

		set {
			spriteRend.sprite = value;
		}
	}

	private void Awake() {
		transform.parent = GameObject.Find("Billboards").transform;
		spriteRend = GetComponent<SpriteRenderer>();

		if (!spriteRend) {
			spriteRend = gameObject.AddComponent<SpriteRenderer>();
		}
	}

	private void Start() {
		spriteRend.enabled = false;
	}

	private void OnDestroy() {
		Destroy(gameObject);
	}

	public void EnableSprite() {
		spriteRend.enabled = true;
	}

	public void DisableSprite() {
		spriteRend.enabled = false;
	}
}
