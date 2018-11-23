using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Playables;
using UnityEngine;

enum StageSelectState {
	Title,
	Opening,
	ListView,
	StageContentView,
	QuitGameView,
}

/// <summary>
/// タイトル画面からステージ選択画面に
/// 切り替えるまでを管理する
/// </summary>
public class StageSelectController : MonoBehaviour {

	public static bool MovieSkip;

	public CanvasGroup SelectStageGroup;
	[HideInInspector] public Button BackButton;
	public Button EndWindowBack;

    public StageWindow StageContentView;
	public QuitGameWindow QuitWindow;

	public TextMesh ClickStartText;
	public Image MonitorEffect;

	public Transform DirectionalLight;
	public float LightMoveSpeed = 1;

	public bool SetFadeTitleBgm;

	private bool _changeSetFadeTitleBgm;
	public bool SetPlaySelectStageBgm;
	private bool _changeSetPlaySelectStageBgm;
	public bool SetEnableButton;
	private bool _changeSetEnableButton;

	public VideoPlayer VideoPlayer;
	public Text MovieText;
	private Color _movieTextColor;
	public float MovieStartTime = 10.0f;

	private PlayableDirector _titleAnimation;

	private Coroutine _clickClickAnimationCoroutine;

	private StageSelectState _state;

	private void Awake() {

		_titleAnimation = Camera.main.GetComponent<PlayableDirector>();

		SelectStageGroup.interactable = false;
		_state = StageSelectState.Title;

		if(MovieSkip) {
			var cam = Camera.main;
			MonitorEffect.color = new Color(0, 0, 0, 0);
			cam.transform.position = new Vector3(0, 0, 20);
			cam.transform.rotation = Quaternion.identity;
			cam.orthographic = true;
			_state = StageSelectState.ListView;
			SelectStageGroup.interactable = true;
		}

		//ムービー待機開始
		StartCoroutine(GameMovieLoop());
	}
	private void Start () {

		BackButton.onClick.AddListener(OnPressBackButton);
        EndWindowBack.onClick.AddListener(OnPressBackButton);

		StageContentView.Hide();
		QuitWindow.IsActive(false);

		var stages = FindObjectsOfType<StageMoveButton>();

		//データをロードする
		GameData.Load(stages
			.Select(item => item.LoadPathName)
			.ToArray()
		);

		//初期化
		foreach(var item in stages) {
			item.Init(this);
		}

		//アニメーション再生
		_clickClickAnimationCoroutine = StartCoroutine(ClickAnimation());
		StartCoroutine(TimeAnimation());

		//BGM再生
		if(AudioManager.CurrentBGMName != "") {
			AudioManager.FadeOut(1.0f);
		}

		AudioManager.FadeIn(1.0f, MovieSkip ? "bgm_maoudamashii_cyber29" : "town1");
	}

	private void Update() {

		//アニメーション制御用
		if(SetFadeTitleBgm && !_changeSetFadeTitleBgm) {
			_changeSetFadeTitleBgm = true;
			Debug.Log("FadeTitleBGM");
			AudioManager.FadeOut(5.0f);
		}

		if(SetPlaySelectStageBgm && !_changeSetPlaySelectStageBgm) {
			_changeSetPlaySelectStageBgm = true;
			Debug.Log("FadeStageSelectBGM");
			AudioManager.FadeIn(3.0f, "bgm_maoudamashii_cyber29");
		}

		if(_state == StageSelectState.Title && Input.GetMouseButtonDown(0)) {
			_titleAnimation.Play();
			_state = StageSelectState.Opening;

			//アニメーションを止める
			StopCoroutine(_clickClickAnimationCoroutine);
			ClickStartText.color = new Color(0, 0, 0, 0);
		}

		if (!SetEnableButton || _changeSetEnableButton) return;

		_changeSetEnableButton = true;
		_state = StageSelectState.ListView;
		SelectStageGroup.interactable = true;
	}

