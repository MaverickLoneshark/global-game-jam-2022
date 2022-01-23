using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonMapper : MonoBehaviour {
	[SerializeField]
	private InputMapper.CONTROLS control_value;

	[SerializeField]
	private InputControl mapping;

	private Button button;
	private InputMapper input_mapper;

	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	// Start is called before the first frame update
	void Start() {
		input_mapper = InputMapper.inputMapper;
		input_mapper.AssignControl(control_value, mapping);
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnClick() {
		Debug.Log("Clicked " + control_value + " button @" + InputMapper.pointer_position_x + ", " + InputMapper.pointer_position_y);
	}
}
