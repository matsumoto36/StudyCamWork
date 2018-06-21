using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PostEffect : MonoBehaviour
{
    //マテリアル
    //public Material wipeCircle;
    //操作キーの表示
    [SerializeField, Range(0, 2)]
    public float gm; //マテリアルのRange値を取得
    public float fadeSpeed; //フェードの速さ

	Image effectImage;

    void Start()
    {
		effectImage = GetComponentInChildren<Image>();

		StartCoroutine(FadeIn());
        
    }
    //void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    //シェーダーの値を取得
    //    wipeCircle.SetFloat("_Radius", gm);
    //    //マテリアル
    //    Graphics.Blit(src, dest, wipeCircle);
    //}

	void LateUpdate() {
		effectImage.material.SetFloat("_Radius", gm);
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
        gm = 2;
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
        gm = 0;
    }

}
