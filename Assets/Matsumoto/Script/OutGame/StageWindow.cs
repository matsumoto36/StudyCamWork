using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StageWindow : MonoBehaviour {

	public GameObject body;

	public Image frameImage;
	public Image stageImage;

	public Text score;
	public Text accuracy;
	public Text maxCombo;

	public Text title;

	public string loadPathName;
	public string loadStudioName;

	public Button startButton;

	void Start() {
		startButton.onClick
			.AddListener(MoveStage);
	}

	void MoveStage() {
		GameMaster.LoadPathName = loadPathName;
		GameMaster.LoadStudioName = loadStudioName;
		FindObjectOfType<TimerController>().SceneMove("GameScene");
	}

	public void Show(StageMoveButton button) {

		frameImage.sprite = button.frameImage.sprite;
		frameImage.color = button.frameImage.color;
		stageImage.sprite = button.stageImage.sprite;
		loadPathName = button.loadPathName;
		loadStudioName = button.loadStudioName;
		title.text = button.title;

		var data = GameData.stageData[loadPathName];
		score.text = data.score.ToString();

		if(data.accuracy == 1.0f) 
			accuracy.text = "100";
		else
			accuracy.text = (data.accuracy * 100).ToString("00.00");

		maxCombo.text = data.maxCombo.ToString();

		body.SetActive(true);
	}

	public void Hide() {
		body.SetActive(false);
	}
}
