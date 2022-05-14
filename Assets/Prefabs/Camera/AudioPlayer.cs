using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {
	public static AudioPlayer audioPlayer;

	public enum BGMTracks {
		titlescreen = 0,
		main_theme,
		blood_sores,
		anti_wave,
		COUNT
	}

	public enum SFX {
		accelerate = 0,
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
	private InputMapper input_mapper;
	private BGMTracks current_bgm = 0;
	private bool holding_shoulder = false;

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
		}
	}

	// Start is called before the first frame update
	void Start() {
		input_mapper = InputMapper.inputMapper;
	}

	// Update is called once per frame
	void Update() {
		if (input_mapper[(int)InputMapper.CONTROLS.extra]) {
			if (!holding_shoulder) {
				current_bgm++;

				if (current_bgm == BGMTracks.COUNT) {
					current_bgm = 0;
				}

				PlayBGM(current_bgm);
				holding_shoulder = true;
			}
		}
		else if (input_mapper[(int)InputMapper.CONTROLS.alternative]) {
			if (!holding_shoulder) {
				current_bgm--;

				if (current_bgm < 0) {
					current_bgm = BGMTracks.COUNT - 1;
				}

				PlayBGM(current_bgm);
				holding_shoulder = true;
			}
		}
		else {
			holding_shoulder = false;
		}
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
	
	public void PlayCarSound(SFX sound, float pitch = 1.0f, float time_start = 0, float time_end = 1.0f, float volume = 0.5f) {
		if (!audioSources[1].isPlaying || (audioSources[1].clip != sfx[(int)sound])) {
			audioSources[1].clip = sfx[(int)sound];
			audioSources[1].pitch = pitch;
			audioSources[1].volume = volume;

			audioSources[1].time = time_start * audioSources[1].clip.length;
			audioSources[1].PlayScheduled(AudioSettings.dspTime);
			audioSources[1].SetScheduledEndTime(AudioSettings.dspTime + ((time_end - time_start) * audioSources[1].clip.length));
		}
	}
}
