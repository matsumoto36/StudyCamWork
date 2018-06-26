using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CameraColorType {
	Normal,
	Hit,
	Fail
}

/// <summary>
/// マウスについてくる
/// UI以外のオブジェクトをまとめたクラス
/// </summary>
public class MouseCameraObject : MonoBehaviour {

	public RectTransform cameraUI;
	public RectTransform startMessage;
	public Image cameraImage;
    public Image SmalCameraImage;
    public Image failImage;
    public Image damageFlashImage;
	public RawImage cameraRenderImage;
	public RawImage debugRenderImage;
	public RenderTexture renderTexture;
	Camera drawCamera;
	Transform maskCube;
	Vector2 startCameraSize = new Vector3(35.5f, 20.0f, 0.1f);
	Vector3 maskCubeScale;

	List<RenderTexture> recordData;

	CameraColorType cameraColorType;
	public CameraColorType CameraColorType {
		get { return cameraColorType; }
		set {
			switch(cameraColorType = value) {
				case CameraColorType.Normal:
					failImage.color = new Color(1, 0, 0, 0);
					return;
				case CameraColorType.Hit:
					failImage.color = new Color(1, 0, 0, 0);
					return;
				case CameraColorType.Fail:
					failImage.color = new Color(1, 0, 0, 0.1f);
					return;
				default: return;
			}

		}
	}

	public void Init() {
		drawCamera = GetComponentInChildren<Camera>();

		maskCube = GetComponentInChildren<Renderer>().transform;
		maskCube.localScale = startCameraSize;
		maskCube.SetParent(null);

		GameMaster.Instance.OnGameStartCountDown
			+= () => StartCoroutine(MaskScaleAnim());
        
		recordData = new List<RenderTexture>();

		damageFlashImage.color = new Color(1, 0, 0, 0);
	}

	/// <summary>
	/// カメラのImageの範囲にカメラを合わせる
	/// </summary>
	/// <param name="cameraSize">スクリーン上のサイズ</param>
	public void SetCameraSize(Vector2 cameraSize) {

		//Imageのサイズを合わせる
		cameraUI.sizeDelta = cameraSize;

		//カメラの表示サイズに合わせる
		drawCamera.rect = new Rect(
			0, 0,
			cameraSize.x / Screen.width,
			cameraSize.y / Screen.height
		);
        float CameraSize = GameMaster.Instance.GameBalanceData.CameraSmallSizeRatio;
        SmalCameraImage.rectTransform.sizeDelta = cameraSize * CameraSize*2;
		//カメラのサイズを決める
		drawCamera.orthographicSize =
			Camera.main.orthographicSize * drawCamera.rect.height;

		var startPoint = Camera.main.ScreenToWorldPoint(new Vector3());
		var worldSize = Camera.main.ScreenToWorldPoint(cameraSize);

		//マスク範囲を計算
		maskCubeScale = new Vector3(
			(worldSize - startPoint).x,
			(worldSize - startPoint).y,
			maskCube.localScale.z
			);
	}

	/// <summary>
	/// カメラの表示位置を更新
	/// </summary>
	public void UpdateCameraPosition(Vector2 mousePosition) {

		//スクリーン位置を更新
		cameraUI.position = mousePosition;

		//ワールド位置の更新
		var pos = Camera.main.ScreenToWorldPoint(mousePosition);
		pos.z = transform.position.z;
		transform.position = pos;

		//ディスプレイ上の表示位置を更新
		var cameraPosition = new Vector2(
			mousePosition.x / Screen.width,
			mousePosition.y / Screen.height
		);

		//中心にずらす
		cameraPosition -= drawCamera.rect.size / 2;

		drawCamera.rect = new Rect(
			cameraPosition,
			drawCamera.rect.size
		);

		//if(GameMaster.Instance.State == GameState.Playing) RecordFrame();
	}

	/// <summary>
	/// 画像で録画する
	/// </summary>
	IEnumerator RecordLoop() {

		while(true) {

			yield return new WaitForEndOfFrame();

			if(GameMaster.Instance.State != GameState.Playing) continue;

			var currentRenderTexture = RenderTexture.active;
			var renderTexture = drawCamera.targetTexture;

			//アクティブを一時的に変更
			RenderTexture.active = drawCamera.targetTexture;
			drawCamera.Render();

			//テクスチャを作成
			var texture = new Texture2D(
				renderTexture.width,
				renderTexture.height,
				TextureFormat.ARGB32,
				false
			);
			//var texture = (Texture)renderTexture;

			//ピクセルを読む 激重
			texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			texture.Apply();

			//yield return null;

			//アクティブを元に戻す
			RenderTexture.active = currentRenderTexture;

			//debug
			debugRenderImage.texture = texture;

			//記録
			//recordData.Add((Texture2D)texture);

		}

	}

	void RecordFrame() {

		var currentRenderTexture = RenderTexture.active;
		//var renderTexture = drawCamera.targetTexture;

		//アクティブを一時的に変更
		RenderTexture.active = drawCamera.targetTexture = new RenderTexture(renderTexture);
		drawCamera.Render();

		//アクティブを元に戻す
		RenderTexture.active = currentRenderTexture;

		//登録
		recordData.Add(drawCamera.targetTexture);

		//再生成
		drawCamera.targetTexture = null;
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.P)) {
			StartCoroutine(PlayMovie());
		}
	}

	public IEnumerator DamageFlash() {

		var from = 0.0f;
		var to = 0.5f;
		var speed = 3.0f;

		var t = 0.0f;
		while((t += Time.deltaTime * speed) < 1.0f) {

			var col = new Color(
				1, 0, 0,
				Mathf.Lerp(from, to, Mathf.Lerp(from, to, 1 - t))
				);

			damageFlashImage.color = col;
			yield return null;
		}

		damageFlashImage.color = new Color(1, 0, 0, 0);

	}

	IEnumerator PlayMovie() {

		for(int i = 0;i < recordData.Count;i++) {
			debugRenderImage.texture = recordData[i];
			yield return null;
		}

	}

	/// <summary>
	/// ゲームのスタート時にマスクをスケールする
	/// </summary>
	/// <returns></returns>
	IEnumerator MaskScaleAnim() {

		startMessage.gameObject.SetActive(false);
		maskCube.SetParent(transform);

		var startPosition = maskCube.localPosition;
		var endPosition = new Vector3();

		float t = 0.0f;
		while(t < 1.0f) {
			t += Time.deltaTime / 3;

			maskCube.localPosition =
				Vector3.Lerp(startPosition, endPosition, t);

			maskCube.localScale =
				Vector3.Lerp(startCameraSize, maskCubeScale, t);

			yield return null;
		}

		maskCube.localPosition = endPosition;
		maskCube.localScale = maskCubeScale;

		Camera.main.cullingMask ^= LayerMask.GetMask("Line");
	}

}
