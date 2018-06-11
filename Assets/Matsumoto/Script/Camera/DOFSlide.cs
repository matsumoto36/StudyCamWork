using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;

public class DOFSlide : MonoBehaviour {

	public PostProcessVolume volume;
	public AnimationCurve FocusCurve;

	DepthOfField dof;

	public float minDistance = 0.45f;
	public float maxDistance = 3.4f;
	float distance;

	public float Value {
		get; set;
	}

	public bool IsFocus {
		get; private set;
	}

	// Use this for initialization
	void Start () {
		dof = volume.profile.GetSetting<DepthOfField>();
	}
	
	// Update is called once per frame
	void Update () {

		IsFocus = Input.GetMouseButton(0);

		var focusAngle = IsFocus ? 1 : -1;
		Value += focusAngle * Time.deltaTime / GameMaster.gameMaster.gameBalanceData.FocusDuration;
		Value = Mathf.Clamp(Value, 0, 1);

		distance = Mathf.Lerp(minDistance, maxDistance, FocusCurve.Evaluate(Value));

		dof.focusDistance.value = distance;
	}
}
