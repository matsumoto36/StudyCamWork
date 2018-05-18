using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームスタート
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
	
	void Update ()
    {
        
	}
}
