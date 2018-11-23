using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面に表示されるウィンドウ
/// </summary>
public class ResultUI : MonoBehaviour {

	public Text ResultScoreText;
	public Text ResultAccText;
	public Text ResultComboText;

	public int ResultScore;
	public float ResultAccuracy;
	public int ResultCombo;

	private MouseCamera _mouseCamera;

	// Use this for initialization
	private void Start() {
		_mouseCamera = FindObjectOfType<MouseCamera>();
	}

	// Update is called once per frame
	private void Update() {
		ResultScore = _mouseCamera.Score;
		ResultCombo = _mouseCamera.ComboMax;
		ResultAccuracy = _mouseCamera.Accuracy;

		ResultScoreText.text = ResultScore.ToString();
		ResultAccText.text =
			ResultAccuracy >= 1.0f ? "100%" : ResultAccuracy.ToString("P");

		ResultComboText.text = "x" + ResultCombo.ToString("");
	}
}
