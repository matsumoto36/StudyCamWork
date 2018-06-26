using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
    //リザルトの背景
    public Image ResultBack;
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

    public GameObject canvas;

    Text[] text = new Text[6];

    //リトライボタン
    public Text Retry;
    public Text ClearRetry;
    //ステージセレクトボタン
    public Text StageSelect;
    public Text ClearStageSelect;

    bool skip;

    // Use this for initialization
    void Start ()
    {
        text[0] = ScoreFont;
        text[1] = ScoreNumber;
        text[2] = ComboFont;
        text[3] = ComboNumber;
        text[4] = AccuracyFont;
        text[5] = AccuracyNumber;

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

    IEnumerator GameClearResult()
    {
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 0.8f);

        Color TextstartColor = new Color(1, 1, 1, 0);
        Color TextendColor = new Color(1, 1, 1, 1);

        ClearRetry.gameObject.SetActive(true);
        ClearStageSelect.gameObject.SetActive(true);

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

        while(time < 1.0)
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

            ClearRetry.color = Color.Lerp(TextstartColor, TextendColor, time);
            ClearStageSelect.color = Color.Lerp(TextstartColor, TextendColor, time);

            yield return null;
        }
        //yield return new WaitForSeconds(1);
    }

    IEnumerator GameOverResult()
    {
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 0.8f);

        Color TextstartColor = new Color(1, 1, 1, 0);
        Color TextendColor = new Color(1, 1, 1, 1);

        Retry.gameObject.SetActive(true);
        StageSelect.gameObject.SetActive(true);

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
            ResultBack.color = Color.Lerp(startColor, endColor, time);

            Retry.color = Color.Lerp(TextstartColor, TextendColor, time);
            StageSelect.color = Color.Lerp(TextstartColor, TextendColor, time);

            yield return null;
        }

        yield return new WaitForSeconds(1);
    }

	public void OnRetryClick() {

		AudioManager.PlaySE("Button3");

		FindObjectOfType<TimerController>()
			.SceneMove("GameScene");
	}

	public void OnStageSelectClick() {

		AudioManager.PlaySE("Button3");

		StageSelectController.movieSkip = true;

		FindObjectOfType<TimerController>()
			.SceneMove("TitleScene");
	}
}