using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScenGo : MonoBehaviour {

    public static string Next_Scene;//シーン名

    void Start () {
		
	}
	
	
	void Update () {
		
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
