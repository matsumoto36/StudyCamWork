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

	float pathLength;

	public void InitStage(Player player, Bezier2D path, GimmickManager manager) {

		this.player = player;
		gimmickManager = manager;
		pathLength = path.Length;

		player.Init(path);
		gimmickManager.Init(path, player);
		mouseCamera.Init(player);
	}

	// Update is called once per frame
	public void StageUpdate () {

		var state = GameMaster.Instance.State;

		if(state == GameState.AfterEnd) return;

		if(state == GameState.Playing) {
			player.Move();
			gimmickManager.GimmickUpdate();
		}

		mouseCamera.CameraUpdate();

		//クリアチェック
		if(state == GameState.Playing) {
			if(player.MovedLength / pathLength >= 1.0f) {
				GameMaster.Instance.GameClear();
			}
		}
	}
}
