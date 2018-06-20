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
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnClick() {
		owner.ShowStageWindow(this);
	}
}
