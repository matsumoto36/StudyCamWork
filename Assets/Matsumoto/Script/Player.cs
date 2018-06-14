using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCaptureState {
	All,
	Focus,
	Contain,
	None,
}

public class Player : MonoBehaviour {
	
	public Bezier2D path;
	public float speed;

	public Renderer centerLight;
	public Renderer ring1;
	public Renderer ring2;

	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color CaptureLightColor;
	[ColorUsage(false, true, 0, 10, 0, 10)]
	public Color FailLightColor;

	public PlayerCaptureState state = PlayerCaptureState.All;
	PlayerCaptureState _state = PlayerCaptureState.All;

	float movedLength;
	public float MovedLength {
		get { return movedLength; }
		set {
			movedLength = value;
			ApplyMove();
		}
	}

	public void Init() {
		MovedLength = 0;

		centerLight.material = new Material(centerLight.material);
		centerLight.material.EnableKeyword("_EMISSION");
		ring1.material = new Material(ring1.material);
		ring1.material.EnableKeyword("_EMISSION");
		ring2.material = new Material(ring2.material);
		ring2.material.EnableKeyword("_EMISSION");

		SetLight(state);
	}

	public void SetLight(PlayerCaptureState state) {
		switch(state) {
			case PlayerCaptureState.All:
				centerLight.material.SetColor("_EmissionColor", CaptureLightColor);
				ring1.material.SetColor("_EmissionColor", CaptureLightColor);
				ring2.material.SetColor("_EmissionColor", CaptureLightColor);
				break;
			case PlayerCaptureState.Focus:
				centerLight.material.SetColor("_EmissionColor", FailLightColor);
				ring1.material.SetColor("_EmissionColor", CaptureLightColor);
				ring2.material.SetColor("_EmissionColor", FailLightColor);
				break;
			case PlayerCaptureState.Contain:
				centerLight.material.SetColor("_EmissionColor", FailLightColor);
				ring1.material.SetColor("_EmissionColor", FailLightColor);
				ring2.material.SetColor("_EmissionColor", CaptureLightColor);
				break;
			case PlayerCaptureState.None:
				centerLight.material.SetColor("_EmissionColor", FailLightColor);
				ring1.material.SetColor("_EmissionColor", FailLightColor);
				ring2.material.SetColor("_EmissionColor", FailLightColor);
				break;
			default:
				break;
		}
	}
	
	void Update() {

		if(state != _state) {
			_state = state;
			SetLight(state);
		}
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
			GameMaster.gameMaster.GameClear();
		}
	}
}
