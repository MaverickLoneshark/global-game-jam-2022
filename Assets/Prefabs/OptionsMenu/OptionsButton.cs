using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour {
	[SerializeField]
	GameObject subMenuObject;

	private Button button;
	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	// Start is called before the first frame update
	void Start() {
		//
	}

	// Update is called once per frame
	void Update() {
		//
	}

	void OnClick() {
		subMenuObject.SetActive(true);
	}
}
