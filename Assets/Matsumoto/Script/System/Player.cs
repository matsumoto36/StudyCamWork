using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerCaptureStatus {
	None,
	FocusOnly,
	ContainOnly,
	Near,
	All,

}

/// <summary>
/// ゲーム内でパスに従って動くキャラクター
/// </summary>
public class Player : MonoBehaviour {

	public const float ScaleMin = 0.8f;
	public const float ScaleMax = 1.2f;

	[FormerlySerializedAs("centerLight")] public Renderer CenterLight;
	[FormerlySerializedAs("ring1")] public Renderer Ring1;
	[FormerlySerializedAs("ring2")] public Renderer Ring2;

	public Color CaptureLightColor;
	public Color NearLightColor;
	public Color FailLightColor;

	[FormerlySerializedAs("intencity")] public float Intensity;

	private PlayerCaptureStatus _status = PlayerCaptureStatus.None;

	private Bezier2D _path;
	private PKFxFX _particle;
	private bool _isSceneMoved;

	public Transform Body {
		get; private set;
	}


	[FormerlySerializedAs("speed")]
	[SerializeField]
	private float _speed;
	public float Speed {
		get { return _speed; }
		set { _speed = value; }
	}

	private float _movedLength;
	public float MovedLength {
		get { return _movedLength; }
		set {
			//パスに沿って移動する
			_movedLength = Mathf.Clamp(value, 0, _path.Length);
			var t = _movedLength / _path.Length;
			transform.position = _path.GetPointNormalize(t);
		}
	}

	public void Init(Bezier2D path) {

		//見た目を取得
		Body = transform.GetChild(0);

		//最初は一番大きくする
		SetScaleFromRatio(1);

		_path = path;
		Speed = GameMaster.Instance.GameBalanceData.PlayerSpeed;

		MovedLength = 0;

		CenterLight.material = new Material(CenterLight.material);
		CenterLight.material.EnableKeyword("_EMISSION");
		Ring1.material = new Material(Ring1.material);
		Ring1.material.EnableKeyword("_EMISSION");
		Ring2.material = new Material(Ring2.material);
		Ring2.material.EnableKeyword("_EMISSION");

		//パーティクルのスポーン
		_particle = ParticleManager.Spawn("ActorMoveEffect", transform.position, Quaternion.identity, 0);
		_particle.transform.SetParent(transform);

		SetLight(PlayerCaptureStatus.All);

		StartCoroutine(MoveAudioUpdate());
	}

	public void SetLight(PlayerCaptureStatus status) {

		if(_status == status) return;
		_status = status;

		switch(status) {
			case PlayerCaptureStatus.None:
				_particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				CenterLight.material.SetColor("_EmissionColor", FailLightColor);
				Ring2.material.SetColor("_EmissionColor", FailLightColor);
				Ring1.material.SetColor("_EmissionColor", FailLightColor);
				break;
			case PlayerCaptureStatus.FocusOnly:
				_particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				CenterLight.material.SetColor("_EmissionColor", FailLightColor * Intensity);
				Ring2.material.SetColor("_EmissionColor", FailLightColor * Intensity);
				Ring1.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				break;
			case PlayerCaptureStatus.ContainOnly:
				_particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				CenterLight.material.SetColor("_EmissionColor", FailLightColor * Intensity);
				Ring2.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				Ring1.material.SetColor("_EmissionColor", FailLightColor * Intensity);
				break;
			case PlayerCaptureStatus.Near:
				_particle.GetAttribute("MainColor").ValueFloat4 = NearLightColor;
				CenterLight.material.SetColor("_EmissionColor", NearLightColor * Intensity);
				Ring2.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				Ring1.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				break;
			case PlayerCaptureStatus.All:
				_particle.GetAttribute("MainColor").ValueFloat4 = CaptureLightColor;
				CenterLight.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				Ring2.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				Ring1.material.SetColor("_EmissionColor", CaptureLightColor * Intensity);
				break;
			default:
				throw new ArgumentOutOfRangeException("status", status, null);
		}
	}

	public float GetScaleFromRatio(float t) {
		return Mathf.Lerp(ScaleMin, ScaleMax, t);
	}

	public void SetScaleFromRatio(float t) {
		var scale = new Vector3(1, 1, 0) * Mathf.Lerp(ScaleMin, ScaleMax, t);
		scale.z = 1;
		Body.localScale = scale;
	}

	private void Update() {

		//リトライ
		if(Input.GetKeyDown(KeyCode.R) && !_isSceneMoved) {
			_isSceneMoved = true;

			AudioManager.PlaySE("Button3");
			GameMaster.Instance.Retry();
		}

		//セレクト
		if(Input.GetKeyDown(KeyCode.T) && !_isSceneMoved) {
			_isSceneMoved = true;

			AudioManager.PlaySE("Button3");
			StageSelectController.MovieSkip = true;
			GameMaster.Instance.MoveSelectScene();
		}
	}

	/// <summary>
	/// 移動する
	/// </summary>
	public void Move() {
		MovedLength += Speed * Time.deltaTime;
	}

	/// <summary>
	/// 移動音の調整を行う
	/// </summary>
	/// <returns></returns>
	private IEnumerator MoveAudioUpdate() {

		var moveSound = AudioManager.PlaySERaw("PlayerMoveLoop");
		moveSound.transform.SetParent(transform);
		moveSound.loop = true;

		var volume = 0.0f;
		const float fadeSpeed = 1.0f;

		while(true) {

			//ゲームプレイ中に動いていたら再生
			var canPlay = Speed > 0.0f && GameMaster.Instance.State == GameState.Playing;
			var volumeVec = canPlay ? 1 : -1;

			volume = Mathf.Clamp(volume + volumeVec * fadeSpeed * Time.deltaTime, 0, 1);

			moveSound.volume = volume;
			moveSound.pitch = Speed / 10;

			yield return null;
		}

	}
}
