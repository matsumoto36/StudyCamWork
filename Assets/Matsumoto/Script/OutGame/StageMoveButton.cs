using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ステージ選択ウィンドウに現れるステージのボタン
/// </summary>
public class StageMoveButton : MonoBehaviour {

	public string LoadPathName;
	public string LoadStudioName;
	public string Title;

	public int WindowIndex;
	public StageMoveButton NextStage;
	public Image StageImage;
	public Image FrameImage;

	private StageSelectController _owner;

	/// <summary>
	/// 最初に呼ばれる
	/// </summary>
	/// <param name="owner"></param>
	public void Init (StageSelectController owner) {
		_owner = owner;

		GetComponentInChildren<Button>().onClick.AddListener(() => {
			AudioManager.PlaySE("click03");
			_owner.ShowStageWindow(this);
		});

		GetComponentInChildren<Text>().text = Title;

		//達成状況によってフレームの色を変える
		var data = GameData.stageData[LoadPathName];

		//未クリア
		if(data.score == 0) {
			FrameImage.color = new Color(0, 0, 0, 0);
		}
		//とりあえずクリア
		else if(data.accuracy < 1.0f) {
			FrameImage.sprite = Resources.Load<Sprite>("Texture/ClearFrame");
		}
		//パーフェクト
		else {
			FrameImage.sprite = Resources.Load<Sprite>("Texture/ClearParfectFrame");
		}
	}
}
