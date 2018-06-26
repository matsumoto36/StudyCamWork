using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class StageMoveButton : MonoBehaviour {

	public string loadPathName;
	public string loadStudioName;
	public string title;

	public StageMoveButton nextStage;
	public Image stageImage;
	public Image frameImage;

	StageSelectController owner;

	// Use this for initialization
	public void Init (StageSelectController owner) {
		this.owner = owner;

		GetComponentInChildren<Button>().onClick.AddListener(OnClick);
		GetComponentInChildren<Text>().text = title;

		//達成状況によってフレームの色を変える
		var data = GameData.stageData[loadPathName];

		//未クリア
		if(data.score == 0) {
			frameImage.color = new Color(0, 0, 0, 0);
		}
		//とりあえずクリア
		else if(data.accuracy < 1.0f) {
			frameImage.sprite = Resources.Load<Sprite>("Texture/ClearFrame");
		}
		//パーフェクト
		else {
			frameImage.sprite = Resources.Load<Sprite>("Texture/ClearParfectFrame");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnClick() {
		AudioManager.PlaySE("Button3");
		owner.ShowStageWindow(this);
	}
}
