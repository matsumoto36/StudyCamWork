using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class QuitGameWindow : MonoBehaviour {

	public Button quitButton;
	public Button titleButton;
	public GameObject body;

	void Start() {
		quitButton.onClick.AddListener(() => {
			AudioManager.PlaySE("click03");
			Application.Quit();
			});

		titleButton.onClick.AddListener(() => {
			AudioManager.PlaySE("click03");
			StageSelectController.movieSkip = false;
			FindObjectOfType<TimerController>().SceneMove("TitleScene");

		});

	}

	public void IsActive(bool enable) {
		body.SetActive(enable);
	}

}
