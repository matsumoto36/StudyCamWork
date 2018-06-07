using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 当たったらクリア文字を表示（いらない子になる多分）
/// </summary>

public class GameClear : MonoBehaviour {

    public GameObject Clear_Text;//クリア表示
	

	void Start ()
    {
		
	}
	
	
	void Update ()
    {
		
	}

    /*
    //トリガー
    void OnTriggerEnter(Collider2D other)
    {
        //Goalのタグに触れたら
        if (other.gameObject.tag == "Goal")
        {
            Clear_Text.SetActive(true);//Clear_Texを表示
        }
    }*/
}
