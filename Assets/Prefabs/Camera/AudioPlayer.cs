using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {
	public static AudioPlayer audioPlayer;

	public enum BGMTracks {
		titlescreen = 0,
		COUNT
	}

	public enum SFX {
		accelerate = 0,
		accelerate2,
		crash,
		decelerate,
		idle,
		low_speed,
		mid_speed,
		high_speed,
		top_speed,
		COUNT
	}

	[SerializeField]
	private AudioClip[] bgmTracks = new AudioClip[(int)BGMTracks.COUNT];

	[SerializeField]
	private AudioClip[] sfx = new AudioClip[(int)SFX.COUNT];

	private AudioSource[] audioSources = new AudioSource[8];

	private void Awake() {
		if (audioPlayer) {
			Destroy(gameObject);
		}
		else {
			audioPlayer = this;

			for (int i = 0; i < audioSources.Length; i++) {
				audioSources[i] = gameObject.AddComponent<AudioSource>();
			}

			audioSources[0].loop = true;
			PlayBGM(BGMTracks.titlescreen);
		}
	}

	// Start is called before the first frame update
	void Start() {
		//
	}

	// Update is called once per frame
	void Update() {
		//
	}

	public void TestPolytonality() {
		for (int i = 0; i < (int)SFX.COUNT; i++) {
			PlaySound((SFX)i);
		}
	}

	public void PlayBGM(BGMTracks bgm) {
		audioSources[0].Stop();
		audioSources[0].clip = bgmTracks[(int)bgm];
		audioSources[0].Play();
	}

	public void PlaySound(SFX sound) {
		for (int i = 1; i < audioSources.Length; i++) {
			if (!audioSources[i].isPlaying) {
				audioSources[i].clip = sfx[(int)sound];
				audioSources[i].Play();
				break;
			}
		}
	}
}
