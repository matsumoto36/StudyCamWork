using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ゲームを終了するときに表示されるウィンドウ
/// </summary>
public class QuitGameWindow : MonoBehaviour {

	public Button QuitButton;
	public Button TitleButton;
	public GameObject Body;

	private void Start() {

		QuitButton.onClick.AddListener(() => {
			AudioManager.PlaySE("click03");
			Application.Quit();
		});

		TitleButton.onClick.AddListener(() => {
			AudioManager.PlaySE("click03");
			StageSelectController.MovieSkip = false;
			FindObjectOfType<TimerController>().SceneMove("TitleScene");

		});

	}

	public void IsActive(bool enable) {
		Body.SetActive(enable);
	}

}
