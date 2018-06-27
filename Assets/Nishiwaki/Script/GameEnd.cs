using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
    //リザルトの背景
    public Image ResultBack;
    //リザルトの背景の枠
    public Image ResultBackFrame;
    //リザルトのタイトル
    public Text Result;
    //スコア　文字
    public Text ScoreFont;
    //スコア　数字
    public Text ScoreNumber;
    //コンボ　文字
    public Text ComboFont;
    //コンボ　数字
    public Text ComboNumber;
    //正確さ　文字
    public Text AccuracyFont;
    //正確さ　数字
    public Text AccuracyNumber;

    //リザルト用キャンバス
    public GameObject canvas;

    Text[] text = new Text[6];

    //リトライボタン
    public CanvasGroup Retry;
    //ステージセレクトボタン
    public CanvasGroup Back;
    //ネクストステージボタン
    public CanvasGroup Next;
    //リザルトのスキップ
    bool skip;
	//シーン移動中か？
	bool isSceneMoving = false;

    Color startColor = new Color(0, 0, 0, 0);
    Color endColor = new Color(0, 0, 0, 0.8f);

    Color TextstartColor = new Color(1, 1, 1, 0);
    Color TextendColor = new Color(1, 1, 1, 1);

    // Use this for initialization
    void Start ()
    {
        text[0] = ScoreFont;
        text[1] = ScoreNumber;
        text[2] = AccuracyFont;
        text[3] = AccuracyNumber;
        text[4] = ComboFont;
        text[5] = ComboNumber;

        //グループ内のUI要素が入力を受け付けるかどうか
        Retry.interactable = false;
        Back.interactable = false;
        Next.interactable = false;

        ResultBack.color = startColor;
        ResultBackFrame.color = TextstartColor;
        Result.color = TextstartColor;
        ScoreFont.color = TextstartColor;
        ScoreNumber.color = TextstartColor;
        ComboFont.color = TextstartColor;
        ComboNumber.color = TextstartColor;
        AccuracyFont.color = TextstartColor;
        AccuracyNumber.color = TextstartColor;

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
        //Color startColor = new Color(0, 0, 0, 0);
        //Color endColor = new Color(0, 0, 0, 0.8f);

        //Color TextstartColor = new Color(1, 1, 1, 0);
        //Color TextendColor = new Color(1, 1, 1, 1);

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
            ResultBack.color = Color.Lerp(startColor, endColor, time);
            Result.color = Color.Lerp(TextstartColor, TextendColor, time);
			ResultBackFrame.color = Color.Lerp(startColor, endColor, time);


			yield return null;
        }

        for (int i = 0; i < 6; i++)
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

		if(isSceneMoving) return;
		isSceneMoving = true;

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}

	public void OnStageSelectClick() {

		AudioManager.PlaySE("click03");

		if(isSceneMoving) return;
		isSceneMoving = true;

		StageSelectController.movieSkip = true;

		FindObjectOfType<TimerController>()
			.SceneMove("TitleScene");
	}

	public void OnNextStageClick() {

		AudioManager.PlaySE("click03");

		if(isSceneMoving) return;
		isSceneMoving = true;

		GameMaster.Instance.NextScene();
	}
}