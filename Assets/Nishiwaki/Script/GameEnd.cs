using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
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
    public GameObject canvas;

    Text[] text = new Text[3];

    //リトライボタン
    public CanvasGroup Retry;
    //ステージセレクトボタン
    public CanvasGroup Back;
    //ネクストステージボタン
    public CanvasGroup Next;
    //リザルトのスキップ
    bool skip;

    Color startColor = new Color(0, 0, 0, 0);
    Color endColor = new Color(0, 0, 0, 0.8f);

    Color TextstartColor = new Color(1, 1, 1, 0);
    Color TextendColor = new Color(1, 1, 1, 1);

    // Use this for initialization
    void Start ()
    {
        text[0] = Score;
        text[1] = Accuracy;
        text[2] = Combo;

        //グループ内のUI要素が入力を受け付けるかどうか
        Retry.interactable = false;
        Back.interactable = false;
        Next.interactable = false;

        ResultBackFrame.alpha = 0;
        ResultGroup.alpha = 0;
        Score.color = TextstartColor;
        Combo.color = TextstartColor;
        Accuracy.color = TextstartColor;

        Retry.alpha = 0;
        Back.alpha = 0;
        Next.alpha = 0;

        GameMaster.Instance.OnGameClear += () =>
        {
            StartCoroutine(GameClearResult());
        };

        GameMaster.Instance.OnGameOver += () =>
        {
            StartCoroutine(GameOverResult());
        };
    }
	
	// Update is called once per frame
	void Update () {

    }

    //ゲームクリア時のコルーチン
    IEnumerator GameClearResult()
    {
        StageData Data = GameData.stageData[GameMaster.LoadPathName];

        BestScore.text = Data.score.ToString();
		if(Data.accuracy >= 1.0f) BestAccuracy.text = "100%";
		else BestAccuracy.text = Data.accuracy.ToString("p");
        BestCombo.text = "x" + Data.maxCombo.ToString();

        //α値を変更する時間
        float time = 0;

        yield return new WaitForSeconds(2);

        canvas.SetActive(false);

        while (time < 1.0f)
        {
            if (Input.GetMouseButton(0))
            {
                skip = true;
            }
            if (skip)
            {
                time = 1;
            }

            time += Time.deltaTime;
            ResultGroup.alpha = Mathf.Lerp(0, 1, time);
            ResultBackFrame.alpha = Mathf.Lerp(0, 1, time);

			yield return null;
        }

        for (int i = 0; i < text.Length; i++)
        {
            time = 0;

            while (time < 1.0f)
            {
                if (Input.GetMouseButton(0))
                {
                    skip = true;
                }
                if (skip)
                {
                    time = 1;
                }

                time += Time.deltaTime;

                text[i].color = Color.Lerp(TextstartColor, TextendColor, time);
                
                yield return null;
            }
        }

        time = 0;

        //Retry.gameObject.SetActive(true);
        //StageSelect.gameObject.SetActive(true);

        //グループ内のUI要素が入力を受け付けるかどうか
        Retry.interactable = true;
        Back.interactable = true;

		if(GameMaster.Instance.CanMoveNextStage())
			Next.interactable = true;

		while (time < 1.0)
        {
            if (Input.GetMouseButton(0))
            {
                skip = true;
            }
            if (skip)
            {
                time = 1;
            }

            time += Time.deltaTime;

            Retry.alpha = Mathf.Lerp(0, 1, time);
            Back.alpha = Mathf.Lerp(0, 1, time);

			if(GameMaster.Instance.CanMoveNextStage())
				Next.alpha = Mathf.Lerp(0, 1, time);
            
            yield return null;
        }
        //yield return new WaitForSeconds(1);
    }

    //ゲームオーバー時のコルーチン
    IEnumerator GameOverResult()
    {
        //Retry.gameObject.SetActive(true);
        //Back.gameObject.SetActive(true);

        //グループ内のUI要素が入力を受け付けるかどうか
        Retry.interactable = true;
        Back.interactable = true;

        //α値を変更する時間
        float time = 0;

        yield return new WaitForSeconds(2);

        while (time < 1.0f)
        {
            if (Input.GetMouseButton(0))
            {
                skip = true;
            }
            if (skip)
            {
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

		AudioManager.PlaySE("click03");

		GameMaster.Instance.Retry();
	}

	public void OnStageSelectClick() {

		AudioManager.PlaySE("click03");

		GameMaster.Instance.MoveSelectScene();
	}

	public void OnNextStageClick() {

		AudioManager.PlaySE("click03");

		GameMaster.Instance.NextScene();
	}
}