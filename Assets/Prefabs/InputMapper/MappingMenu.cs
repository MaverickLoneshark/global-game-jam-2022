using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MappingMenu : MonoBehaviour {
	InputMapper inputMapper;
	
	// Start is called before the first frame update
	void Start() {
		inputMapper = InputMapper.inputMapper;
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnGUI() {
		string pressed_buttons = "nothing";

		for (int i = 1; i < (int)InputMapper.CONTROLS.COUNT; i++) {
			if ((inputMapper[i] != null) && inputMapper[i].IsPressed()) {
				if (pressed_buttons == "nothing") {
					pressed_buttons = ((InputMapper.CONTROLS)i).ToString();
				}
				else {
					pressed_buttons += ", " + (InputMapper.CONTROLS)i;
				}
			}
		}

		GUILayout.TextField("Pointer: " + inputMapper.pointer_position_x + ", " + inputMapper.pointer_position_y);
		GUILayout.TextField(pressed_buttons + " pressed");
	}
}
