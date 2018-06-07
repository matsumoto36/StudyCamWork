using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ内の処理の順番を制御するクラス
/// </summary>
public class StageController : MonoBehaviour {

	public GimmickManager gimmickManager;
	public MouseCamera mouseCamera;
	public Player player;

	bool isGameStart;

	public void InitStage() {

		gimmickManager.Init();
		mouseCamera.Init();
		player.Init();

		GameMaster.gameMaster.OnGameStart += () => isGameStart = true;
		GameMaster.gameMaster.OnGameOver += () => isGameStart = false;
		GameMaster.gameMaster.OnGameClear += () => isGameStart = false;
	}

	// Update is called once per frame
	public void StageUpdate () {

		if(isGameStart) {
			player.Move();
			gimmickManager.GimmickUpdate();
		}

		mouseCamera.CameraUpdate();

	}
}
