﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCamera : MonoBehaviour
{

    public MouseCameraObject cameraObject;

    public Slider hpBar;

    //public bool isCapture = false;
    //public bool isPlayerFocus;

    int life = 100;				//体力
    int lifeDamage;			//ダメージの数値
    public static int Combo;				//コンボ
    public static int act;
    public static int Score;               //最終的に入るスコア
    static int scoreWithoutCombo;          //現時点でコンボ抜きにしたスコア
    static int scoreMax;                   //現時点でのスコアの最大値

	Player targetPlayer;
	GameBalanceData gameBalance;    //ゲームのバランスを決めるクラス
    Vector2 wideCameraSize;
    Vector2 smallCameraSize;
    public Text scoreText;
    public Text accText;
    public Text comboText;

    int plus; //プラスのスコアをスコアに入れるためのもの

    bool isGameStart;　　　　//ゲームスタート

    float playTime;
    int comboTimeCount;
    int lifeTimeCount;
    public int ComboData;
    public int AccuaryTex;
    float gaugeAmount = 1;
    float gauge = 1;

    GimmickManager gimmickManager;
    DOFSlide dofSlide;

    bool isTeleport;
    bool DamagOn = false;

	public PlayerCaptureStatus CaptureStatus {
		get; private set;
	}

    public bool IsTeleport
    {        //テレポート中かどうか
        get { return isTeleport; }
        set
        {
            isTeleport = value;
            if (!isTeleport) CameraUpdate();
            Debug.Log("bbbb");
        }
    }

    public static float Accuracy
    {
        get { return (float)scoreWithoutCombo / scoreMax; }
    }

    // Use this for initialization
    public void Init(Player player)
    {
        
        targetPlayer = player;
        gameBalance = GameMaster.Instance.GameBalanceData;

		//受けるダメージを設定
		lifeDamage = gameBalance.Damage;

        //カメラのサイズを設定
        wideCameraSize = new Vector2(Screen.width, Screen.height) * gameBalance.CameraWideSizeRatio;
        smallCameraSize = wideCameraSize * gameBalance.CameraSmallSizeRatio;

        

        //カメラ用のオブジェクトの設定
        cameraObject.Init();
        cameraObject.SetCameraSize(wideCameraSize);
        cameraObject.UpdateCameraPosition(Camera.main.WorldToScreenPoint(targetPlayer.transform.position));

        GameMaster.Instance.OnGameStart += () => {
            Debug.Log("a");
            Cursor.visible = false;
            isGameStart = true;
            playTime = 0;
            Score = 0;
            scoreWithoutCombo = 0;
            scoreMax = 0;
        };

        GameMaster.Instance.OnGameClear += () => {
            Cursor.visible = true;
            isGameStart = false;
            cameraObject.CameraColorType = CameraColorType.Normal;
        };

        GameMaster.Instance.OnGameOver += () => {
            Cursor.visible = true;
            isGameStart = false;
            cameraObject.CameraColorType = CameraColorType.Normal;
        };

        Combo = 0;

        dofSlide = FindObjectOfType<DOFSlide>();
        gimmickManager = FindObjectOfType<GimmickManager>();
    }

    // Update is called once per frame
    public void CameraUpdate()
    {

        cameraObject.UpdateCameraPosition(Input.mousePosition);

        if (isGameStart)
        {

            playTime += Time.deltaTime;

            if (Input.GetMouseButton(0))
            {
                gauge = Mathf.Max(gauge - Time.deltaTime / gaugeAmount, 0);
            }
            else
            {
                gauge = Mathf.Min(gauge + Time.deltaTime / gaugeAmount, 1);
            }

			CaptureStatus = IsPlayerCapture();
			//プレイヤーにステータスを伝える
            targetPlayer.SetLight(CaptureStatus);

            if (CaptureStatus != PlayerCaptureStatus.All)
            {
                cameraObject.CameraColorType = CameraColorType.Fail;
            }
            
            ComboChain();
            hpBar.value = life;
            scoreText.text = Score.ToString("000000");
            if (Accuracy == 1.0f) accText.text = ("100%");
            else accText.text = Accuracy.ToString("P");
            comboText.text = "x" + Combo.ToString("");
        }
    }


	/// <summary>
	/// プレイヤーがいい感じにキャプチャーされているか判定する
	/// </summary>
	/// <returns></returns>
    PlayerCaptureStatus IsPlayerCapture()
    {                   //主なあたり判定

        if (!targetPlayer) return PlayerCaptureStatus.None;

        //フォーカスできているか調べる
        var playerZRate = targetPlayer.transform.GetChild(0).localPosition.z / gimmickManager.moveZ;
        var focusRate = dofSlide.Value;
        var focusGrace = GameMaster.Instance.GameBalanceData.FocusGrace;
		var isFocus = Mathf.Abs(playerZRate - focusRate) <= focusGrace;

        //枠に入っているか調べる
        var sizeOffset = new Vector2(0, 0);
		var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);  //スクリーン座標に置き換える
        var rect = new Rect(
			(Vector2)Input.mousePosition - (wideCameraSize / 2),
            wideCameraSize + sizeOffset);

		var isInside = rect.Contains(checkPoint);

		if(!isFocus && !isInside) return PlayerCaptureStatus.None;
		if(!isFocus && isInside) return PlayerCaptureStatus.ContainOnly;
		if(isFocus && !isInside) return PlayerCaptureStatus.FocusOnly;

		rect = new Rect(
			(Vector2)Input.mousePosition - (smallCameraSize / 2),
			smallCameraSize + sizeOffset);

		var isInsideSmall = rect.Contains(checkPoint);

		if(isFocus && !isInsideSmall) return PlayerCaptureStatus.Near;

		return PlayerCaptureStatus.All;
    }
    bool IsPSmallCapture()
    {

        if (!targetPlayer) return false;

        var sizeOffset = new Vector2(0, 0);
        var rect = new Rect(
            (Vector2)Input.mousePosition - (smallCameraSize / 2),
            smallCameraSize + sizeOffset);

        var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);
        return rect.Contains(checkPoint);
        

    }
    void ComboChain()
    {
        if (playTime > comboTimeCount)
        {
            comboTimeCount++;
            if (CaptureStatus == PlayerCaptureStatus.Near ||
				CaptureStatus == PlayerCaptureStatus.All)
            {
				if(CaptureStatus == PlayerCaptureStatus.All) {
					plus = gameBalance.CameraInsideScore;
				}
				else {
					plus = 0;
				}

				var point = plus + gameBalance.BaseScore;
                if (Combo >= 1)
                {
                    Score += (int)(point * Combo * 1.05);
                }
                else if (Combo >= 0)
                {
                    Score += point;
                }

                scoreWithoutCombo += point;
                if (!isTeleport)
                {
                    Combo++;
                }
               
            }
            else
            {
                if (!IsTeleport)
                {
                    life -= lifeDamage;
                }

				Debug.Log("damage");
                if (!IsTeleport)
                {
                    if (ComboData <= Combo)
                    {
                        ComboData =Combo;
                    }
                    Combo = 0;
                    if (lifeTimeCount < playTime)
                    {
                        if (life <= 0.0f) GameMaster.Instance.GameOver();
                    }
                }
            }

			scoreMax += gameBalance.BaseScore + gameBalance.CameraInsideScore;
		}
	}
}