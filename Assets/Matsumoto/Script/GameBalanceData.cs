using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージごとのゲームバランスを定義
/// </summary>
[System.Serializable]
public class GameBalanceData {

	[SerializeField]
	float focusGrace;
	public float FocusGrace { get { return focusGrace; }}

	[SerializeField]
	float focusDuration;
	public float FocusDuration { get { return focusDuration; }}

	public GameBalanceData(float focusGrace, float focusDuration) {
		this.focusGrace = focusGrace;
		this.focusDuration = focusDuration;
	}

}
