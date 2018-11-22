using UnityEngine;

/// <summary>
/// CRTのようなエフェクトをシェーダーで表現する
/// </summary>
public class CameraEffect : MonoBehaviour {

	public Material Mat;
	public float Fade;
	public float Light;
	public float Wave;
	public float AnimSpeed;

	private void OnRenderImage(RenderTexture src, RenderTexture dest) {

		Mat.SetFloat("_Fade", Fade);
		Mat.SetFloat("_Light", Light);
		Mat.SetFloat("_Wave", Wave);
		Mat.SetFloat("_AnimSpeed", AnimSpeed);

		Graphics.Blit(src, dest, Mat);
	}
}
