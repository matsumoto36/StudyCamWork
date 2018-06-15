using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージごとのゲームバランスを定義
/// </summary>
public class GameBalanceData : MonoBehaviour{

	[SerializeField]
	float playerSpeed = 3f;					//プレイヤーのスピード
	public float PlayerSpeed { get { return playerSpeed; } }

	[SerializeField]
	float focusGrace = 0.1f;				//フォーカスの猶予時間
	public float FocusGrace { get { return focusGrace; }}

	[SerializeField]
	float focusDuration = 1.0f;				//フォーカスにかかる時間
	public float FocusDuration { get { return focusDuration; }}

	[SerializeField]
	float cameraWideSizeRatio = 0.2f;		//画面サイズを基にしたカメラのワイドサイズの割合
	public float CameraWideSizeRatio { get { return cameraWideSizeRatio; } }

	[SerializeField]
	float cameraSmallSizeRatio = 0.5f;		//カメラのワイドサイズ基にしたカメラの中のサイズの割合
	public float CameraSmallSizeRatio { get { return cameraSmallSizeRatio; } }

	[SerializeField]
	int baseScore = 100;					//カメラの中に入れると加算されるスコアの量
	public int BaseScore { get { return baseScore; } }

	[SerializeField]
	int cameraInsideScore = 200;			//カメラの中心に入れると加算されるスコアの量
	public int CameraInsideScore { get { return cameraInsideScore; } }
}
