using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMapper : MonoBehaviour {
	public static InputMapper inputMapper { private set; get; }

	public enum CONTROLS {
		//primary buttons
		execute = 0,
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
		//internal counter (must be at end!)
		COUNT
	}

	public int pointer_position_x { get; private set; } = 0;
	public int pointer_position_y { get; private set; } = 0;

	private GameObject pauseMenu;
	private GameObject mappingMenu;
	private bool holding_pause = false;

	[SerializeField][Header("0: Execute Action")]
	private InputAction execute_action = new InputAction(name: CONTROLS.execute.ToString(), binding: "<Keyboard>/z");

	[SerializeField][Header("1: Action Action")]
	private InputAction action_action = new InputAction(name: CONTROLS.action.ToString(), binding: "<Keyboard>/x");

	[SerializeField][Header("2: Cancel Action")]
	private InputAction cancel_action = new InputAction(name: CONTROLS.cancel.ToString(), binding: "<Keyboard>/space");

	[SerializeField][Header("3: Special Action")]
	private InputAction special_action = new InputAction(name: CONTROLS.special.ToString(), binding: "<Keyboard>/c");

	[SerializeField][Header("4: Alternative Action")]
	private InputAction alternative_action = new InputAction(name: CONTROLS.alternative.ToString(), binding: "<Keyboard>/ctrl");

	[SerializeField][Header("5: Extra Action")]
	private InputAction extra_action = new InputAction(name: CONTROLS.extra.ToString(), binding: "<Keyboard>/shift");

	[SerializeField][Header("6: Start Action")]
	private InputAction start_action = new InputAction(name: CONTROLS.start.ToString(), binding: "<Keyboard>/enter");

	[SerializeField][Header("7: Select Action")]
	private InputAction select_action = new InputAction(name: CONTROLS.select.ToString(), binding: "<Keyboard>/quote");

	[SerializeField][Header("8: Up Action")]
	private InputAction up_action = new InputAction(name: CONTROLS.up.ToString(), binding: "<Keyboard>/uparrow");

	[SerializeField][Header("9: Down Action")]
	private InputAction down_action = new InputAction(name: CONTROLS.down.ToString(), binding: "<Keyboard>/downarrow");

	[SerializeField][Header("10: Left Action")]
	private InputAction left_action = new InputAction(name: CONTROLS.left.ToString(), binding: "<Keyboard>/leftarrow");

	[SerializeField][Header("11: Right Action")]
	private InputAction right_action = new InputAction(name: CONTROLS.right.ToString(), binding: "<Keyboard>/rightarrow");

	private InputAction[] boundAction = new InputAction[(int)CONTROLS.COUNT];

	private Dictionary<string, CONTROLS> name2Control = new Dictionary<string, CONTROLS> {
		{ ((CONTROLS)0).ToString(), (CONTROLS)0 },
		{ ((CONTROLS)1).ToString(), (CONTROLS)1 },
		{ ((CONTROLS)2).ToString(), (CONTROLS)2 },
		{ ((CONTROLS)3).ToString(), (CONTROLS)3 },
		{ ((CONTROLS)4).ToString(), (CONTROLS)4 },
		{ ((CONTROLS)5).ToString(), (CONTROLS)5 },
		{ ((CONTROLS)6).ToString(), (CONTROLS)6 },
		{ ((CONTROLS)7).ToString(), (CONTROLS)7 },
		{ ((CONTROLS)8).ToString(), (CONTROLS)8 },
		{ ((CONTROLS)9).ToString(), (CONTROLS)9 },
		{ ((CONTROLS)10).ToString(), (CONTROLS)10 },
		{ ((CONTROLS)11).ToString(), (CONTROLS)11 },
	};

	private InputAction pointer_move = new InputAction(binding: "<Pointer>/delta");

	public InputAction getBoundAction(CONTROLS control) {
		return boundAction[(int)control];
	}

	private bool[] pressed = new bool[(int)CONTROLS.COUNT];
	public bool this[int index] {
		get { return pressed[index]; }
	}

	private void Awake() {
		if (inputMapper) {
			Destroy(gameObject);
		}
		else {
			inputMapper = this;
			DontDestroyOnLoad(gameObject);
			pauseMenu = transform.Find("PauseCanvas").Find("OptionsMenu").gameObject;
			mappingMenu = pauseMenu.transform.Find("MappingMenu").gameObject;

			InputSystem.Update();

			boundAction[0] = execute_action;
			boundAction[1] = action_action;
			boundAction[2] = cancel_action;
			boundAction[3] = special_action;
			boundAction[4] = alternative_action;
			boundAction[5] = extra_action;
			boundAction[6] = start_action;
			boundAction[7] = select_action;
			boundAction[8] = up_action;
			boundAction[9] = down_action;
			boundAction[10] = left_action;
			boundAction[11] = right_action;

			for (int i = 0; i < (int)CONTROLS.COUNT; i++) {
				boundAction[i].performed += (context) => {
					try {
						pressed[(int)name2Control[context.action.name]] = context.ReadValueAsButton();
					}
					catch {
						Vector2 deltaVector = context.ReadValue<Vector2>();

						switch (name2Control[context.action.name]) {
							case CONTROLS.up:
								pressed[(int)name2Control[context.action.name]] = deltaVector.y > 0.4f;
							break;

							case CONTROLS.right:
								pressed[(int)name2Control[context.action.name]] = deltaVector.x > 0.4f;
							break;

							case CONTROLS.down:
								pressed[(int)name2Control[context.action.name]] = deltaVector.y < -0.4f;
							break;

							case CONTROLS.left:
								pressed[(int)name2Control[context.action.name]] = deltaVector.x < -0.4f;
							break;

							default:
							break;
						}
					}
				};
				
				boundAction[i].canceled += (context) => {
					pressed[(int)name2Control[context.action.name]] = false;
				};

				boundAction[i].Enable();
			}

			pointer_move.performed += context => OnPointerMove(context);
			pointer_move.Enable();

#if DEBUG
Debug.Log(InputSystem.devices.Count + " input device(s) detected");
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
#endif
		}
	}

	// Start is called before the first frame update
	void Start() {
		//
	}

	// Update is called once per frame
	void Update() {
		if (inputMapper[(int)CONTROLS.start]) {
			if (!holding_pause) {
				if (!mappingMenu.activeSelf && UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 1) {
					pauseMenu.SetActive(!pauseMenu.activeSelf);
					holding_pause = true;

					if (!pauseMenu.activeSelf) {
						Time.timeScale = 1.0f;
					}
				}
			}
		}
		else {
			holding_pause = false;
		}
	}

	private void OnPointerMove(InputAction.CallbackContext context) {
		pointer_position_x = (int)Pointer.current.position.x.ReadValue();
		pointer_position_y = (int)Pointer.current.position.y.ReadValue();
	}
}
