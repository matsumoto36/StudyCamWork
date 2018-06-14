using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerController : MonoBehaviour
{
    public static string Next_Scene;//シーン名

    PostEffect effect;

    void Start()
    {
        effect = GetComponent<PostEffect>();

        DontDestroyOnLoad(gameObject);
    }


    void Update()
    {

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

    //フェードアウト→シーン移動→フェードイン
    IEnumerator SM(string NextSceneName)
    {
 
        yield return StartCoroutine(effect.FadeOut());
        
        Next_Scene = NextSceneName;
        SceneManager.LoadScene(Next_Scene);

        yield return new WaitForSeconds(1);

        yield return StartCoroutine(effect.FadeIn());
    }
    public void SceneMove(string SceneName)
    {
        StartCoroutine(SM(SceneName));
    }
}