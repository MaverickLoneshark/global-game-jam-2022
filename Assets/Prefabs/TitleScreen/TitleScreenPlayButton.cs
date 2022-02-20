using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenPlayButton : MonoBehaviour {
	private Button button;

	private InputMapper inputMapper;
	private AudioPlayer audioPlayer;
	private float startTime = 0;
	private bool pressing_start = true;

	// Start is called before the first frame update
	void Start() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);

		inputMapper = InputMapper.inputMapper;

		audioPlayer = AudioPlayer.audioPlayer;
		audioPlayer.PlayBGM(AudioPlayer.BGMTracks.titlescreen);

		startTime = Time.time;
	}

	// Update is called once per frame
	void Update() {
		if ((Time.time - startTime) > 30) {
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			startTime = Time.time; //so multiple loads don't trigger
		}

		if (inputMapper[(int)InputMapper.CONTROLS.start]) {
			if (!pressing_start) {
				inputMapper.HoldStart();
				StartGame();
			}
		}
		else {
			pressing_start = false;

			if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed ||
				UnityEngine.InputSystem.Mouse.current.middleButton.isPressed ||
				UnityEngine.InputSystem.Mouse.current.rightButton.isPressed ||
				UnityEngine.InputSystem.Keyboard.current.anyKey.isPressed) {
				startTime = Time.time;
			}
			else {
				for (int i = 0; i < (int)InputMapper.CONTROLS.COUNT; i++) {
					if (inputMapper[i]) {
						startTime = Time.time;
						break;
					}
				}
			}
		}
	}

	private void StartGame() {
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}

	private void OnClick() {
		StartGame();
	}
}
