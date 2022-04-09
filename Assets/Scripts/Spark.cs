using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spark : MonoBehaviour {
	public SpriteRenderer sparkSprite;
	public NPCar sparkedCar;

	public enum SparkDirection { Front, Back, Left, Right }
	public SparkDirection SparkSide;

	public float sparkFrameRefTime;
	public int curSparkFrameIndex;

	private void Awake() {
		if (!sparkSprite) {
			sparkSprite = GetComponent<SpriteRenderer>();
		}
	}

	private void Start() {
		curSparkFrameIndex = 0;
	}

	private void OnDestroy() {
		Destroy(gameObject);
	}
}
