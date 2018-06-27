using System.Linq;
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
    public Button EndWindowBack;

    public StageWindow stageContentView;
	public QuitGameWindow quitWindow;

	public TextMesh clickStartText;
	public Image monitorEffect;

	public Transform directionalLight;
	public float lightMoveSpeed = 1;

	public bool setFadeTitleBGM;
	bool changeSetFadeTitleBGM;
	public bool setPlaySelectStageBGM;
	bool changeSsetPlaySelectStageBGM;

	Coroutine clickClickAnimationCoroutine;

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
        EndWindowBack.onClick.AddListener(BackButton);

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
			AudioManager.FadeOut(1.0f);
		}

		if(movieSkip) {
			AudioManager.FadeIn(1.0f, "bgm_maoudamashii_cyber29");
		}
		else {
			AudioManager.FadeIn(1.0f, "town1");
		}
	}

	void Update() {

		//アニメーション制御用
		if(setFadeTitleBGM && !changeSetFadeTitleBGM) {
			changeSetFadeTitleBGM = true;
			Debug.Log("FadeTitleBGM");
			AudioManager.FadeOut(5.0f);
		}

		if(setPlaySelectStageBGM && !changeSsetPlaySelectStageBGM) {
			changeSsetPlaySelectStageBGM = true;
			Debug.Log("FadeStageSelectBGM");
			AudioManager.FadeIn(3.0f, "bgm_maoudamashii_cyber29");
		}


		if(state == StageSelectState.Opening && Input.GetMouseButtonDown(0)) {
			Camera.main.GetComponent<PlayableDirector>().Play();
			state = StageSelectState.ListView;

			//アニメーションを止める
			StopCoroutine(clickClickAnimationCoroutine);
			clickStartText.color = new Color(0, 0, 0, 0);
		}
	}

	void BackButton() {


		switch(state) {
			case StageSelectState.ListView:
				//ゲームを終了するかきく
				AudioManager.PlaySE("cancel5");
				quitWindow.IsActive(true);
				state = StageSelectState.QuitGameView;
				break;
			case StageSelectState.StageContentView:
				//一覧に戻る
				AudioManager.PlaySE("click03");
				stageContentView.Hide();
				state = StageSelectState.ListView;
				break;
			case StageSelectState.QuitGameView:
				//ゲームを終了するのをやめる
				AudioManager.PlaySE("click03");
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