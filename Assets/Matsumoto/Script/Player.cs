using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCaptureStatus {
	None,
	Focus,
	Contain,
	All,

}

public class Player : MonoBehaviour {

	public float speed;

	public Renderer centerLight;
	public Renderer ring1;
	public Renderer ring2;

	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color CaptureLightColor;
	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color FailLightColor;

	PlayerCaptureStatus status = PlayerCaptureStatus.None;

	Bezier2D path;

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
		speed = GameMaster.Instance.GameBalanceData.PlayerSpeed;

		MovedLength = 0;

		centerLight.material = new Material(centerLight.material);
		centerLight.material.EnableKeyword("_EMISSION");
		ring1.material = new Material(ring1.material);
		ring1.material.EnableKeyword("_EMISSION");
		ring2.material = new Material(ring2.material);
		ring2.material.EnableKeyword("_EMISSION");

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

	// Update is called once per frame
	public void Move () {

		MovedLength += speed * Time.deltaTime;
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
