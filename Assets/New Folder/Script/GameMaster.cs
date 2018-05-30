using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

	public static GameMaster gameMaster;

	public Text countDownText;
    public Text ComboText;
    public Text ScoreText;
   
    public MouseCamera mouseCamera;

	public event Action OnGameStart;
	public event Action OnGameEnd;

	bool isGameStart;
    Slider _slider;


    // Use this for initialization
    void Awake () {
		gameMaster = this;
		countDownText.text = "";
	}

	void Start() {

		StartCoroutine(CountDown());
        _slider = GameObject.Find("Slider").GetComponent<Slider>();


    }

    // Update is called once per frame
    void Update () {
       _slider.value =mouseCamera.life ;
        ScoreText.text = mouseCamera.Score.ToString("");
        ComboText.text = mouseCamera.Combo.ToString("");
    }
    
	public void GameClear() {
		//OnGameEnd?.Invoke();
		isGameStart = false;
	}

	IEnumerator CountDown() {

		yield return new WaitForSeconds(1);
		countDownText.text = "3";
		yield return new WaitForSeconds(1);
		countDownText.text = "2";
		yield return new WaitForSeconds(1);
		countDownText.text = "1";
		yield return new WaitForSeconds(1);
		countDownText.text = "Queue";
		yield return new WaitForSeconds(1);


		countDownText.text = "";
		if(OnGameStart != null)
        {

            OnGameStart();
            //Debug.Log("a");
        }
        isGameStart = true;
	}
}
