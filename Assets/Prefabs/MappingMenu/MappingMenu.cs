using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MappingMenu : MonoBehaviour {
	InputMapper inputMapper;
	AudioPlayer audioPlayer;

	bool [] still_pressed = new bool[12];

	// Start is called before the first frame update
	void Start() {
		inputMapper = InputMapper.inputMapper;
		audioPlayer = AudioPlayer.audioPlayer;
	}

	// Update is called once per frame
	void Update() {
		if (!inputMapper[(int)InputMapper.CONTROLS.alternative] && !inputMapper[(int)InputMapper.CONTROLS.extra]) {
			for (int i = 0; i < (int)InputMapper.CONTROLS.COUNT; i++) {
				if (inputMapper[i]) {
					if (!still_pressed[i]) {
						audioPlayer.PlaySound((AudioPlayer.SFX)(i % (int)AudioPlayer.SFX.COUNT));
						still_pressed[i] = true;
					}
				}
				else {
					still_pressed[i] = false;
				}
			}
		}
	}

	private void OnGUI() {
		GUILayout.TextField("Pointer: " + inputMapper.pointer_position_x + ", " + inputMapper.pointer_position_y);

#if DEBUG
		/*
		foreach (InputDevice device in InputSystem.devices) {
			if (!device.CheckStateIsAtDefaultIgnoringNoise()) {
				GUILayout.TextField(device.displayName + " is being messed with");
			}
		}
		*/
#endif
	}
}
