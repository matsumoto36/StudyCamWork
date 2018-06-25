﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine;

public enum StageSelectState {
	Opening,
	ListView,
	StageContentView,
	QuitGameView,
}

public class StageSelectController : MonoBehaviour {

	public static bool movieSkip;

	public Button backButton;
	public StageWindow stageContentView;
	public QuitGameWindow quitWindow;

	public TextMesh clickStartText;
	public Image monitorEffect;

	Coroutine clickClickAnimationCoroutine;

	public Transform directionalLight;
	public float lightMoveSpeed = 1;

	StageSelectState state = StageSelectState.Opening;
	
	void Awake() {
		if(movieSkip) {
			var cam = Camera.main;
			monitorEffect.color = new Color(0, 0, 0, 0);
			cam.transform.position = new Vector3(0, 0, 20);
			cam.transform.rotation = Quaternion.identity;
			cam.orthographic = true;
			state = StageSelectState.ListView;
		}
	}
	void Start () {

		backButton.onClick.AddListener(BackButton);
		stageContentView.Hide();
		quitWindow.IsActive(false);

		var stages = FindObjectsOfType<StageMoveButton>();

		//データをロードする
		GameData.Load(stages
			.Select(item => item.loadPathName)
			.ToArray()
		);

		//初期化
		foreach(var item in stages) {
			item.Init(this);
		}

		//アニメーション再生
		clickClickAnimationCoroutine = StartCoroutine(ClickAnimation());
		StartCoroutine(TimeAnimation());

		//BGM再生
		if(AudioManager.CurrentBGMName != "") {
			AudioManager.FadeOut(2.0f);
		}
		AudioManager.FadeIn(1.0f, "town1");
	}

	void Update() {
		if(state == StageSelectState.Opening && Input.GetMouseButtonDown(0)) {
			Camera.main.GetComponent<PlayableDirector>().Play();
			state = StageSelectState.ListView;

			//アニメーションを止める
			StopCoroutine(clickClickAnimationCoroutine);
			clickStartText.color = new Color(0, 0, 0, 0);
		}
	}

	void BackButton() {

		AudioManager.PlaySE("Button3");

		switch(state) {
			case StageSelectState.ListView:
				//ゲームを終了するかきく
				quitWindow.IsActive(true);
				state = StageSelectState.QuitGameView;
				break;
			case StageSelectState.StageContentView:
				//一覧に戻る
				stageContentView.Hide();
				state = StageSelectState.ListView;
				break;
			case StageSelectState.QuitGameView:
				//ゲームを終了するのをやめる
				quitWindow.IsActive(false);
				state = StageSelectState.ListView;
				break;
			default:
				break;
		}
	}

	IEnumerator ClickAnimation() {

		var textColor = clickStartText.color;
		var speed = 1;

		while(true) {

			textColor.a = Mathf.Abs(Mathf.Sin(Time.time * speed));
			clickStartText.color = textColor;
			yield return null;
		}

	}

	IEnumerator TimeAnimation() {

		var speed = lightMoveSpeed;

		while(true) {

			directionalLight.rotation *= Quaternion.Euler(new Vector3(1 * speed * Time.deltaTime, 0, 0));

			yield return null;
		}

	}

	public void ShowStageWindow(StageMoveButton button) {
		stageContentView.Show(button);
		state = StageSelectState.StageContentView;
	}
}