using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerCaptureStatus {
	None,
	FocusOnly,
	ContainOnly,
	Near,
	All,

}

public class Player : MonoBehaviour {

	public const float SCALE_MIN = 0.8f;
	public const float SCALE_MAX = 1.2f;

	public Renderer centerLight;
	public Renderer ring1;
	public Renderer ring2;

	public Color CaptureLightColor;
	public Color NearLightColor;
	public Color FailLightColor;

	public float intencity;

	PlayerCaptureStatus status = PlayerCaptureStatus.None;

	Bezier2D path;
	PKFxFX particle;
	float sumSpeed;
	bool isSceneMoved;

	public Transform Body {
		get; private set;
	}


	[SerializeField]
	float speed;
	public float Speed {
		get { return speed; }
		set { speed = value; }
	}

	float movedLength;
	public float MovedLength {
		get { return movedLength; }
		set {
			movedLength = value;
			ApplyMove();
		}
	}

	public void Init(Bezier2D path) {

		//見た目を取得
		Body = transform.GetChild(0);

		//最初は一番大きくする
		SetScaleFromRatio(1);

		this.path = path;
		Speed = GameMaster.Instance.GameBalanceData.PlayerSpeed;

		MovedLength = 0;

		centerLight.material = new Material(centerLight.material);
		centerLight.material.EnableKeyword("_EMISSION");
		ring1.material = new Material(ring1.material);
		ring1.material.EnableKeyword("_EMISSION");
		ring2.material = new Material(ring2.material);
		ring2.material.EnableKeyword("_EMISSION");

		//パーティクルのスポーン
		particle = ParticleManager.Spawn("ActorMoveEffect", transform.position, Quaternion.identity, 0);
		particle.transform.SetParent(transform);

		SetLight(PlayerCaptureStatus.All);

		StartCoroutine(MoveAudioUpdate());
	}

	public void SetLight(PlayerCaptureStatus status) {

		if(this.status == status) return;
		this.status = status;

		switch(status) {
			case PlayerCaptureStatus.None:
				particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				centerLight.material.SetColor("_EmissionColor", FailLightColor);
				ring2.material.SetColor("_EmissionColor", FailLightColor);
				ring1.material.SetColor("_EmissionColor", FailLightColor);
				break;
			case PlayerCaptureStatus.FocusOnly:
				particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				centerLight.material.SetColor("_EmissionColor", FailLightColor * intencity);
				ring2.material.SetColor("_EmissionColor", FailLightColor * intencity);
				ring1.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				break;
			case PlayerCaptureStatus.ContainOnly:
				particle.GetAttribute("MainColor").ValueFloat4 = FailLightColor;
				centerLight.material.SetColor("_EmissionColor", FailLightColor * intencity);
				ring2.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				ring1.material.SetColor("_EmissionColor", FailLightColor * intencity);
				break;
			case PlayerCaptureStatus.Near:
				particle.GetAttribute("MainColor").ValueFloat4 = NearLightColor;
				centerLight.material.SetColor("_EmissionColor", NearLightColor * intencity);
				ring2.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				ring1.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				break;
			case PlayerCaptureStatus.All:
				particle.GetAttribute("MainColor").ValueFloat4 = CaptureLightColor;
				centerLight.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				ring2.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				ring1.material.SetColor("_EmissionColor", CaptureLightColor * intencity);
				break;
			default:
				break;
		}
	}

	public float GetScaleFromRatio(float t) {
		return Mathf.Lerp(SCALE_MIN, SCALE_MAX, t);
	}

	public void SetScaleFromRatio(float t) {
		var scale = new Vector3(1, 1, 0) * Mathf.Lerp(SCALE_MIN, SCALE_MAX, t);
		scale.z = 1;
		Body.localScale = scale;
	}

	void Update() {

		//リトライ
		if(Input.GetKeyDown(KeyCode.R) && !isSceneMoved) {
			isSceneMoved = true;

			AudioManager.PlaySE("Button3");
			GameMaster.Instance.Retry();
		}

		//セレクト
		if(Input.GetKeyDown(KeyCode.T) && !isSceneMoved) {
			isSceneMoved = true;

			AudioManager.PlaySE("Button3");
			StageSelectController.MovieSkip = true;
			GameMaster.Instance.MoveSelectScene();
		}
	}

	// Update is called once per frame
	public void Move () {

		MovedLength += Speed * Time.deltaTime;
	}

	void ApplyMove() {

		movedLength = Mathf.Clamp(movedLength, 0, path.Length);
		var t = movedLength / path.Length;
		transform.position = path.GetPointNormalize(t);
	}

	IEnumerator MoveAudioUpdate() {

		var moveSound = AudioManager.PlaySERaw("PlayerMoveLoop");
		moveSound.transform.SetParent(transform);
		moveSound.loop = true;

		var volume = 0.0f;
		var fadeSpeed = 1.0f;

		while(true) {

			var canPlay = false;
			if(Speed != 0) canPlay = true;
			if(GameMaster.Instance.State == GameState.Playing) canPlay = true;
			else canPlay = false;

			var volumeVec = canPlay ? 1 : -1;
			volume = Mathf.Clamp(volume + volumeVec * fadeSpeed * Time.deltaTime, 0, 1);

			moveSound.volume = volume;
			moveSound.pitch = Speed / 10;

			yield return null;
		}

	}
}
