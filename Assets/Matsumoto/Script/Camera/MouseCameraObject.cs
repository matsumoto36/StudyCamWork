using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マウスについてくる
/// UI以外のオブジェクトをまとめたクラス
/// </summary>
public class MouseCameraObject : MonoBehaviour {

	Camera drawCamera;
	Transform maskCube;
	Vector2 cameraSize;

	public void Init() {
		drawCamera = GetComponentInChildren<Camera>();
		maskCube = GetComponentInChildren<Renderer>().transform;
	}

	/// <summary>
	/// カメラのImageの範囲にカメラを合わせる
	/// </summary>
	/// <param name="cameraSize">スクリーン上のサイズ</param>
	public void SetCameraSize(Vector2 cameraSize) {

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
		maskCube.localScale = new Vector3(
			(worldSize - startPoint).x,
			(worldSize - startPoint).y,
			maskCube.localScale.z
			);

		//表示サイズを更新
		this.cameraSize = cameraSize;
	}

	/// <summary>
	/// カメラの表示位置を更新
	/// </summary>
	public void UpdateCameraPosition(Vector2 mousePosition) {

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

}
