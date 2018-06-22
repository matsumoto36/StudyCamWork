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
    //スコア　左
    public Text ScoreFont;
    //スコア　右
    public Text ScoreNumber;
    //コンボ　左
    public Text ComboFont;
    //コンボ　右
    public Text ComboNumber;
    //正確さ　左
    public Text AccuracyFont;
    //正確さ　右
    public Text AccuracyNumber;

    public Canvas canvas;

    public Image oto;

    Text[] text = new Text[6];

    //リトライボタン
    public Text Retry;
    public Text ClearRetry;
    //ステージセレクトボタン
    public Text StageSelect;
    public Text ClearStageSelect;

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
        /*
        //ゲームクリア時
        if (Input.GetKeyDown(KeyCode.Z))//判定を変更しておくこと
        {
            StartCoroutine(GameClearResult());
        }
        //ゲームオーバー時
        else if (Input.GetKeyDown(KeyCode.X))//判定を変更しておくこと
        {
            StartCoroutine(GameOverResult());
        }*/
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

        canvas.enabled = false;

        while (time < 1.0f)
        {
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
                //ScoreFont.color = Color.Lerp(TextstartColor, TextendColor, time);
                //ScoreNumber.color = Color.Lerp(TextstartColor, TextendColor, time);
                //ComboFont.color = Color.Lerp(TextstartColor, TextendColor, time);
                //ComboNumber.color = Color.Lerp(TextstartColor, TextendColor, time);
                //AccuracyFont.color = Color.Lerp(TextstartColor, TextendColor, time);
                //AccuracyNumber.color = Color.Lerp(TextstartColor, TextendColor, time);

                //ClearRetry.color = Color.Lerp(TextstartColor, TextendColor, time);
                //ClearStageSelect.color = Color.Lerp(TextstartColor, TextendColor, time);

                time += Time.deltaTime;

                text[i].color = Color.Lerp(TextstartColor, TextendColor, time);

                yield return null;
            }
        }

        time = 0;

        while(time < 1.0)
        {
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
            time += Time.deltaTime * 3;
            ResultBack.color = Color.Lerp(startColor, endColor, time);

            Retry.color = Color.Lerp(TextstartColor, TextendColor, time);
            StageSelect.color = Color.Lerp(TextstartColor, TextendColor, time);

            yield return null;
        }

        yield return new WaitForSeconds(1);

        oto.color = new Color(1, 1, 1, 1);
    }
}