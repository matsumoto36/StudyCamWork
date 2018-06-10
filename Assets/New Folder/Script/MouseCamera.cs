using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCamera : MonoBehaviour {

	public Player targetPlayer;
	public MouseCameraObject cameraObject;

	public Text scoreText;
	public Text accText;
	public Text comboText;
	public Slider hpBar;

	public bool isCapture = false;
	public bool isPlayerFocus;

    public int life;				//体力
    public int lifeDamage;			//ダメージの数値
    public int Combo;				//コンボ

    public int Score;               //最終的に入るスコア
	int scoreWithoutCombo;			//現時点でコンボ抜きにしたスコア
	int scoreMax;					//現時点でのスコアの最大値

	GameBalanceData gameBalance;	//ゲームのバランスを決めるクラス
	Vector2 wideCameraSize;
	Vector2 smallCameraSize;


	int plus; //プラスのスコアをスコアに入れるためのもの

	bool isGameStart;　　　　//ゲームスタート

    float playTime;
    int comboTimeCount;
    int lifeTimeCount;

	float gaugeAmount = 1;
	float gauge = 1;

	GimmickManager gimmickManager;
	DOFSlide dofSlide;

	bool isTeleport;
	public bool IsTeleport {        //テレポート中かどうか
		get { return isTeleport; }
		set {
			isTeleport = value;
			if(!isTeleport) CameraUpdate();
		}
	}

	public float Accuracy {
		get { return (float)scoreWithoutCombo / scoreMax; }
	} 

	// Use this for initialization
	public void Init () {

		targetPlayer = GameObject.FindGameObjectWithTag("Player")
			.GetComponent<Player>();

		gameBalance = GameMaster.gameMaster.gameBalanceData;

		//カメラのサイズを設定
		wideCameraSize = new Vector2(Screen.width, Screen.height) * gameBalance.CameraWideSizeRatio;
		smallCameraSize = wideCameraSize * gameBalance.CameraSmallSizeRatio;

		//カメラ用のオブジェクトの設定
		cameraObject.Init();
		cameraObject.SetCameraSize(wideCameraSize);
		cameraObject.UpdateCameraPosition(Camera.main.WorldToScreenPoint(targetPlayer.transform.position));

		GameMaster.gameMaster.OnGameStart += () => {
            Debug.Log("a");
			Cursor.visible = false;
			isGameStart = true;
			playTime = 0;
			Score = 0;
			scoreWithoutCombo = 0;
			scoreMax = 0;
		};

		GameMaster.gameMaster.OnGameClear += () => {
			Cursor.visible = true;
			isGameStart = false;
			cameraObject.CameraColorType = CameraColorType.Normal;
		};

		GameMaster.gameMaster.OnGameOver += () => {
			Cursor.visible = true;
			isGameStart = false;
			cameraObject.CameraColorType = CameraColorType.Normal;
		};

		Combo = 0;

		dofSlide = FindObjectOfType<DOFSlide>();
		gimmickManager = FindObjectOfType<GimmickManager>();
	}

	// Update is called once per frame
	public void CameraUpdate () {

		cameraObject.UpdateCameraPosition(Input.mousePosition);

		if(isGameStart) {
        
			playTime += Time.deltaTime;

			if(Input.GetMouseButton(0)) {
				gauge = Mathf.Max(gauge - Time.deltaTime / gaugeAmount, 0);
			}
			else {
				gauge = Mathf.Min(gauge + Time.deltaTime / gaugeAmount, 1);
			}

			if(isCapture = IsPlayerCapture())
            {
                IsPSmallCapture();
            }
			else {
				cameraObject.CameraColorType = CameraColorType.Fail;
			}

			SmallCap();
			ComboChain();

			hpBar.value = life;
			scoreText.text = Score.ToString("000000");
			if(Accuracy == 1.0f) accText.text = "100%";
			else accText.text = Accuracy.ToString("P");
			comboText.text = "x" + Combo.ToString("");
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
			(Vector2)Input.mousePosition - (wideCameraSize / 2),
			wideCameraSize + sizeOffset);

		var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);  //スクリーン座標に置き換える

		return rect.Contains(checkPoint);
        
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
        if (isCapture)
        {
            if (playTime>comboTimeCount)
            {
                comboTimeCount++;

				var point = plus + gameBalance.BaseScore;

				if (Combo>=1 )
                {
                     Score += (int)(point * Combo * 1.05);
                }
                else if(Combo >= 0)
                {
                   Score += point;
                }

				scoreWithoutCombo += point;
				scoreMax += gameBalance.BaseScore + gameBalance.CameraInsideScore;

				Combo++;
            }
        }
        else
        {
			if(!IsTeleport) {

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
            plus = gameBalance.CameraInsideScore;
        }
        else
        {
            plus = 0;
        }
        }
}
