using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;

public class TestDOFSlide : MonoBehaviour {

	public PostProcessVolume volume;

	public float duration;

	public AnimationCurve FocusCurve;

	DepthOfField dof;

	float minDistance = 0.45f;
	float distance = 0.3f;
	float maxDistance = 3.4f;

	float t = 0.0f;

	// Use this for initialization
	void Start () {
		dof = volume.profile.GetSetting<DepthOfField>();
	}
	
	// Update is called once per frame
	void Update () {

		var focusAngle = Input.GetMouseButton(0) ? 1 : -1;
		t += focusAngle * Time.deltaTime / duration;
		t = Mathf.Clamp(t, 0, 1);

		//distance += Mathf.Pow(1, focusAngle * speed * Time.deltaTime);
		distance = Mathf.Lerp(minDistance, maxDistance, FocusCurve.Evaluate(t));

		dof.focusDistance.value = distance;
	}
}
