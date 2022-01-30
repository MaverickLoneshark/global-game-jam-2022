using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonMapper : MonoBehaviour {
	[SerializeField]
	private InputMapper.CONTROLS control_value;

	private InputAction bound_action;
	private Button button;
	private Text button_text;
	private InputMapper input_mapper;
	private InputActionRebindingExtensions.RebindingOperation rebinding_operation;

	private void Awake() {
		button = GetComponent<Button>();
		button_text = transform.Find("Text").GetComponent<Text>();
		button.onClick.AddListener(OnClick);
	}

	// Start is called before the first frame update
	void Start() {
		input_mapper = InputMapper.inputMapper;
		bound_action = input_mapper.getBoundAction(control_value);
		button_text.text = bound_action.controls[0].displayName;
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnClick() {
		bound_action.Disable();
		rebinding_operation = bound_action.PerformInteractiveRebinding().WithTargetBinding(0).Start();
		button_text.text = "Assigning...";

		rebinding_operation.OnComplete(delegate {
			button_text.text = rebinding_operation.selectedControl.displayName;
			bound_action.Enable();
			rebinding_operation.Dispose();

			return;
		});
	}
}
