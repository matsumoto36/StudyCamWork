using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState {
	BeforeStart,
	Starting,
	Playing,
	Ending,
	AfterEnd,
}

public class GameMaster : MonoBehaviour {

	public const string STUDIO_PREFAB_BASE_PATH = "Prefab/Stage/StudioSet/";
	public const string PATH_PREFAB_BASE_PATH = "Prefab/Stage/Path/";
	const string PLAYER_PREFAB_PATH = "Prefab/Player";


	public GameBalanceData GameBalanceData { get; private set; }

	public static string LoadPathName = "";
	public string loadPathName;
	public static string LoadStudioName = "";
	public string loadStudioName;

	public static List<StageInfo> nextStageList;
	static int counter = 0;

	public Text countDownText;
   
    public StageController stageController;
    public MouseCamera mouseCamera;

	public event Action OnGameStart;
	public event Action OnGameStartCountDown;
	public event Action OnGameOver;
	public event Action OnGameClear;

	bool isSceneMoving;

	public GameState State { get; private set; }

	static GameMaster instance;
	public static GameMaster Instance {
		get {
			if(!instance) {
				instance = FindObjectOfType<GameMaster>();
			}
			return instance;
		}
	}

	// Use this for initialization
	void Awake () {
		if(Instance != this) Destroy(gameObject);
		instance = this;

		//サウンドの読み込み
		AudioManager.Load();
		//パーティクルの読み込み
		ParticleManager.Load();

		countDownText.text = "";

		//BGM再生
		if(AudioManager.CurrentBGMName != "StageBGM1") {
			AudioManager.CrossFade(1.0f, "StageBGM1");
		}
	}

	void Start() {

		if(LoadPathName != "") loadPathName = LoadPathName;
		if(LoadStudioName != "") loadStudioName = LoadStudioName;

		//スタジオの生成
		Instantiate(Resources.Load<GameObject>(STUDIO_PREFAB_BASE_PATH + loadStudioName));

		//ステージの生成
		var stage = Instantiate(Resources.Load<GameObject>(PATH_PREFAB_BASE_PATH + loadPathName));
		var path = stage.GetComponent<Bezier2D>();
		var gimmickManager = stage.GetComponent<GimmickManager>();

		GameBalanceData = stage.GetComponent<GameBalanceData>();

		//プレイヤーの生成
		var player = Instantiate(Resources.Load<Player>(PLAYER_PREFAB_PATH));

		stageController.InitStage(player, path, gimmickManager);
	}

	// Update is called once per frame
	void Update () {

		if(State == GameState.BeforeStart) {
			if(Input.GetMouseButtonDown(0)) GameStart();
		}

		stageController.StageUpdate();
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

		Debug.Log("DataSeve");
		Debug.Log("Combo " + MouseCamera.ComboMax);

		//データのセーブ
		if(GameData.stageData != null) {
			var data = GameData.stageData[loadPathName];
			if(data.score < MouseCamera.Score) {
				data.score = MouseCamera.Score;
				data.accuracy = MouseCamera.Accuracy;
				data.maxCombo = MouseCamera.ComboMax;
				GameData.stageData[loadPathName] = data;
				GameData.Save();
			}
		}

		countDownText.text = "GameClear";

		State = GameState.AfterEnd;
	}

	public void GameOver() {

		State = GameState.Ending;

		if(OnGameOver != null) OnGameOver();
		countDownText.text = "GameOver";

		State = GameState.AfterEnd;
	}

	IEnumerator CountDown() {

		if(OnGameStartCountDown != null)
			OnGameStartCountDown();

		yield return new WaitForSeconds(1);
		AudioManager.PlaySE("CountDown");
		countDownText.text = "3";
		yield return new WaitForSeconds(1);
		countDownText.text = "2";
		yield return new WaitForSeconds(1);
		countDownText.text = "1";
		yield return new WaitForSeconds(1);
		countDownText.text = "Queue";
		yield return new WaitForSeconds(1);

		countDownText.text = "";
		if(OnGameStart != null)
        {

            OnGameStart();
        }

		State = GameState.Playing;
	}

	public void NextScene() {

		var nextStageInfo = nextStageList[counter];
		if(nextStageInfo == null) return;

		if(isSceneMoving) return;
		isSceneMoving = true;

		LoadPathName = nextStageInfo.pathName;
		LoadStudioName = nextStageInfo.studioName;

		counter++;

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}

	public bool CanMoveNextStage() {
		if(nextStageList == null) return false;
		return nextStageList[counter] != null;
	}

	public void MoveSelectScene() {

		if(isSceneMoving) return;
		isSceneMoving = true;

		StageSelectController.movieSkip = true;

		if(counter > 0) {
			SelectWindowActive.activeWindowIndex = nextStageList[counter - 1].windowIndex;
		}

		FindObjectOfType<TimerController>()
			.SceneMove("TitleScene");

	}

	public void Retry() {

		if(isSceneMoving) return;
		isSceneMoving = true;

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}
}
