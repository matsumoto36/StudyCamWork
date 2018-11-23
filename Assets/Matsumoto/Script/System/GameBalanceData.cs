using UnityEngine;

/// <summary>
/// ステージごとのゲームバランスを定義
/// </summary>
public class GameBalanceData : MonoBehaviour {

	//プレイヤーのスピード
	[SerializeField]
	private float _playerSpeed = 3f;
	public float PlayerSpeed { get { return _playerSpeed; } }

	//失敗したときのダメージ
	[SerializeField]
	private int _damage = 10;
	public int Damage { get { return _damage; } }

	//何秒に一回収まっているか調べるか
	[SerializeField]
	private float _checkWait = 1;
	public float CheckWait { get { return _checkWait; } }

	//フォーカスの猶予時間
	[SerializeField]
	private float _focusGrace = 0.1f;
	public float FocusGrace { get { return _focusGrace; } }

	//フォーカスにかかる時間
	[SerializeField]
	private float _focusDuration = 1.0f;
	public float FocusDuration { get { return _focusDuration; } }

	//画面サイズを基にしたカメラのワイドサイズの割合
	[SerializeField]
	private float _cameraWideSizeRatio = 0.2f;
	public float CameraWideSizeRatio { get { return _cameraWideSizeRatio; } }

	//カメラのワイドサイズを基にしたカメラの中のサイズの割合
	[SerializeField]
	private float _cameraSmallSizeRatio = 0.5f;
	public float CameraSmallSizeRatio { get { return _cameraSmallSizeRatio; } }

	//カメラの中に入れると加算されるスコアの量
	[SerializeField]
	private int _baseScore = 100;
	public int BaseScore { get { return _baseScore; } }

	//カメラの中心に入れると加算されるスコアの量
	[SerializeField]
	private int _cameraInsideScore = 200;
	public int CameraInsideScore { get { return _cameraInsideScore; } }
}
