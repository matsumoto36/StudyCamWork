using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ステージ選択からステージを選んだ時に表示される
/// ステージの詳細のウィンドウ
/// </summary>
public class StageWindow : MonoBehaviour {

	public GameObject Body;

	public Image FrameImage;
	public Image StageImage;

	public Text Score;
	public Text Accuracy;
	public Text MaxCombo;

	public Text Title;

	public string LoadPathName;
	public string LoadStudioName;

	public Button StartButton;

	private StageMoveButton _button;
	private bool _isSceneMove;

	private void Start() {
		StartButton.onClick
			.AddListener(MoveStage);
	}

	/// <summary>
	/// 各ステージに挑戦するとき
	/// </summary>
	private void MoveStage() {

		if(_isSceneMove) return;
		_isSceneMove = true;

		//ボタンをロック
		StartButton.interactable = false;

		SelectWindowActive.ActiveWindowIndex = _button.WindowIndex;

		//先のステージをすべて登録
		var nextStageList = new List<StageInfo>();
		while(_button.NextStage) {
			_button = _button.NextStage;

			nextStageList.Add(
				new StageInfo(
					_button.LoadPathName,_button.LoadStudioName, _button.WindowIndex
					));
		}
		nextStageList.Add(null);

		GameMaster.SetNextStage(nextStageList);
		GameMaster.LoadPathName = LoadPathName;
		GameMaster.LoadStudioName = LoadStudioName;

		AudioManager.PlaySE("Button3");
		FindObjectOfType<TimerController>().SceneMove("GameScene");
	}

	/// <summary>
	/// ウィンドウを表示する
	/// </summary>
	/// <param name="button"></param>
	public void Show(StageMoveButton button) {

		_button = button;

		FrameImage.sprite = button.FrameImage.sprite;
		FrameImage.color = button.FrameImage.color;
		StageImage.sprite = button.StageImage.sprite;
		LoadPathName = button.LoadPathName;
		LoadStudioName = button.LoadStudioName;
		Title.text = button.Title;

		var data = GameData.stageData[LoadPathName];
		Score.text = data.score.ToString();

		Accuracy.text = 
			data.accuracy <= 1.0f ? "100" : (data.accuracy * 100).ToString("00.00");

		MaxCombo.text = data.maxCombo.ToString();

		Body.SetActive(true);
	}

	/// <summary>
	/// ウィンドウを隠す
	/// </summary>
	public void Hide() {
		Body.SetActive(false);
	}
}
