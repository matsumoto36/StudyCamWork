using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームスタートスクリプト
/// </summary>
public class Count : MonoBehaviour
{
    //カウントオプション
    public bool CountOption = true;

    //表示する数字まで
    [SerializeField]
    Image[] Images = new Image[4];

    //0～9までの数字
    [SerializeField]
    Sprite[] NumberSprites = new Sprite[10];

    //時間をカウント
    public float TimeCount;

    //スタート表示文字
    public GameObject Start_Text;

    //スタート表示の間隔
    public float Text_interval=3f;

    //スタート時表示
    [SerializeField]
    Image[] st_img = new Image[4];

    //数字
    public GameObject ST_Number;

    //クリア表示
    public GameObject Clea_Object;

    //オーバー表示
    public GameObject Over_Object;

    //表示クリア（仮）
    public bool Judgment_CL = false;

    //表示オーバー（仮）
    public bool Judgment_OV = false;


    /// <summary>
    /// 時間を画像で表示
    /// </summary>
    void SetNumbers(int sec, int val1, int val2)
    {
        //時間形式に直し
        string str = string.Format("{00:00}", sec);
        //秒
        Images[val1].sprite = NumberSprites[Convert.ToInt32(str.Substring(0, 1))];
        //分
        Images[val2].sprite = NumberSprites[Convert.ToInt32(str.Substring(1, 1))];
    }

    /// <summary>
    /// スタート時に表示
    /// </summary>
    void St_Numbers(int Do_sec,int st1,int st2)
    {
        //時間形式に直す
        string st_r = string.Format("{00:00}", Do_sec);
        //秒
        st_img[st1].sprite = NumberSprites[Convert.ToInt32(st_r.Substring(0, 1))];
        //分
        st_img[st2].sprite = NumberSprites[Convert.ToInt32(st_r.Substring(1, 1))];
    }

    /// <summary>
    /// コルーチン分秒
    /// </summary>
    IEnumerator TimerStart()
    {
        //TimeCountが0以上なら
        while (0 <= TimeCount)
        {
            //CountOptionがtrueなら
            if (CountOption == true)
            {

                //表示時間
                Text_interval -= 1.0f;
                //表示時間を過ぎたら
                if (0 >= Text_interval)
                {
                    //表示時間を0に戻す
                    Text_interval = 0;

                    //Start_Textを非表示
                    Start_Text.SetActive(false);
                }

                //秒
                int sec = Mathf.FloorToInt(TimeCount % 60);
                SetNumbers(sec, 2, 3);

                //分
                int minu = Mathf.FloorToInt((TimeCount) / 60);
                SetNumbers(minu, 0, 1);

                //1秒待つ
                yield return new WaitForSeconds(1.0f);

                //カウントアップ
                TimeCount += 1.0f;
            }

            //CountOptionがfalse
            if (CountOption==false)
            {
                //カウントダウン秒
                int Do_sec = Mathf.FloorToInt(TimeCount % 60);
                St_Numbers(Do_sec, 2, 3);

                //カウントダウン分
                int Do_minu = Mathf.FloorToInt(TimeCount % 60);
                St_Numbers(Do_minu, 0, 1);

                //1秒待つ
                yield return new WaitForSeconds(1.0f);

                //カウントダウン
                TimeCount -= 1.0f;

                //0以下になったら
                if (TimeCount <= 0)
                {
                    TimeCount = 0;
                    //Start_Textを表示
                    Start_Text.SetActive(true);

                    //数字非表示
                    ST_Number.SetActive(false);

                    //CountOptionをtrueにする
                    CountOption = true;
                }

            }
        }
    }

	/// <summary>
    /// スタートしたら
    /// </summary>
	void Start ()
    {
        //コルーチンでカウントダウン
        StartCoroutine(TimerStart());
    }

    /// <summary>
    /// クリアUI
    /// </summary>
    void Clea()
    {
        //Judgment_CLがtrueなら
        if (Judgment_CL == true)
        {
            //クリアを表示
            Clea_Object.SetActive(true);

            //オーバー非表示
            Over_Object.SetActive(false);

            //非表示
            Judgment_OV = false;
        }

        //Judgment_CLがfalseなら
        if (Judgment_CL == false)
        {
            //クリア非表示
            Clea_Object.SetActive(false);
        }
    }

    /// <summary>
    /// オーバーUI
    /// </summary>
    void Over()
    {
        //Judgment_OVがtrueなら
        if (Judgment_OV == true)
        {
            //オーバーを表示
            Over_Object.SetActive(true);

            //クリア非表示
            Clea_Object.SetActive(false);

            //非表示
            Judgment_CL = false;
        }
        //Judgment_OVがfalseなら
        if (Judgment_OV == false)
        {
            //オーバー非表示
            Over_Object.SetActive(false);
        }
    }

    /// <summary>
    /// アップデート
    /// </summary>
    void Update ()
    {
        //クリア表示
        Clea();

        //オーバー表示
        Over();
    }
}
