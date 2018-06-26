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

	public const float SCALE_MIN = 1.0f;
	public const float SCALE_MAX = 1.5f;

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
	float startTime;
	float sumSpeed;

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

		GameMaster.Instance.OnGameStart += () => startTime = Time.time;
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
		//デバッグ機能
		//リトライ
		if(Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);

	}

	// Update is called once per frame
	public void Move () {

		MovedLength += Speed * Time.deltaTime;
	}

	void ApplyMove() {

		movedLength = Mathf.Clamp(movedLength, 0, path.Length);
		var t = movedLength / path.Length;
		transform.position = path.GetPointNormalize(t);

		if(t >= 1.0f) {
			GameMaster.Instance.GameClear();
		}
	}
}
