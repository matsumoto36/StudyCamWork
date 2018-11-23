using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PostEffect : MonoBehaviour {
	//操作キーの表示
	[SerializeField, Range(0, 2)]
	public float Radius; //マテリアルのRange値を取得
	public float FadeSpeed; //フェードの速さ

	private Image _effectImage;

	private void Start() {
		_effectImage = GetComponentInChildren<Image>();
		StartCoroutine(FadeIn());

	}

	private void LateUpdate() {
		_effectImage.material.SetFloat("_Radius", Radius);
	}

	public IEnumerator FadeIn() {
		var t = 0.0f;
		while(t < 2.0f) {
			t += FadeSpeed * Time.deltaTime;
			Radius = t;
			yield return null;
		}
		Radius = 2;
	}
	public IEnumerator FadeOut() {
		var t = 2.0f;
		while(t > 0.0f) {
			t -= FadeSpeed * Time.deltaTime;
			Radius = t;
			yield return null;
		}
		Radius = 0;
	}

}
