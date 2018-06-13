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



    Camera drawCamera;
	Transform maskCube;
	Vector2 startCameraSize = new Vector3(35.5f, 20.0f, 0.1f);
	Vector3 maskCubeScale;

	CameraColorType cameraColorType;
	public CameraColorType CameraColorType {
		get { return cameraColorType; }
		set {
			switch(cameraColorType = value) {
				case CameraColorType.Normal:
					cameraImage.color = Color.white;
					return;
				case CameraColorType.Hit:
					cameraImage.color = Color.cyan;
					return;
				case CameraColorType.Fail:
					cameraImage.color = Color.red;
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

		GameMaster.gameMaster.OnGameStartCountDown
			+= () => StartCoroutine(MaskScaleAnim());
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

		//サイズを決める
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
	}

}
