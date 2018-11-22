using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// 被写界深度エフェクトの焦点を操作する
/// </summary>
public class DOFSlide : MonoBehaviour {

	private readonly float[] _distanceList = {
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

	public PostProcessVolume Volume;

	private DepthOfField _depthOfField;
	private bool _isFocusPrev;
	private float _distance;

	public float Value {
		get; set;
	}

	public bool IsFocus {
		get; private set;
	}

	// Use this for initialization
	private void Start() {
		_depthOfField = Volume.profile.GetSetting<DepthOfField>();
	}

	// Update is called once per frame
	private void Update() {

		if(GameMaster.Instance.State != GameState.Playing) return;

		IsFocus = Input.GetMouseButton(0);

		var focusAngle = IsFocus ? 1 : -1;
		Value += focusAngle * Time.deltaTime / GameMaster.Instance.GameBalanceData.FocusDuration;
		Value = Mathf.Clamp(Value, 0, 1);

		//配列上の位置を割合で取得
		var position = Value * (_distanceList.Length - 1);
		var positionFloor = (int)position;

		//配列の中間の値は線形補完で求める
		_distance = 
			positionFloor + 1 < _distanceList.Length
			? Mathf.Lerp(_distanceList[positionFloor], _distanceList[positionFloor + 1], position - positionFloor)
			: _distanceList[positionFloor];

		_depthOfField.focusDistance.value = _distance;

		if(IsFocus != _isFocusPrev) {
			AudioManager.PlaySE("Focus2");
		}

		_isFocusPrev = IsFocus;
	}
}
