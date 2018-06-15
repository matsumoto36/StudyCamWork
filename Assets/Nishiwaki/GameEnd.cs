using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
    public Image ResultBack;

    // Use this for initialization
    void Start () {
		//ResultBack = GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update () {
        //GameClear = ゲームクリアの判定
        //if(GameClear == true && Input.GetMouseButtonDown(0))
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(Result());
        }
    }
    IEnumerator Result()
    {
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 0.8f);
        float time=0;

        Debug.Log("コルーチン");
        while (time<1.0f)
        {
            time += Time.deltaTime;
            ResultBack.color = Color.Lerp(startColor, endColor, time);
            yield return null;
        }
        //yield return new WaitForSeconds(1);
    }
}