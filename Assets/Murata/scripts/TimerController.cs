using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerController : MonoBehaviour {

    public Text TimerText;//UI文字テキスト

    public float TotalTime;//現在のタイム

    int Seconds;//秒

    public int Timer_Min = 0;//最小値

    public float Clear_interval = 0;//スターt表示間隔

    public bool Controller_On = true;//カウントアップかダウン

    public static string Next_Scene;//シーン名

    public bool Scene_go = true;

	void Start ()
    {
		
	}
	
	
	void Update ()
    {
        //trueなら
        if (Controller_On==true &Scene_go==false)
        {
            //カウントダウン
            Count_Down();
        }
        //falseなら
        if (Controller_On==false&Scene_go==false)
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
            Clear_interval += Time.deltaTime;
            //最小値になったらスタート文字に変わる
            TimerText.text = "スタート";
            if (Clear_interval>=3)
            {
                TimerText.text="　";
            }
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

    /// <summary>
    /// シーン遷移
    /// </summary>
    /// <param name="NextSceneName"></param>
    public void stageSelect(string NextSceneName)
    {
        Next_Scene = NextSceneName;//Next_Sceneに名前を入れる
        SceneManager.LoadScene(Next_Scene);//飛びますよ。
    }
}
