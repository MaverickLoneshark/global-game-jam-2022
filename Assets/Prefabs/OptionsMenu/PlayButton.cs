using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour {
	private Button button;
	private InputMapper inputMapper;

	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	// Start is called before the first frame update
	void Start() {
		inputMapper = InputMapper.inputMapper;
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnEnable() {
		Time.timeScale = 0.0f;
	}

	void OnClick() {
		inputMapper.TogglePauseMenu();
	}
}
