using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCamera : MonoBehaviour
{

    public MouseCameraObject CameraObject;

    public Slider HpBar;
    public Slider InCameraInsideHPBar;
	public Text ScoreText;
	public Text AccText;
	public Text ComboText;

	public RectTransform HPFlash;

	public int ComboMax;                //コンボ
	public int Score;               //最終的に入るスコア

    private int _combo;              //コンボ
	private int _scoreWithoutCombo;          //現時点でコンボ抜きにしたスコア
	private int _scoreMax;                   //現時点でのスコアの最大値

	private int _life = 100;              //体力

	private Player _targetPlayer;
	private GameBalanceData _gameBalance;    //ゲームのバランスを決めるクラス
    private Vector2 _wideCameraSize;
	private Vector2 _smallCameraSize;

	private int _scoreUpRate;
	private int _scoreTextView;

	private float _accTextView = 1;

	private bool _isGameStart;    //ゲームスタート

	private float _playTime;
	private float _checkSpeed;
	private int _comboTimeSync;

	private DOFSlide _dofSlide;
	private float _warningPlayTime;

	public PlayerCaptureStatus CaptureStatus {
		get; private set;
	}

	private bool _isTeleport;
	public bool IsTeleport
    {        //テレポート中かどうか
        get { return _isTeleport; }
        set
        {
            _isTeleport = value;
	        if (_isTeleport) return;
	        CameraUpdate();
	        Apply();
        }
    }

    public float Accuracy
    {
        get { return (float)_scoreWithoutCombo / _scoreMax; }
    }

    // Use this for initialization
    public void Init(Player player)
    {
        
        _targetPlayer = player;
        _gameBalance = GameMaster.Instance.GameBalanceData;

		//チェックの速さの設定
		_checkSpeed = 1 / _gameBalance.CheckWait;

		//カメラのサイズを設定
		_wideCameraSize = new Vector2(Screen.width, Screen.height) * _gameBalance.CameraWideSizeRatio;
        _smallCameraSize = _wideCameraSize * _gameBalance.CameraSmallSizeRatio;

        

        //カメラ用のオブジェクトの設定
        CameraObject.Init();
        CameraObject.SetCameraSize(_wideCameraSize);
        CameraObject.UpdateCameraPosition(Camera.main.WorldToScreenPoint(_targetPlayer.transform.position));

        GameMaster.Instance.OnGameStart += () => {
            Debug.Log("a");
            Cursor.visible = false;
            _isGameStart = true;
            _playTime = 0;
            Score = 0;
            _scoreWithoutCombo = 0;
            _scoreMax = 0;
            ComboMax = 0;
        };

        GameMaster.Instance.OnGameClear += () => {
            Cursor.visible = true;
            _isGameStart = false;
            CameraObject.CameraColorType = CameraColorType.Normal;
        };

        GameMaster.Instance.OnGameOver += () => {
            Cursor.visible = true;
            _isGameStart = false;
            CameraObject.CameraColorType = CameraColorType.Normal;
        };

        _combo = 0;

        _dofSlide = FindObjectOfType<DOFSlide>();
    }

    // Update is called once per frame
    public void CameraUpdate()
    {

        CameraObject.UpdateCameraPosition(Input.mousePosition);

	    if (!_isGameStart) return;

	    _playTime += Time.deltaTime;

	    CaptureStatus = IsPlayerCapture(CameraObject.GetObjectPosition());


	    if(IsTeleport) {
		    _targetPlayer.SetLight(PlayerCaptureStatus.All);
		    CameraObject.CameraColorType = CameraColorType.Normal;
	    }
	    else {
		    _targetPlayer.SetLight(CaptureStatus);

		    switch(CaptureStatus) {
			    case PlayerCaptureStatus.All:
				    CameraObject.CameraColorType = CameraColorType.Normal;
				    break;
			    case PlayerCaptureStatus.Near:
				    CameraObject.CameraColorType = CameraColorType.Normal;
				    break;
			    default:
				    CameraObject.CameraColorType = CameraColorType.Fail;
				    break;
		    }
	    }



		if(_playTime * _checkSpeed > _comboTimeSync) {
			_comboTimeSync++;
			Apply();

		}

		UpdateUI();
    }

	/// <summary>
	/// スコアの上昇率を計算
	/// </summary>
	private void CalcScoreUpRate() {
		_scoreUpRate = (int)((Score - _scoreTextView) * Time.deltaTime);
	}


	/// <summary>
	/// 表示部分の更新
	/// </summary>
	private void UpdateUI() {

		HpBar.value = _life;
		InCameraInsideHPBar.value = _life;

		_scoreTextView = Mathf.Min(_scoreTextView + _scoreUpRate, Score);
		ScoreText.text = _scoreTextView.ToString();

		_accTextView = Mathf.MoveTowards(_accTextView, Accuracy, Time.deltaTime / 10);
		AccText.text = _accTextView >= 1.0f ? "100%" : _accTextView.ToString("P");

		ComboText.text = "x" + _combo.ToString("");
	}

	/// <summary>
	/// プレイヤーがいい感じにキャプチャーされているか判定する
	/// </summary>
	/// <returns></returns>
	private PlayerCaptureStatus IsPlayerCapture(Vector2 cameraPosition)
    {

		//チート
		if (Input.GetKey(KeyCode.F)) return PlayerCaptureStatus.All;
        if (!_targetPlayer) return PlayerCaptureStatus.None;

        //フォーカスできているか調べる
        var playerZRate = _targetPlayer.Body.localPosition.z / GimmickManager.MoveZ;
        var focusRate = _dofSlide.Value;
        var focusGrace = GameMaster.Instance.GameBalanceData.FocusGrace;
		var isFocus = Mathf.Abs(playerZRate - focusRate) <= focusGrace;

        //枠に入っているか調べる
        var sizeOffset = new Vector2(0, 0);
		var checkPoint = Camera.main.WorldToScreenPoint(_targetPlayer.transform.position);  //スクリーン座標に置き換える
        var rect = new Rect(
			cameraPosition - (_wideCameraSize / 2),
            _wideCameraSize + sizeOffset);

		var isInside = rect.Contains(checkPoint);

		if(!isFocus && !isInside) return PlayerCaptureStatus.None;
		if(!isFocus && isInside) return PlayerCaptureStatus.ContainOnly;
		if(isFocus && !isInside) return PlayerCaptureStatus.FocusOnly;

		rect = new Rect(
			cameraPosition - (_smallCameraSize / 2),
			_smallCameraSize + sizeOffset);

		var isInsideSmall = rect.Contains(checkPoint);

		if(isFocus && !isInsideSmall) return PlayerCaptureStatus.Near;

		return PlayerCaptureStatus.All;
    }

	/// <summary>
	/// 実行されると判定によってスコア上昇かダメージが入る
	/// </summary>
	private void Apply() {

		if(!_isTeleport) _scoreMax += _gameBalance.BaseScore + _gameBalance.CameraInsideScore;

		if(CaptureStatus == PlayerCaptureStatus.Near ||
			CaptureStatus == PlayerCaptureStatus.All) {
			if (_isTeleport) return;

			var plus = CaptureStatus == PlayerCaptureStatus.All ? _gameBalance.CameraInsideScore : 0;
			var point = plus + _gameBalance.BaseScore;

			_combo++;

			if(_combo >= 1) {
				Score += (int)(point * _combo * 1.05);
			}
			else if(_combo >= 0) {
				Score += point;
			}

			if(ComboMax < _combo) {
				Debug.Log("AddCombo " + _combo);
				ComboMax = _combo;
			}

			AudioManager.PlaySE("decision22");

			CalcScoreUpRate();
			_scoreWithoutCombo += point;

			StartCoroutine(FlashComboText());

		}
		else {
			if(!IsTeleport) {

				var lifeDamage = _gameBalance.Damage;

				_life -= lifeDamage;

				if(Time.time - _warningPlayTime > 0.7f) {
					AudioManager.PlaySE("warning2");
					_warningPlayTime = Time.time;
				}

				//ダメージ演出
				var damage = _life < 0 ? lifeDamage + _life : lifeDamage;
				StartCoroutine(FlashHPDown(damage));
				StartCoroutine(CameraObject.DamageFlash());

				Debug.Log("damage");

				if(_life <= 0.0f) {
					UpdateUI();
					GameMaster.Instance.GameOver();
				}
			}
			if(!IsTeleport) {
				_combo = 0;
			}
		}

	}

	private IEnumerator FlashComboText() {

		yield return null;
		var cText = Instantiate(ComboText, ComboText.transform);
		cText.transform.SetParent(ComboText.transform.parent);
		cText.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(1.0f / 3);
		Destroy(cText.gameObject);
	}

	private IEnumerator FlashHPDown(float downHP) {

		var hpDownBar = Instantiate(HPFlash, HPFlash.transform);
		hpDownBar.transform.SetParent(HPFlash.transform.parent);

		var parentT = hpDownBar.transform.parent.GetComponent<RectTransform>();
		var width = parentT.sizeDelta.x * downHP * 0.01f;
		var posX = parentT.sizeDelta.x * (_life) * 0.01f;

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