using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadStrip : MonoBehaviour {
	public SpriteRenderer spriteRenderer;

	// Start is called before the first frame update
	void Awake() {
		if (!spriteRenderer) {
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
	}
}
