using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour {

    public Text TimerText;//UI文字テキスト

    public float TotalTime;//現在のタイム

    int Seconds;//秒

    public int Timer_Min = 0;//最小値

    public bool Controller_On = true;//カウントアップかダウン

	void Start ()
    {
		
	}
	
	
	void Update ()
    {
        //trueなら
        if (Controller_On==true)
        {
            //カウントダウン
            Count_Down();
        }
        //falseなら
        if (Controller_On==false)
        {
            //カウントアップ
            Count_Up();
        }
	}

    /// <summary>
    ///カウントダウン
    /// </summary>
    public void Count_Down()
    {
        //現在のタイムに減算
        TotalTime -= Time.deltaTime;

        //int型に変換
        Seconds = (int)TotalTime;

        //テキストに表示
        TimerText.text = Seconds.ToString();

        //最小値以下なら
        if (TotalTime<=Timer_Min)
        {
            //最小値にする
            TotalTime = Timer_Min;
            //最小値になったらスタート文字に変わる
            TimerText.text = "スタート";

        }
    }

    /// <summary>
    /// カウントアップ
    /// </summary>
    public void Count_Up()
    {
        //現在のタイムに加算
        TotalTime += Time.deltaTime;

        //int型に変換
        Seconds = (int)TotalTime;

        //テキストに表示
        TimerText.text = Seconds.ToString();

        //最小値以上なら
        if (TotalTime >= Timer_Min)
        {
            //最小値にする
            TotalTime = Timer_Min;
            //最小値になったらゲームオーバー文字に変わる
            TimerText.text = "ゲームオーバー";

        }
    }
}
