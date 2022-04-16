using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
	[SerializeField]
	private GameObject elementPrefab;

	[SerializeField]
	private int poolSize;

	private GameObject[] pool;

	private void Awake() {
		pool = new GameObject[poolSize];

		for (int i = 0; i < poolSize; i++) {
			pool[i] = GameObject.Instantiate(elementPrefab);
			pool[i].transform.SetParent(transform);
			pool[i].SetActive(false);
		}
	}

	public GameObject Instate() {
		for (int i = 0; i < poolSize; i++) {
			if (!pool[i].activeSelf) {
				pool[i].SetActive(true);

				return pool[i];
			}
		}

		return null;
	}

	public void Terminate(GameObject element) {
		element.SetActive(false);
	}
}
