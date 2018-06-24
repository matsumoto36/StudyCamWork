using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class QuitGameWindow : MonoBehaviour {

	public Button quitButton;
	public GameObject body;

	void Start() {
		quitButton.onClick.AddListener(() => {
			AudioManager.PlaySE("Button3");
			Application.Quit();
			});

	}

	public void IsActive(bool enable) {
		body.SetActive(enable);
	}

}
