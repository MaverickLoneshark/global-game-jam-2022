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

    //private void OnGUI() {
    //    string pressed_buttons = "";

    //    for (int i = 1; i < (int)InputMapper.CONTROLS.COUNT; i++) {
    //        if ((inputMapper[i] != null) && inputMapper[i].IsPressed()) {
    //            if (pressed_buttons == "") {
    //                pressed_buttons = ((InputMapper.CONTROLS)i).ToString();
    //            }
    //            else {
    //                pressed_buttons += ", " + (InputMapper.CONTROLS)i;
    //            }
    //        }
    //    }

    //    GUILayout.TextField("Pointer: " + inputMapper.pointer_position_x + ", " + inputMapper.pointer_position_y);

    //    if (pressed_buttons.Length > 0) {
    //        GUILayout.TextField(pressed_buttons + " pressed");
    //    }

    //    foreach (InputDevice device in InputSystem.devices) {
    //        if (!device.CheckStateIsAtDefaultIgnoringNoise()) {
    //            GUILayout.TextField(device.displayName + " is being messed with");
    //        }
    //    }
    //}
}
