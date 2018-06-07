using UnityEngine;
using System.Collections;

public class PostEffect : MonoBehaviour
{
    //マテリアル
    public Material wipeCircle;
    //操作キーの表示
    [SerializeField, Range(0, 2)]
    public float gm; //マテリアルのRange値を取得
    public float fadeSpeed; //フェードの速さ

    void Start()
    {
        StartCoroutine(FadeIn());
        
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //シェーダーの値を取得
        wipeCircle.SetFloat("_Radius", gm);
        //マテリアル
        Graphics.Blit(src, dest, wipeCircle);
    }
    //開始時にフェードイン
    void Awake()
    {
       GetComponent<PostEffect>().FadeIn();
    }
    public IEnumerator FadeIn()
    {
        var t = 0.0f;
        while(t < 2.0f)
        {
            t += fadeSpeed * Time.deltaTime;
            gm = t;
            yield return null;
        }
    }
    public IEnumerator FadeOut()
    {
        var t = 2.0f;
        while (t > 0.0f)
        {
            t -= fadeSpeed * Time.deltaTime;
            gm = t;
            yield return null;
        }
    }

}
