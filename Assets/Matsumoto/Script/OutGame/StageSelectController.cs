using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Playables;
using UnityEngine;

public enum StageSelectState {
	Title,
	Opening,
	ListView,
	StageContentView,
	QuitGameView,
}

public class StageSelectController : MonoBehaviour {

	public static bool movieSkip;

	public CanvasGroup selectStageGroup;
	public Button backButton;
    public Button endWindowBack;

    public StageWindow stageContentView;
	public QuitGameWindow quitWindow;

	public TextMesh clickStartText;
	public Image monitorEffect;

	public Transform directionalLight;
	public float lightMoveSpeed = 1;

	public bool setFadeTitleBGM;
	bool changeSetFadeTitleBGM;
	public bool setPlaySelectStageBGM;
	bool changeSetPlaySelectStageBGM;
	public bool setEnableButton;
	bool changeSetEnableButton;

	public VideoPlayer videoPlayer;
	public Text movieText;
	Color movieTextColor;
	public float movieStartTime = 10.0f;

	PlayableDirector titleAnimation;

	Coroutine clickClickAnimationCoroutine;

	StageSelectState state;
	
	void Awake() {

		titleAnimation = Camera.main.GetComponent<PlayableDirector>();

		selectStageGroup.interactable = false;
		state = StageSelectState.Title;

		if(movieSkip) {
			var cam = Camera.main;
			monitorEffect.color = new Color(0, 0, 0, 0);
			cam.transform.position = new Vector3(0, 0, 20);
			cam.transform.rotation = Quaternion.identity;
			cam.orthographic = true;
			state = StageSelectState.ListView;
			selectStageGroup.interactable = true;
		}

		//ムービー待機開始
		StartCoroutine(GameMovieLoop());
	}
	void Start () {

		backButton.onClick.AddListener(BackButton);
        endWindowBack.onClick.AddListener(BackButton);


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

		if(setPlaySelectStageBGM && !changeSetPlaySelectStageBGM) {
			changeSetPlaySelectStageBGM = true;
			Debug.Log("FadeStageSelectBGM");
			AudioManager.FadeIn(3.0f, "bgm_maoudamashii_cyber29");
		}

		if(state == StageSelectState.Title && Input.GetMouseButtonDown(0)) {
			titleAnimation.Play();
			state = StageSelectState.Opening;

			//アニメーションを止める
			StopCoroutine(clickClickAnimationCoroutine);
			clickStartText.color = new Color(0, 0, 0, 0);
		}

		if(setEnableButton && !changeSetEnableButton) {
			changeSetEnableButton = true;
			state = StageSelectState.ListView;
			selectStageGroup.interactable = true;
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

	IEnumerator GameMovieLoop() {

		var t = 0.0f;
		var prevMousePosition = Input.mousePosition;
		var isPlayingMovie = false;
		var videoImage = videoPlayer.GetComponent<RawImage>();

		movieTextColor = movieText.color;
		Coroutine textAnimCol = null;

		//ちらつきを抑えるための動作
		videoPlayer.Play();
		videoPlayer.Pause();

		while(true) {
			yield return null;

			var mouseDelta = Input.mousePosition - prevMousePosition;
			prevMousePosition = Input.mousePosition;

			if(isPlayingMovie) {
				//ムービー再生中にマウスをいじった場合
				if((mouseDelta.magnitude > 0.01f || Input.GetMouseButtonDown(0))) {
					isPlayingMovie = false;

					if(textAnimCol != null) StopCoroutine(textAnimCol);
					movieText.gameObject.SetActive(false);

					videoImage.color = new Color(1, 1, 1, 0);
					videoPlayer.Pause();

					Cursor.visible = true;
					AudioManager.BGMVolume = 0;
				}
			}
			else {

				if(state != StageSelectState.Title) yield break;

				//再生待ち時間が超えた場合
				if((t += Time.deltaTime) > movieStartTime && !isPlayingMovie) {
					t = 0;
					isPlayingMovie = true;
					videoPlayer.time = 0;
					videoPlayer.Play();
					yield return new WaitForSeconds(1.0f);
					videoImage.color = new Color(1, 1, 1, 1);

					movieText.gameObject.SetActive(true);
					textAnimCol = StartCoroutine(MovieTextAnimation());

					Cursor.visible = false;
					AudioManager.BGMVolume = -80;
				}
			}
		}
	}

	IEnumerator MovieTextAnimation() {

		var textColor = movieTextColor;
		var speed = 1;

		while(true) {

			textColor.a = Mathf.Abs(Mathf.Sin(Time.time * speed));
			movieText.color = textColor;
			yield return null;
		}

	}

	public void ShowStageWindow(StageMoveButton button) {
		stageContentView.Show(button);
		state = StageSelectState.StageContentView;
	}
}