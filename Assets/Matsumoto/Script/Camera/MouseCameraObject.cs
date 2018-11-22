using System.Collections;
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

	private static readonly Vector2 StartCameraSize = new Vector3(35.5f, 20.0f, 0.1f);

	public RectTransform CameraUi;
	public RectTransform StartMessage;
	public Image CameraImage;
	public Image SmallCameraImage;
	public Image FailImage;
	public Image DamageFlashImage;
	public Image RectImage;

	private Camera _mainCamera;
	private Camera _drawCamera;
	private Transform _maskCube;
	private Vector3 _maskCubeScale;

	private CameraColorType _cameraColorType;
	public CameraColorType CameraColorType {
		get { return _cameraColorType; }
		set {
			switch(_cameraColorType = value) {
				case CameraColorType.Normal:
					FailImage.color = new Color(1, 0, 0, 0);
					return;
				case CameraColorType.Hit:
					FailImage.color = new Color(1, 0, 0, 0);
					return;
				case CameraColorType.Fail:
					FailImage.color = new Color(1, 0, 0, 0.1f);
					return;
				default: return;
			}

		}
	}

	public void Init() {

		_mainCamera = Camera.main;
		_drawCamera = GetComponentInChildren<Camera>();

		_maskCube = GetComponentInChildren<Renderer>().transform;
		_maskCube.localScale = StartCameraSize;
		_maskCube.SetParent(null);

		GameMaster.Instance.OnGameStartCountDown
			+= () => StartCoroutine(MaskScaleAnim());

		GameMaster.Instance.OnGameStart
			+= () => RectImage.color = new Color(1, 1, 1, 0.5f);

		GameMaster.Instance.OnGameOver
			+= () => RectImage.color = new Color(1, 1, 1, 0);

		GameMaster.Instance.OnGameClear
			+= () => RectImage.color = new Color(1, 1, 1, 0);

		DamageFlashImage.color = new Color(1, 0, 0, 0);
		RectImage.color = new Color(1, 1, 1, 0);
	}

	/// <summary>
	/// カメラのImageの範囲にカメラを合わせる
	/// </summary>
	/// <param name="cameraSize">スクリーン上のサイズ</param>
	public void SetCameraSize(Vector2 cameraSize) {

		//Imageのサイズを合わせる
		CameraUi.sizeDelta = cameraSize;

		//カメラの表示サイズに合わせる
		_drawCamera.rect = new Rect(
			0, 0,
			cameraSize.x / Screen.width,
			cameraSize.y / Screen.height
		);

		//内部の範囲を決める
		SmallCameraImage.rectTransform.sizeDelta =
			cameraSize * GameMaster.Instance.GameBalanceData.CameraSmallSizeRatio * 2;

		//カメラのサイズを決める
		_drawCamera.orthographicSize =
			_mainCamera.orthographicSize * _drawCamera.rect.height;

		var startPoint = _mainCamera.ScreenToWorldPoint(new Vector3());
		var worldSize = _mainCamera.ScreenToWorldPoint(cameraSize);

		//マスク範囲を計算
		_maskCubeScale = new Vector3(
			(worldSize - startPoint).x,
			(worldSize - startPoint).y,
			_maskCube.localScale.z
			);
	}

	/// <summary>
	/// カメラの表示位置を更新
	/// </summary>
	public void UpdateCameraPosition(Vector2 mousePosition) {

		//範囲内に収める
		mousePosition.x =
			Mathf.Clamp(mousePosition.x, CameraUi.sizeDelta.x / 2, Screen.width - CameraUi.sizeDelta.x / 2);

		mousePosition.y =
			Mathf.Clamp(mousePosition.y, CameraUi.sizeDelta.y / 2, Screen.height - CameraUi.sizeDelta.y / 2);

		//スクリーン位置を更新
		CameraUi.position = mousePosition;

		//ワールド位置の更新
		var pos = _mainCamera.ScreenToWorldPoint(mousePosition);
		pos.z = transform.position.z;
		transform.position = pos;

		//ディスプレイ上の表示位置を更新
		var cameraPosition = new Vector2(
			mousePosition.x / Screen.width,
			mousePosition.y / Screen.height
		);

		//中心にずらす
		cameraPosition -= _drawCamera.rect.size / 2;

		_drawCamera.rect = new Rect(
			cameraPosition,
			_drawCamera.rect.size
		);
	}

	/// <summary>
	/// カメラの位置をスクリーン座標系で取得
	/// </summary>
	/// <returns></returns>
	public Vector2 GetObjectPosition() {
		return CameraUi.position;
	}

	public IEnumerator DamageFlash() {

		const float from = 0.0f;
		const float to = 0.5f;
		const float speed = 3.0f;

		var t = 0.0f;
		while((t += Time.deltaTime * speed) < 1.0f) {

			var alpha = Mathf.Lerp(from, to, Mathf.Lerp(from, to, 1 - t));
			var col = new Color(1, 0, 0, alpha);

			DamageFlashImage.color = col;
			yield return null;
		}

		DamageFlashImage.color = new Color(1, 0, 0, 0);

	}

	/// <summary>
	/// ゲームのスタート時にマスクをスケールする
	/// </summary>
	/// <returns></returns>
	private IEnumerator MaskScaleAnim() {

		StartMessage.gameObject.SetActive(false);
		_maskCube.SetParent(transform);

		var startPosition = _maskCube.localPosition;
		var endPosition = new Vector3();

		var t = 0.0f;
		while((t += Time.deltaTime / 3) < 1.0f) {

			_maskCube.localPosition =
				Vector3.Lerp(startPosition, endPosition, t);

			_maskCube.localScale =
				Vector3.Lerp(StartCameraSize, _maskCubeScale, t);

			yield return null;
		}

		_maskCube.localPosition = endPosition;
		_maskCube.localScale = _maskCubeScale;

		_mainCamera.cullingMask ^= LayerMask.GetMask("Line");
	}

}
