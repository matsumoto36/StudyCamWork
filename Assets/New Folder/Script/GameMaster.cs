using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

	public static GameMaster gameMaster;

	public GameBalanceData gameBalanceData = new GameBalanceData();

	public Button startButton;
	public Text countDownText;
   
    public StageController stageController;
    public MouseCamera mouseCamera;

	public event Action OnGameStart;
	public event Action OnGameStartCountDown;
	public event Action OnGameOver;
	public event Action OnGameClear;

	bool isGameStart;

    // Use this for initialization
    void Awake () {
		gameMaster = this;
		countDownText.text = "";
	}

	void Start() {

		stageController.InitStage();

		startButton.onClick.AddListener(GameStart);

		OnGameStartCountDown += () => startButton.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update () {

		if(!isGameStart) return;

		stageController.StageUpdate();
    }
    
	public void GameStart() {

		StartCoroutine(CountDown());

	}

	public void GameClear() {
		if(OnGameClear != null) OnGameClear();
		isGameStart = false;
		countDownText.text = "GameClear";
	}

	public void GameOver() {
		if(OnGameOver != null) OnGameOver();
		isGameStart = false;
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
        isGameStart = true;
	}
}
