using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Bomb : MonoBehaviour {
	[SerializeField]
	private GameObject blipObject;
	[SerializeField]
	private GameObject blipProjectionObject;

	public RawImage blip, blipProjection;
	public SpriteRenderer bombSprite;
	public int segmentIndex;
	public float offsetX;
	public float explosionHeightOffset = 50.0f;
	public bool isExploding;
	public int explosionFrameIndex;
	public float explosionRefTime, blipProjectionFlashRefTime;

	private void Awake() {
		if (!bombSprite) {
			bombSprite = GetComponent<SpriteRenderer>();
		}

		if (!blipObject) {
			blipObject = new GameObject("BombBlip");
			blipObject.AddComponent<RawImage>();
		}

		blip = blipObject.GetComponent<RawImage>();

		if (!blipProjectionObject) {
			blipProjectionObject = new GameObject("BlipProjection");
			blipProjectionObject.transform.SetParent(blipObject.transform);
			blipProjectionObject.AddComponent<RawImage>();
		}

		blipProjection = blipProjectionObject.GetComponent<RawImage>();
	}

	private void Start() {
		blipObject.SetActive(true);
		blipProjectionObject.SetActive(true);
		blipProjectionFlashRefTime = Time.time;
		isExploding = false;
		explosionFrameIndex = 0;
	}

	private void OnDestroy() {
		Destroy(blipObject);
		Destroy(blipProjectionObject);
		Destroy(gameObject);
	}

	public void InitializeBlip(Transform parent, Texture texture, Vector2 anchoredPosition) {
		blip.transform.SetParent(parent);
		blip.texture = texture;
		blip.rectTransform.anchoredPosition = anchoredPosition;
		blip.rectTransform.sizeDelta = Vector2.zero;
		blipProjection.transform.SetParent(parent);
	}

	public void InitializeBlipProjection(Texture texture, Vector2 anchoredPosition) {
		blipProjection.texture = texture;
		blipProjection.rectTransform.anchoredPosition = anchoredPosition;
		blipProjection.rectTransform.sizeDelta = Vector2.zero;
	}

	public void InitializeBomb(int segment_index, float offset_x, Sprite sprite) {
		segmentIndex = segment_index;
		offsetX = offset_x;
		bombSprite.sprite = sprite;
	}

	public void Detonate() {
		isExploding = false;
		Deactivate();
	}

	public void Deactivate() {
		blipObject.SetActive(false);
		blipProjectionObject.SetActive(false);
		gameObject.SetActive(false);
	}

	private void OnEnable() {
		Start();
	}
}
