using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 各ステージのウィンドウを切り替える
/// </summary>
public class SelectWindowActive : MonoBehaviour {

	public static int ActiveWindowIndex;

	public RectTransform[] StageWindows;
	public Button[] StageSelectButtons;

	public Color ActiveColor;
	public Color InActiveColor;

	private Image[] _buttonImages;

	private void Start() {

		_buttonImages = new Image[StageSelectButtons.Length];

		for(var i = 0;i < StageWindows.Length;i++) {

			var button = StageSelectButtons[i];
			var captureI = i;

			button.onClick.AddListener(() => {
				AudioManager.PlaySE("click03");
				SetActiveWindow(captureI);
			});

			_buttonImages[i] = button.GetComponent<Image>();
		}

		SetActiveWindow(ActiveWindowIndex);
	}

	/// <summary>
	/// ウィンドウタブを切り替える
	/// </summary>
	/// <param name="windowIndex"></param>
	private void SetActiveWindow(int windowIndex) {

		ActiveWindowIndex = windowIndex;

		for(var i = 0;i < _buttonImages.Length;i++) {

			Color col;
			if(windowIndex == i) {
				col = ActiveColor;
				StageWindows[i].SetAsLastSibling();
			}
			else {
				col = InActiveColor;
			}

			_buttonImages[i].color = col;

		}
	}
}
