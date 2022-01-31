using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour {
	private Button button;

	// Start is called before the first frame update
	void Start() {
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	// Update is called once per frame
	void Update() {
		//
	}

	private void OnClick() {
		Time.timeScale = 1.0f;
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		transform.parent.gameObject.SetActive(false);
	}
}
