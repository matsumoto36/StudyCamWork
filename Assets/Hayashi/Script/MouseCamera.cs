using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCamera : MonoBehaviour
{

    public MouseCameraObject cameraObject;

    public Slider hpBar;
    public Slider inCamhpBar;

	public RectTransform HPFlash;

	int life = 100;				//体力
    int lifeDamage;			//ダメージの数値
    public static int Combo;				//コンボ
    public static int ComboMax;                //コンボ
	public static int act;
    public static int Score;               //最終的に入るスコア
    static int scoreWithoutCombo;          //現時点でコンボ抜きにしたスコア
    static int scoreMax;                   //現時点でのスコアの最大値

	Player targetPlayer;
	GameBalanceData gameBalance;    //ゲームのバランスを決めるクラス
    Vector2 wideCameraSize;
    Vector2 smallCameraSize;

    public Text scoreText;
	int scoreUpRate;
	int scoreTextView;

	public Text accText;
	float accDownRate;
	float accTextView = 1;

    public Text comboText;
	int comboTextView;

	int plus; //プラスのスコアをスコアに入れるためのもの

    bool isGameStart;　　　　//ゲームスタート

    float playTime;
	float checkSpeed;
    int comboTimeCount;
    public int AccuaryTex;
    float gaugeAmount = 1;
    float gauge = 1;

    DOFSlide dofSlide;

	float warningPlayTime;

	public PlayerCaptureStatus CaptureStatus {
		get; private set;
	}

    bool isTeleport;
    public bool IsTeleport
    {        //テレポート中かどうか
        get { return isTeleport; }
        set
        {
            isTeleport = value;
			if(!isTeleport) {
				CameraUpdate();
				Apply();
			}
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

		//チェックの速さの設定
		checkSpeed = 1 / gameBalance.CheckWait;

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
            ComboMax = 0;
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
    }

    // Update is called once per frame
    public void CameraUpdate()
    {

		if(Input.GetKeyDown(KeyCode.T)) {
			StartCoroutine(FlashComboText());
		}

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

			CaptureStatus = IsPlayerCapture(cameraObject.GetObjectPosition());


			if(IsTeleport) {
				targetPlayer.SetLight(PlayerCaptureStatus.All);
				cameraObject.CameraColorType = CameraColorType.Normal;
			}
			else {
				targetPlayer.SetLight(CaptureStatus);

				switch(CaptureStatus) {
					case PlayerCaptureStatus.All:
						cameraObject.CameraColorType = CameraColorType.Normal;
						break;
					case PlayerCaptureStatus.Near:
						cameraObject.CameraColorType = CameraColorType.Normal;
						break;
					default:
						cameraObject.CameraColorType = CameraColorType.Fail;
						break;
				}
			}


            
            ComboChain();
			UpdateUI();


		}
    }

	void CalcScoreUpRate() {
		scoreUpRate = (int)((Score - scoreTextView) * Time.deltaTime);
	}

	void UpdateUI() {

		hpBar.value = life;
		inCamhpBar.value = life;

		scoreTextView = Mathf.Min(scoreTextView + scoreUpRate, Score);
		scoreText.text = scoreTextView.ToString();

		accTextView = Mathf.MoveTowards(accTextView, Accuracy, Time.deltaTime / 10);
		if(accTextView == 1.0f) accText.text = ("100%");
		else accText.text = accTextView.ToString("P");

		comboText.text = "x" + Combo.ToString("");
	}

	/// <summary>
	/// プレイヤーがいい感じにキャプチャーされているか判定する
	/// </summary>
	/// <returns></returns>
	PlayerCaptureStatus IsPlayerCapture(Vector2 cameraPosition)
    {                   //主なあたり判定

		//チート
		if (Input.GetKey(KeyCode.F)) return PlayerCaptureStatus.All;
        if (!targetPlayer) return PlayerCaptureStatus.None;

        //フォーカスできているか調べる
        var playerZRate = targetPlayer.Body.localPosition.z / GimmickManager.MoveZ;
        var focusRate = dofSlide.Value;
        var focusGrace = GameMaster.Instance.GameBalanceData.FocusGrace;
		var isFocus = Mathf.Abs(playerZRate - focusRate) <= focusGrace;

        //枠に入っているか調べる
        var sizeOffset = new Vector2(0, 0);
		var checkPoint = Camera.main.WorldToScreenPoint(targetPlayer.transform.position);  //スクリーン座標に置き換える
        var rect = new Rect(
			cameraPosition - (wideCameraSize / 2),
            wideCameraSize + sizeOffset);

		var isInside = rect.Contains(checkPoint);

		if(!isFocus && !isInside) return PlayerCaptureStatus.None;
		if(!isFocus && isInside) return PlayerCaptureStatus.ContainOnly;
		if(isFocus && !isInside) return PlayerCaptureStatus.FocusOnly;

		rect = new Rect(
			cameraPosition - (smallCameraSize / 2),
			smallCameraSize + sizeOffset);

		var isInsideSmall = rect.Contains(checkPoint);

		if(isFocus && !isInsideSmall) return PlayerCaptureStatus.Near;

		return PlayerCaptureStatus.All;
    }

    void ComboChain()
    {
        if (playTime * checkSpeed > comboTimeCount)
        {
			comboTimeCount++;
			Apply();

		}
	}

	/// <summary>
	/// 実行されると判定によってスコア上昇かダメージが入る
	/// </summary>
	void Apply() {

		if(!isTeleport) scoreMax += gameBalance.BaseScore + gameBalance.CameraInsideScore;

		if(CaptureStatus == PlayerCaptureStatus.Near ||
			CaptureStatus == PlayerCaptureStatus.All) {
			if(!isTeleport) {
				if(CaptureStatus == PlayerCaptureStatus.All) {
					plus = gameBalance.CameraInsideScore;
				}
				else {
					plus = 0;
				}

				var point = plus + gameBalance.BaseScore;

				Combo++;

				if(Combo >= 1) {
					Score += (int)(point * Combo * 1.05);
				}
				else if(Combo >= 0) {
					Score += point;
				}

				if(ComboMax < Combo) {
					Debug.Log("AddCombo " + Combo);
					ComboMax = Combo;
				}

				AudioManager.PlaySE("decision22");

				CalcScoreUpRate();
				scoreWithoutCombo += point;

				StartCoroutine(FlashComboText());
			}

		}
		else {
			if(!IsTeleport) {


				life -= lifeDamage;

				if(Time.time - warningPlayTime > 0.7f) {
					AudioManager.PlaySE("warning2");
					warningPlayTime = Time.time;
				}

				//ダメージ演出
				var damage = life < 0 ? lifeDamage + life : lifeDamage;
				StartCoroutine(FlashHPDown(damage));
				StartCoroutine(cameraObject.DamageFlash());

				Debug.Log("damage");

				if(life <= 0.0f) {
					UpdateUI();
					GameMaster.Instance.GameOver();
				}
			}
			if(!IsTeleport) {
				Combo = 0;
			}
		}

	}

	IEnumerator FlashComboText() {

		yield return null;
		var cText = Instantiate(comboText, comboText.transform);
		cText.transform.SetParent(comboText.transform.parent);
		cText.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(1.0f / 3);
		Destroy(cText.gameObject);
	}

	IEnumerator FlashHPDown(float downHP) {

		var hpDownBar = Instantiate(HPFlash, HPFlash.transform);
		hpDownBar.transform.SetParent(HPFlash.transform.parent);

		var parentT = hpDownBar.transform.parent.GetComponent<RectTransform>();
		var width = parentT.sizeDelta.x * downHP * 0.01f;
		var posX = parentT.sizeDelta.x * (life) * 0.01f;

		var image = hpDownBar.GetChild(0).GetComponent<RectTransform>();
		image.sizeDelta
			= new Vector2(width, image.sizeDelta.y);
		hpDownBar.anchoredPosition
			= new Vector2(posX, -43);

		hpDownBar.GetComponent<Animation>().Play();

		yield return new WaitForSeconds(0.5f);
		Destroy(hpDownBar.gameObject);
	}
}