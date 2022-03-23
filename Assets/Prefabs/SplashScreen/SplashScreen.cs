using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SplashScreen : MonoBehaviour {
	public VideoPlayer videoPlayer;

	InputMapper inputMapper;
	AudioPlayer audioPlayer;
	
	GameObject credits;
	GameObject programmingCredits;
	GameObject artCredits;
	GameObject audioCredits;
	GameObject producerDirectorCredits;

	private string videoPath = Path.Combine(Application.streamingAssetsPath, "Video", Uri.EscapeDataString("GGJ2022 Intro Video.mp4"));

	void Awake() {
		if (!videoPlayer) {
			videoPlayer = transform.Find("VideoPlayer").GetComponent<VideoPlayer>();
		}
		
		if (!videoPlayer.targetCamera) {
			videoPlayer.targetCamera = Camera.main;
		}

		videoPlayer.errorReceived += ResolveVideoError;
		videoPlayer.prepareCompleted += BeginVideo;
		videoPlayer.loopPointReached += (context) => { UnityEngine.SceneManagement.SceneManager.LoadScene(1); };
		videoPlayer.Prepare();

		credits = transform.Find("Credits").gameObject;
		programmingCredits = credits.transform.Find("ProgrammingCredits").gameObject;
		artCredits = credits.transform.Find("ArtCredits").gameObject;
		audioCredits = credits.transform.Find("AudioCredits").gameObject;
		producerDirectorCredits = credits.transform.Find("ProducerDirectorCredits").gameObject;
	}

	void Start() {
		inputMapper = InputMapper.inputMapper;
		audioPlayer = AudioPlayer.audioPlayer;
	}

	// Update is called once per frame
	void Update() {
		if ((videoPlayer.time > 7.0f) && (videoPlayer.time < 12.0f)) {
			if (!programmingCredits.activeSelf) {
				programmingCredits.SetActive(true);
			}
		}
		else if ((videoPlayer.time > 12.0f) && (videoPlayer.time < 17.0f)) {
			if (!artCredits.activeSelf) {
				programmingCredits.SetActive(false);
				artCredits.SetActive(true);
			}
		}
		else if((videoPlayer.time > 17.0f) && (videoPlayer.time < 22.0f)) {
			if (!audioCredits.activeSelf) {
				artCredits.SetActive(false);
				audioCredits.SetActive(true);
			}
		}
		else if(videoPlayer.time > 22.0f) {
			if (!producerDirectorCredits.activeSelf) {
				audioCredits.SetActive(false);
				producerDirectorCredits.SetActive(true);
			}
		}

		for (int i = 0; i < (int)InputMapper.CONTROLS.COUNT; i++) {
			if (inputMapper[i]) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(1);
				break;
			}
		}
	}

	void BeginVideo(VideoPlayer videoPlayer) {
		audioPlayer.PlayBGM(AudioPlayer.BGMTracks.blood_sores);
		videoPlayer.Play();
	}

	void ResolveVideoError(VideoPlayer videoPlayer, string errorMessage) {
		if (videoPlayer.url != videoPath) {
			Debug.LogWarning(errorMessage +
				"\nAttempting to load " + videoPath);

			videoPlayer.url = videoPath;
			videoPlayer.Prepare();
		}
		else {
			Debug.LogError(errorMessage +
				"\nSkipping to title screen.");

			UnityEngine.SceneManagement.SceneManager.LoadScene(1);
		}
	}
}
