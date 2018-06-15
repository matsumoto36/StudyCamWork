using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

	public const string STUDIO_PREFAB_BASE_PATH = "Prefab/Stage/StudioSet/";
	public const string PATH_PREFAB_BASE_PATH = "Prefab/Stage/Path/";
	const string PLAYER_PREFAB_PATH = "Prefab/Player";


	public GameBalanceData GameBalanceData { get; private set; }

	public static string LoadStudioName = "";
	public string loadStudioName;
	public static string LoadPathName = "";
	public string loadPathName;

	public Text countDownText;
   
    public StageController stageController;
    public MouseCamera mouseCamera;

	public event Action OnGameStart;
	public event Action OnGameStartCountDown;
	public event Action OnGameOver;
	public event Action OnGameClear;

	bool isGameStart;

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
		countDownText.text = "";
	}

	void Start() {

		if(LoadStudioName != "") loadStudioName = LoadStudioName;
		if(LoadPathName != "") loadPathName = LoadPathName;

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

		if(!isGameStart) {
			if(Input.GetMouseButtonDown(0)) GameStart();
		}

		stageController.StageUpdate();
    }
    
	public void GameStart() {

		isGameStart = true;
		StartCoroutine(CountDown());

	}

	public void GameClear() {
		if(OnGameClear != null) OnGameClear();
		countDownText.text = "GameClear";
	}

	public void GameOver() {
		if(OnGameOver != null) OnGameOver();
		countDownText.text = "GameOver";
	}

	IEnumerator CountDown() {

		if(OnGameStartCountDown != null)
			OnGameStartCountDown();

		yield return new WaitForSeconds(1);
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
            //Debug.Log("a");
        }
	}
}
