using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectWindowActive : MonoBehaviour {

	public RectTransform[] stageWindows;
	public Button[] stageSelectButtons;

	public Color activeColor;
	public Color inActiveColor;

	int activeWindowIndex = -1;

	Image[] buttonImages;

	// Use this for initialization
	void Start () {

		buttonImages = new Image[stageSelectButtons.Length];

		for(int i = 0;i < stageWindows.Length;i++) {

			var button = stageSelectButtons[i];
			var capture_i = i;

			button.onClick.AddListener(() => {
				AudioManager.PlaySE("Button3");
				SetActiveWindow(capture_i);
			});

			buttonImages[i] = button.GetComponent<Image>();
		}

		SetActiveWindow(0);
	}
	
	void SetActiveWindow(int windowIndex) {

		if(windowIndex == activeWindowIndex) return;
		activeWindowIndex = windowIndex;

		for(int i = 0;i < buttonImages.Length;i++) {

			Color col;
			if(windowIndex == i) {
				col = activeColor;
				stageWindows[i].SetAsLastSibling();
			}
			else {
				col = inActiveColor;
			}

			buttonImages[i].color = col;

		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
