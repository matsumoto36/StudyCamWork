using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;

public class DOFSlide : MonoBehaviour {

	public PostProcessVolume volume;
	public AnimationCurve FocusCurve;

	DepthOfField dof;

	bool buttonPrev = false;

	public float[] distanceList = new float[] {
		0.433f,
		0.455f,
		0.479f,
		0.506f,
		0.536f,
		0.570f,
		0.609f,
		0.653f,
		0.704f,
		0.765f,
		0.835f,
		0.915f,
		1.030f,
		1.160f,
		1.325f,
		1.550f,
		1.890f,
		2.380f,
		3.190f,
		4.950f,
	};

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
		Value += focusAngle * Time.deltaTime / GameMaster.Instance.GameBalanceData.FocusDuration;
		Value = Mathf.Clamp(Value, 0, 1);

		var position = Value * (distanceList.Length - 1);

		distance = distanceList[(int)position];

		if(position != distanceList.Length - 1)
			distance += (distanceList[(int)position + 1] - distanceList[(int)position]) * (position - (int)position);

		dof.focusDistance.value = distance;

		if(IsFocus != buttonPrev) {
			AudioManager.PlaySE("Focus2");
		}

		buttonPrev = IsFocus;
	}
}
