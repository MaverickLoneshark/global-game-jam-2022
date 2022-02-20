using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour {
	private Button button;

	private InputMapper input_mapper;

	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	// Start is called before the first frame update
	void Start() {
		input_mapper = InputMapper.inputMapper;
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnClick() {
		Time.timeScale = 1.0f;
		input_mapper.TogglePauseMenu();
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
}
