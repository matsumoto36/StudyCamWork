using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerCaptureStatus {
	None,
	Focus,
	Contain,
	All,

}

public class Player : MonoBehaviour {

	public Renderer centerLight;
	public Renderer ring1;
	public Renderer ring2;

	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color CaptureLightColor;
	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color FailLightColor;

	PlayerCaptureStatus status = PlayerCaptureStatus.None;

	Bezier2D path;

	[SerializeField]
	float speed;
	public float Speed {
		get { return speed; }
		set { speed = value; }
	}

	[SerializeField]
	float scale;
	public float Scale {
		get { return scale; }
		set { scale = value; }
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

		this.path = path;
		Speed = GameMaster.Instance.GameBalanceData.PlayerSpeed;

		MovedLength = 0;
		transform.localScale = Vector3.one * Scale;

		centerLight.material = new Material(centerLight.material);
		centerLight.material.EnableKeyword("_EMISSION");
		ring1.material = new Material(ring1.material);
		ring1.material.EnableKeyword("_EMISSION");
		ring2.material = new Material(ring2.material);
		ring2.material.EnableKeyword("_EMISSION");

		var p = ParticleManager.Spawn("TestParticle", new Vector3(), Quaternion.identity, 0);
		p.transform.SetParent(transform);
		p.transform.localPosition = new Vector3();

		SetLight(PlayerCaptureStatus.All);
	}

	public void SetLight(PlayerCaptureStatus status) {

		if(this.status == status) return;
		this.status = status;

		if(status == PlayerCaptureStatus.All) 
			centerLight.material.SetColor("_EmissionColor", CaptureLightColor);
		else
			centerLight.material.SetColor("_EmissionColor", FailLightColor);

		var bit = (int)status;
		if((bit & (int)PlayerCaptureStatus.Contain) != 0) 
			ring2.material.SetColor("_EmissionColor", CaptureLightColor);
		else 
			ring2.material.SetColor("_EmissionColor", FailLightColor);

		if((bit & (int)PlayerCaptureStatus.Focus) != 0)
			ring1.material.SetColor("_EmissionColor", CaptureLightColor);
		else
			ring1.material.SetColor("_EmissionColor", FailLightColor);
	}

	void Update() {
		//デバッグ機能
		//リトライ
		if(Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);

		if(Input.GetKeyDown(KeyCode.P))
			AudioManager.PlaySE("decision1");
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
