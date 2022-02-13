using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenPlayButton : MonoBehaviour {
	private Button button;

	private InputMapper inputMapper;
	private AudioPlayer audioPlayer;
	private float startTime = 0;

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

		for (int i = 0; i < (int)InputMapper.CONTROLS.COUNT; i++) {
			if (inputMapper[i]) {
				startTime = Time.time;
			}
		}
	}

	private void OnClick() {
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}
}
