using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButton : MonoBehaviour {
    [SerializeField]
	GameObject targetMenuObject;

	private Button button;
	private void Awake() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);

		if (!targetMenuObject) {
			targetMenuObject = transform.parent.gameObject;
		}
	}

	// Start is called before the first frame update
	void Start() {
		//
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnClick() {
        Debug.Log("hi");
		targetMenuObject.SetActive(false);
	}
}
