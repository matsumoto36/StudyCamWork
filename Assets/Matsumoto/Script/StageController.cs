using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ内の処理の順番を制御するクラス
/// </summary>
public class StageController : MonoBehaviour {

	public MouseCamera mouseCamera;
	Player player;
	GimmickManager gimmickManager;

	bool isGameStart;
	bool isGameEnd;

	public void InitStage(Player player, Bezier2D path, GimmickManager manager) {

		this.player = player;
		gimmickManager = manager;

		player.Init(path);
		gimmickManager.Init(path, player);
		mouseCamera.Init(player);

		GameMaster.Instance.OnGameStart += () => isGameStart = true;
		GameMaster.Instance.OnGameOver += () => {
			isGameStart = false;
			isGameEnd = true;
		};
		GameMaster.Instance.OnGameClear += () => {
			isGameStart = false;
			isGameEnd = true;
		};
	}

	// Update is called once per frame
	public void StageUpdate () {

		if(isGameEnd) return;

		if(isGameStart) {
			player.Move();
			gimmickManager.GimmickUpdate();
		}

		mouseCamera.CameraUpdate();
	}
}
