using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMapper : MonoBehaviour {
	public static InputMapper inputMapper;
	//private UnityEngine.InputSystem.Utilities.ReadOnlyArray<Gamepad> gamepad;

	private InputAction pointer_move = new InputAction(binding: "<Pointer>/delta");
	public static int pointer_position_x { get; private set; }
	public static int pointer_position_y { get; private set; }

	public enum CONTROLS {
		execute = 1,
		action,
		cancel,
		special,
		alternative,
		extra,
		start,
		select,
		COUNT
	}

	private InputControl [] mappings = new InputControl[(int)CONTROLS.COUNT];

	private void Awake() {
		if (inputMapper) {
			Destroy(gameObject);
		}
		else {
			inputMapper = this;

			pointer_move.performed += context => OnPointerMove(context);
			pointer_move.Enable();

			string debug_text;
			Debug.Log(InputSystem.devices.Count + " input device(s) detected");

			UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputControl> all_controls;

			foreach (InputDevice device in InputSystem.devices) {
				debug_text = device.displayName + " detected: " + device.description + '\n' +
					device.usages + '\n' +
					device.valueType + '\n';

				switch (device.valueType) {
					default:
						all_controls = device.allControls;

						for (int i = 0; i < all_controls.Count; i++) {
							debug_text += '\t' + all_controls[i].displayName + '\n';
						}
					break;
				}

				Debug.Log(debug_text);
			}
		}
	}

	public void AssignControl(CONTROLS control, InputControl mapping) {
		mappings[(int)control] = mapping;
	}

	// Start is called before the first frame update
	void Start() {
		//
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnPointerMove(InputAction.CallbackContext context) {
		pointer_position_x = (int)Pointer.current.position.x.ReadValue();
		pointer_position_y = (int)Pointer.current.position.y.ReadValue();
	}

	private void OnGUI() {
		GUILayout.TextField("Pointer: " + pointer_position_x + ", " + pointer_position_y);
	}
}
