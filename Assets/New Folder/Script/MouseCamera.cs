﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCamera : MonoBehaviour {

	public Player targetPlayer;
	public GameObject nonCaptureContent;
	public Image cameraImage;
	public MouseCameraObject cameraObject;

	//public Image cameraGauge;
	public Vector2 wideCameraSize = new Vector2(300f, 200f);
    public Vector2 smallCameraSize = new Vector2(150f, 100f);

	public bool isCapture = false;
	public bool isPlayerFocus;

	bool isTeleport;
	public bool IsTeleport {		//テレポート中かどうか
		get { return isTeleport; }
		set {
			isTeleport = value;
			if(!isTeleport) CameraUpdate();
		}
	}

    public int life;				//体力
    public int lifeDamage;			//ダメージの数値
    public int Combo;				//コンボ
    public int baseScore;			//ベースのスコア
    public int Score;				//最終的に入るスコア
    public int BonusScore;			//中心地点で獲得するスコア
    int plus; //プラスのスコアをスコアに入れるためのもの

    Color baseCameraColor = Color.white;
	Color captureCameraColor = Color.cyan;
	Color filterCameraColor = Color.yellow;
	Color failCameraColor = Color.red;

	bool isGameStart;　　　　//ゲームスタート
	bool isFilterOn;
    bool ComboPlus;

    float playTime;
    int comboTimeCount;
    int lifeTimeCount;
	float captureTime;　　　

	float gaugeAmount = 1;
	float gauge = 1;

	GimmickManager gimmickManager;
	DOFSlide dofSlide;

	public float CaptureRate {
		get { return captureTime / playTime; }
	} 

	// Use this for initialization
	public void Init () {

		Cursor.visible = false;
       
		cameraImage.rectTransform.sizeDelta = wideCameraSize;

		//カメラ用のオブジェクトの設定
		cameraObject.Init();
		cameraObject.SetCameraSize(wideCameraSize);

		targetPlayer = GameObject.FindGameObjectWithTag("Player")
			.GetComponent<Player>();

		GameMaster.gameMaster.OnGameStart += () => {
            Debug.Log("a");
			isGameStart = true;
			playTime = 0;
			captureTime = 0;
		};

		GameMaster.gameMaster.OnGameClear += () => {
			isGameStart = false;
			cameraImage.color = baseCameraColor;
		};

		GameMaster.gameMaster.OnGameOver += () => {
			isGameStart = false;
			cameraImage.color = baseCameraColor;
		};

		Combo = 0;
        ComboPlus = false;

		dofSlide = FindObjectOfType<DOFSlide>();
		gimmickManager = FindObjectOfType<GimmickManager>();
	}

	// Update is called once per frame
	public void CameraUpdate () {

        cameraImage.rectTransform.position = Input.mousePosition;
		cameraObject.UpdateCameraPosition(Input.mousePosition);

		if(isGameStart) {
        
			playTime += Time.deltaTime;

			if(Input.GetMouseButton(0)) {
				gauge = Mathf.Max(gauge - Time.deltaTime / gaugeAmount, 0);
			}
			else {
				gauge = Mathf.Min(gauge + Time.deltaTime / gaugeAmount, 1);
			}

			isFilterOn = gauge > 0;
			//cameraGauge.fillAmount = gauge;
			if(isCapture = IsPlayerCapture())
            {
            
                IsPSmallCapture();
                if (IsCaputureNonCaptureContent()) {

					if(isFilterOn) {
						captureTime += Time.deltaTime;
						cameraImage.color = filterCameraColor;
					}
					else {
						cameraImage.color = failCameraColor;
					}
				}
				else {
					captureTime += Time.deltaTime;
					cameraImage.color = captureCameraColor;
				}

         
            }
			else {
				cameraImage.color = failCameraColor;
			}

			SmallCap();
			ComboChain();
		}
        

    }

    bool IsPlayerCapture() {					//主なあたり判定

		if(!targetPlayer) return false;

		//フォーカスできているか調べる
		var playerZRate = targetPlayer.transform.GetChild(0).localPosition.z / gimmickManager.moveZ;
		var focusRate = dofSlide.Value;
		var focusGrace = GameMaster.gameMaster.gameBalanceData.FocusGrace;
		if(Mathf.Abs(playerZRate - focusRate) > focusGrace) return false;

		//枠に入っているか調べる
		var sizeOffset = new Vector2(0, 0);
		var rect = new Rect(
			(Vector2)cameraImage.rectTransform.position - (wideCameraSize / 2),
			wideCameraSize + sizeOffset);

		var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);  //スクリーン座標に置き換える

		return rect.Contains(checkPoint);
        
	}
    bool IsPSmallCapture()
    {   

        if (!targetPlayer) return false;

        var sizeOffset = new Vector2(0, 0);
        var rect = new Rect(
            (Vector2)cameraImage.rectTransform.position - (smallCameraSize / 2),
           smallCameraSize + sizeOffset);
        
        var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);
        return rect.Contains(checkPoint);


    }
    bool IsCaputureNonCaptureContent() {

		if(!nonCaptureContent) return false;

		var sizeOffset = new Vector2(0, 0);
		var rect = new Rect(
			(Vector2)cameraImage.rectTransform.position - (wideCameraSize / 2),
			wideCameraSize + sizeOffset);

		var checkPoint = Camera.main.WorldToScreenPoint(nonCaptureContent.transform.position);

		return rect.Contains(checkPoint);
	}
    void ComboChain()
    {
        if (isCapture)
        {
            ComboPlus = true;
            if (playTime>comboTimeCount)
            {
                comboTimeCount++;
                if (Combo>=1 )
                {
                     Score += (int)(plus + baseScore * Combo * 1.05);
                }
                else if(Combo >= 0)
                {
                   Score+=(baseScore+plus);
                }
                
                Combo++;
            }
        }
        else
        {
			if(!IsTeleport) {

				ComboPlus = false;
				Combo = 0;
				if(lifeTimeCount < playTime) {
					lifeTimeCount++;
					life -= lifeDamage;
					if(life <= 0.0f) GameMaster.gameMaster.GameOver();
				}
			}
        }
    }
        void SmallCap()
        {
        if (IsPSmallCapture())
        {
            plus = BonusScore;
        }
        else
        {
            plus = 0;
        }
        }
}
