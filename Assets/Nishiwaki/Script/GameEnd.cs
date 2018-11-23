using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour {
	//リザルトの背景、枠、文字
	public CanvasGroup ResultBackFrame;
	//リザルトのもろもろ
	public CanvasGroup ResultGroup;
	//スコア
	public Text Score;
	//コンボ
	public Text Combo;
	//正確さ
	public Text Accuracy;

	//ベストスコア
	public Text BestScore;
	//ベスト正確さ
	public Text BestAccuracy;
	//ベストコンボ
	public Text BestCombo;

	//リザルト用キャンバス
	public GameObject Canvas;

	private readonly Text[] _texts = new Text[3];

	//リトライボタン
	public CanvasGroup Retry;
	//ステージセレクトボタン
	public CanvasGroup Back;
	//ネクストステージボタン
	public CanvasGroup Next;

	//どれかのボタンが選択されたかどうか
	private bool _isButtonSelected;

	private readonly Color _textStartColor = new Color(1, 1, 1, 0);
	private readonly Color _textEndColor = new Color(1, 1, 1, 1);

	// Use this for initialization
	private void Start() {
		_texts[0] = Score;
		_texts[1] = Accuracy;
		_texts[2] = Combo;

		//グループ内のUI要素が入力を受け付けるかどうか
		Retry.interactable = false;
		Back.interactable = false;
		Next.interactable = false;

		ResultBackFrame.alpha = 0;
		ResultGroup.alpha = 0;
		Score.color = _textStartColor;
		Combo.color = _textStartColor;
		Accuracy.color = _textStartColor;

		Retry.alpha = 0;
		Back.alpha = 0;
		Next.alpha = 0;

		GameMaster.Instance.OnGameClear += () => {
			StartCoroutine(GameClearResult());
		};

		GameMaster.Instance.OnGameOver += () => {
			StartCoroutine(GameOverResult());
		};
	}

	//ゲームクリア時のコルーチン
	private IEnumerator GameClearResult() {
		if(GameMaster.IsTestPlayMode) {
			BestScore.text = "0";
			BestAccuracy.text = "0%";
			BestCombo.text = "x0";
		}
		else {
			var data = GameData.StageData[GameMaster.LoadPathName];

			BestScore.text = data.Score.ToString();
			BestAccuracy.text = data.Accuracy >= 1.0f ? "100%" : data.Accuracy.ToString("p");
			BestCombo.text = "x" + data.MaxCombo.ToString();
		}

		//リザルトのスキップ
		var skip = false;

		//α値を変更する時間
		float time = 0;

		yield return new WaitForSeconds(2);

		Canvas.SetActive(false);

		while(time < 1.0f) {
			if(Input.GetMouseButton(0)) {
				skip = true;
			}
			if(skip) {
				time = 1;
			}

			time += Time.deltaTime;
			ResultGroup.alpha = Mathf.Lerp(0, 1, time);
			ResultBackFrame.alpha = Mathf.Lerp(0, 1, time);

			yield return null;
		}

		foreach(var text in _texts) {
			time = 0;

			while(time < 1.0f) {
				if(Input.GetMouseButton(0)) {
					skip = true;
				}
				if(skip) {
					time = 1;
				}

				time += Time.deltaTime;

				text.color = Color.Lerp(_textStartColor, _textEndColor, time);

				yield return null;
			}
		}

		time = 0;

		if(GameMaster.IsTestPlayMode) {
			Next.gameObject.SetActive(false);
			Back.gameObject.SetActive(false);
		}

		//グループ内のUI要素が入力を受け付けるかどうか
		Retry.interactable = true;
		Back.interactable = true;

		if(GameMaster.Instance.CanMoveNextStage())
			Next.interactable = true;

		while(time < 1.0) {
			if(Input.GetMouseButton(0)) {
				skip = true;
			}
			if(skip) {
				time = 1;
			}

			time += Time.deltaTime;

			Retry.alpha = Mathf.Lerp(0, 1, time);
			Back.alpha = Mathf.Lerp(0, 1, time);

			if(GameMaster.Instance.CanMoveNextStage())
				Next.alpha = Mathf.Lerp(0, 1, time);

			yield return null;
		}
	}

	//ゲームオーバー時のコルーチン
	private IEnumerator GameOverResult() {

		if(GameMaster.IsTestPlayMode)
			Back.gameObject.SetActive(false);

		//グループ内のUI要素が入力を受け付けるかどうか
		Retry.interactable = true;
		Back.interactable = true;

		var skip = false;

		//α値を変更する時間
		float time = 0;

		yield return new WaitForSeconds(2);

		while(time < 1.0f) {
			if(Input.GetMouseButton(0)) {
				skip = true;
			}
			if(skip) {
				time = 1;
			}

			time += Time.deltaTime * 3;
			Retry.alpha = Mathf.Lerp(0, 1, time);
			Back.alpha = Mathf.Lerp(0, 1, time);

			yield return null;
		}

		yield return new WaitForSeconds(1);
	}

	public void OnRetryClick() {

		if(_isButtonSelected) return;
		_isButtonSelected = true;

		AudioManager.PlaySE("click03");
		Retry.GetComponentInChildren<Button>().interactable = false;

		GameMaster.Instance.Retry();
	}

	public void OnStageSelectClick() {

		if(_isButtonSelected) return;
		_isButtonSelected = true;

		AudioManager.PlaySE("click03");
		Back.GetComponentInChildren<Button>().interactable = false;

		GameMaster.Instance.MoveSelectScene();
	}

	public void OnNextStageClick() {

		if(_isButtonSelected) return;
		_isButtonSelected = true;

		AudioManager.PlaySE("click03");
		Next.GetComponentInChildren<Button>().interactable = false;

		GameMaster.Instance.NextScene();
	}
}