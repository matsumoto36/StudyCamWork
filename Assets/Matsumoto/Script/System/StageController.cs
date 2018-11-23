using System;
using UnityEngine;

/// <summary>
/// ステージ内の処理の順番を制御するクラス
/// </summary>
public class StageController : MonoBehaviour {

	public MouseCamera MouseCamera;

	private Player _player;
	private GimmickManager _gimmickManager;
	private float _pathLength;

	public void InitStage(Player player, Bezier2D path, GimmickManager manager) {

		_player = player;
		_gimmickManager = manager;
		_pathLength = path.Length;

		player.Init(path);
		_gimmickManager.Init(path, player);
		MouseCamera.Init(player);
	}

	// Update is called once per frame
	public void StageUpdate() {

		var state = GameMaster.Instance.State;

		switch(state) {
			case GameState.AfterEnd:
				return;
			case GameState.Playing:
				_player.Move();
				_gimmickManager.GimmickUpdate();
				break;
			case GameState.BeforeStart:
				break;
			case GameState.Starting:
				break;
			case GameState.Ending:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		MouseCamera.CameraUpdate();

		//クリアチェック
		if(state != GameState.Playing) return;
		if(_player.MovedLength / _pathLength >= 1.0f) {
			GameMaster.Instance.GameClear();
		}
	}
}
