using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameState {
	BeforeStart,
	Starting,
	Playing,
	Ending,
	AfterEnd,
}

/// <summary>
/// ゲームの進行管理をする
/// </summary>
public class GameMaster : MonoBehaviour {

	public const string StudioPrefabBasePath = "Prefab/Stage/StudioSet/";
	public const string PathPrefabBasePath = "Prefab/Stage/Path/";

	private const string PlayerPrefabPath = "Prefab/Player";

	public static bool IsTestPlayMode;

	public static string LoadPathName = "";
	private string _loadPathName;
	public static string LoadStudioName = "";
	private string _loadStudioName;

	public static List<StageInfo> NextStageList;
	private static int _counter = 0;

	public Text CountDownText;

	public StageController StageController;
	public MouseCamera MouseCamera;

	private bool _isSceneMoving;

	public event Action OnGameStart;
	public event Action OnGameStartCountDown;
	public event Action OnGameOver;
	public event Action OnGameClear;

	public GameState State { get; private set; }

	private static GameMaster _instance;
	public static GameMaster Instance {
		get {
			if(!_instance) {
				_instance = FindObjectOfType<GameMaster>();
			}
			return _instance;
		}
	}

	public GameBalanceData GameBalanceData { get; private set; }

	// Use this for initialization
	private void Awake() {
		if(Instance != this) Destroy(gameObject);
		_instance = this;

		//サウンドの読み込み
		AudioManager.Load();
		//パーティクルの読み込み
		ParticleManager.Load();

		CountDownText.text = "";

		//BGM再生
		if(AudioManager.CurrentBGMName != "StageBGM1") {
			AudioManager.CrossFade(1.0f, "StageBGM1");
		}
	}

	private void Start() {

		if(LoadPathName != "") _loadPathName = LoadPathName;
		if(LoadStudioName != "") _loadStudioName = LoadStudioName;

		//スタジオの生成
		Instantiate(Resources.Load<GameObject>(StudioPrefabBasePath + _loadStudioName));

		//ステージの生成
		var stage = Instantiate(Resources.Load<GameObject>(PathPrefabBasePath + _loadPathName));
		var path = stage.GetComponent<Bezier2D>();
		var gimmickManager = stage.GetComponent<GimmickManager>();

		GameBalanceData = stage.GetComponent<GameBalanceData>();

		//プレイヤーの生成
		var player = Instantiate(Resources.Load<Player>(PlayerPrefabPath));

		StageController.InitStage(player, path, gimmickManager);
	}

	// Update is called once per frame
	private void Update() {

		if(State == GameState.BeforeStart) {
			if(Input.GetMouseButtonDown(0)) GameStart();
		}

		StageController.StageUpdate();
	}

	public void GameStart() {

		State = GameState.Starting;
		StartCoroutine(CountDown());

	}

	public void GameClear() {

		State = GameState.Ending;

		if(MouseCamera.Accuracy < 1.0f) {
			//通常クリア
			AudioManager.PlaySE("people-performance-cheer2");
		}
		else {
			//パーフェクト
			AudioManager.PlaySE("people-performance-cheer1");
		}

		if(OnGameClear != null) OnGameClear();

		if(!IsTestPlayMode) {

			var mouseCamera = FindObjectOfType<MouseCamera>();
			if(mouseCamera) {

				Debug.Log("DataSave");
				Debug.Log("Combo " + mouseCamera.ComboMax);

				//データのセーブ
				if(GameData.StageData != null) {
					var data = GameData.StageData[_loadPathName];
					if(data.Score < mouseCamera.Score) {
						data.Score = mouseCamera.Score;
						data.Accuracy = mouseCamera.Accuracy;
						data.MaxCombo = mouseCamera.ComboMax;
						GameData.StageData[_loadPathName] = data;
						GameData.Save();
					}
				}
			}
		}

		CountDownText.text = "GameClear";

		State = GameState.AfterEnd;
	}

	public void GameOver() {

		State = GameState.Ending;

		if(OnGameOver != null) OnGameOver();
		CountDownText.text = "GameOver";

		State = GameState.AfterEnd;
	}

	private IEnumerator CountDown() {

		if(OnGameStartCountDown != null)
			OnGameStartCountDown();

		yield return new WaitForSeconds(1);
		AudioManager.PlaySE("CountDown");
		CountDownText.text = "3";
		yield return new WaitForSeconds(1);
		CountDownText.text = "2";
		yield return new WaitForSeconds(1);
		CountDownText.text = "1";
		yield return new WaitForSeconds(1);
		CountDownText.text = "Queue";
		yield return new WaitForSeconds(1);

		CountDownText.text = "";
		if(OnGameStart != null) {

			OnGameStart();
		}

		State = GameState.Playing;
	}

	public static void SetNextStage(List<StageInfo> stageInfo) {
		NextStageList = stageInfo;
		_counter = 0;
	}

	public void NextScene() {

		var nextStageInfo = NextStageList[_counter];
		if(nextStageInfo == null) return;

		if(_isSceneMoving) return;
		_isSceneMoving = true;

		LoadPathName = nextStageInfo.PathName;
		LoadStudioName = nextStageInfo.StudioName;

		_counter++;

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}

	public bool CanMoveNextStage() {
		if(NextStageList == null) return false;
		return NextStageList[_counter] != null;
	}

	public void MoveSelectScene() {

		if(_isSceneMoving) return;
		_isSceneMoving = true;

		Cursor.visible = true;

		StageSelectController.MovieSkip = true;

		if(_counter > 0) {
			SelectWindowActive.ActiveWindowIndex = NextStageList[_counter - 1].WindowIndex;
		}

		FindObjectOfType<TimerController>()
			.SceneMove("TitleScene");

	}

	public void Retry() {

		if(_isSceneMoving) return;
		_isSceneMoving = true;

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}
}
