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

    //表示する数字少数まで
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


    //時間を画像で表示
    void SetNumbers(int sec, int val1, int val2)
    {
        string str = string.Format("{0:00}", sec);
            Images[val1].sprite = NumberSprites[Convert.ToInt32(str.Substring(0, 1))];//秒
            Images[val2].sprite = NumberSprites[Convert.ToInt32(str.Substring(1, 1))];//分
    }

    //コルーチン分秒
    IEnumerator TimerStart()
    {
        //TimeCountが0以上なら
        while (0<=TimeCount)
        {
            //秒
            int sec = Mathf.FloorToInt(TimeCount % 60);
            SetNumbers(sec, 2, 3);
            //分
            int minu = Mathf.FloorToInt((TimeCount) / 60);
            SetNumbers(minu, 0, 1);
            yield return new WaitForSeconds(1.0f);
            //CountOptionがtrueなら
            if (CountOption == true)
            {
                TimeCount += 1.0f;//カウントアップ
            }
            //CountOptionがfalse
            if (CountOption==false)
            {
                TimeCount -= 1.0f;//カウントダウン
                //0以下になったら
                if (TimeCount <= 0)
                {
                    //Start_Textを表示
                    Start_Text.SetActive(true);
                    TimeCount = 0;
                    //表示時間
                    Text_interval -= 1.0f;

                    //表示時間を過ぎたら
                    if (0>=Text_interval)
                    {
                        //Start_Textを非表示
                        Start_Text.SetActive(false);
                        //CountOptionをtrueにする
                        CountOption = true;
                    }
                }

            }
        }
    }


	
	void Start ()
    {
        //コルーチンでカウントダウン
        StartCoroutine(TimerStart());
    }
	
	void Update ()
    {
        
	}
}
