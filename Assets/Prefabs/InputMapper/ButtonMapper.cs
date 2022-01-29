using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonMapper : MonoBehaviour {
	[SerializeField]
	private InputMapper.CONTROLS control_value;

	[SerializeField]
	private InputAction bound_action;

	private InputControl mapping;
	private Button button;
	private Text button_text;
	private InputMapper input_mapper;
	private bool rebinding = false;
	private InputActionRebindingExtensions.RebindingOperation rebinding_operation;

	private void Awake() {
		button = GetComponent<Button>();
		button_text = transform.Find("Text").GetComponent<Text>();
		button.onClick.AddListener(OnClick);
		mapping = bound_action.controls[0];
	}

	// Start is called before the first frame update
	void Start() {
		input_mapper = InputMapper.inputMapper;
		input_mapper.AssignControl(control_value, mapping);
		button_text.text = mapping.displayName;
	}

	// Update is called once per frame
	void Update() {
		if (rebinding) {
			button_text.text = "Assigning...";
		}
	}

	private void OnClick() {
		rebinding_operation = bound_action.PerformInteractiveRebinding().Start();
		rebinding = true;

		rebinding_operation.OnApplyBinding(delegate {
			mapping = rebinding_operation.selectedControl;
			input_mapper.AssignControl(control_value, mapping);
			button_text.text = mapping.displayName;

			rebinding = false;
			rebinding_operation.Dispose();

			return;
		});
	}
}