	/// <summary>
	/// 戻るボタンが押されたときの動作
	/// </summary>
	private void OnPressBackButton() {

		switch(_state) {
			case StageSelectState.Title:
				break;
			case StageSelectState.Opening:
				break;
			case StageSelectState.ListView:
				//ゲームを終了するかきく
				AudioManager.PlaySE("cancel5");
				QuitWindow.IsActive(true);
				_state = StageSelectState.QuitGameView;
				break;
			case StageSelectState.StageContentView:
				//一覧に戻る
				AudioManager.PlaySE("click03");
				StageContentView.Hide();
				_state = StageSelectState.ListView;
				break;
			case StageSelectState.QuitGameView:
				//ゲームを終了するのをやめる
				AudioManager.PlaySE("click03");
				QuitWindow.IsActive(false);
				_state = StageSelectState.ListView;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	/// <summary>
	/// クリックを促すアニメーション
	/// </summary>
	/// <returns></returns>
	private IEnumerator ClickAnimation() {

		const int speed = 1;
		var textColor = ClickStartText.color;

		while(true) {

			textColor.a = Mathf.Abs(Mathf.Sin(Time.time * speed));
			ClickStartText.color = textColor;
			yield return null;
		}

	}

	/// <summary>
	/// タイトル画面の昼夜を切り替える
	/// </summary>
	/// <returns></returns>
	private IEnumerator TimeAnimation() {

		var speed = LightMoveSpeed;

		while(true) {

			DirectionalLight.rotation *= Quaternion.Euler(new Vector3(1 * speed * Time.deltaTime, 0, 0));

			yield return null;
		}

	}

	/// <summary>
	/// 動画を再生してマウスを待機する
	/// </summary>
	/// <returns></returns>
	private IEnumerator GameMovieLoop() {

		var t = 0.0f;
		var prevMousePosition = Input.mousePosition;
		var isPlayingMovie = false;
		var videoImage = VideoPlayer.GetComponent<RawImage>();

		_movieTextColor = MovieText.color;
		Coroutine textAnimCol = null;

		//ちらつきを抑えるための動作
		VideoPlayer.Play();
		VideoPlayer.Pause();

		while(true) {
			yield return null;

			var mouseDelta = Input.mousePosition - prevMousePosition;
			prevMousePosition = Input.mousePosition;

			if(isPlayingMovie) {
				//ムービー再生中にマウスをいじった場合
				if ((!(mouseDelta.magnitude > 0.01f) && !Input.GetMouseButtonDown(0))) continue;

				isPlayingMovie = false;

				if(textAnimCol != null) StopCoroutine(textAnimCol);
				MovieText.gameObject.SetActive(false);

				videoImage.color = new Color(1, 1, 1, 0);
				VideoPlayer.Pause();

				Cursor.visible = true;
				AudioManager.BGMVolume = 0;
			}
			else {

				if(_state != StageSelectState.Title) yield break;

				//再生待ち時間が超えた場合
				if (!((t += Time.deltaTime) > MovieStartTime)) continue;

				t = 0;
				isPlayingMovie = true;
				VideoPlayer.time = 0;
				VideoPlayer.Play();
				yield return new WaitForSeconds(1.0f);
				videoImage.color = new Color(1, 1, 1, 1);

				MovieText.gameObject.SetActive(true);
				textAnimCol = StartCoroutine(MovieTextAnimation());

				Cursor.visible = false;
				AudioManager.BGMVolume = -80;
			}
		}
	}

	/// <summary>
	/// ムービー中に表示するテキスト
	/// </summary>
	/// <returns></returns>
	private IEnumerator MovieTextAnimation() {

		const int speed = 1;
		var textColor = _movieTextColor;

		while(true) {

			textColor.a = Mathf.Abs(Mathf.Sin(Time.time * speed));
			MovieText.color = textColor;
			yield return null;
		}

	}

	/// <summary>
	/// ステージ選択のウィンドウを表示する
	/// </summary>
	/// <param name="button"></param>
	public void ShowStageWindow(StageMoveButton button) {
		StageContentView.Show(button);
		_state = StageSelectState.StageContentView;
	}
}