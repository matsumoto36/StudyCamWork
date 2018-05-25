using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スタートの文字点滅
/// </summary>
public class Flashing : MonoBehaviour {

    //点滅するオブジェクト
    private GameObject _Start;

    //α値の変化速度
    public float _Step = 0.01f;

    void Start()
    {
        //Start_Buttonを探す
        this._Start = GameObject.Find("Start_Button");
    }


    void Update()
    {
        // 現在のAlpha値を取得
        float toColor = this._Start.GetComponent<Image>().color.a;
        // Alphaが0 または 1になったら増減値を反転
        if (toColor < 0 || toColor > 1)
        {
            _Step = _Step * -1f;
        }
        // Alpha値を増減させてセット
        this._Start.GetComponent<Image>().color = new Color(255, 255, 255, toColor + _Step);
    }
}
