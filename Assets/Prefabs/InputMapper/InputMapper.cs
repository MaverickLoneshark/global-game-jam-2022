using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMapper : MonoBehaviour {
	public static InputMapper inputMapper {
		private set;
		get;
	}

	public enum CONTROLS {
		//primary buttons
		execute = 1,
		action,
		cancel,
		special,
		//shoulder buttons
		alternative,
		extra,
		//menu & options buttons
		start,
		select,
		//directions
		up,
		down,
		left,
		right,
		//used only for internal reference
		COUNT
	}

	public int pointer_position_x { get; private set; }
	public int pointer_position_y { get; private set; }

	private InputControl[] mappings = new InputControl[(int)CONTROLS.COUNT];
	public InputControl this[int index] {
		get { return mappings[index]; }
	}

	private InputAction pointer_move = new InputAction(binding: "<Pointer>/delta");

	private void Awake() {
		if (inputMapper) {
			Destroy(gameObject);
		}
		else {
			inputMapper = this;
			DontDestroyOnLoad(gameObject);

Debug.Log(InputSystem.devices.Count + " input device(s) detected");
			pointer_move.performed += context => OnPointerMove(context);
			pointer_move.Enable();

			string debug_text;
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
}
