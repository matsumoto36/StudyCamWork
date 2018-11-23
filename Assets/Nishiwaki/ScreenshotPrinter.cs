#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// スクリーンショットを撮るエディタ拡張。
/// メニューの Tools/SaveScreenshot から撮影出来る。
/// または Shift + F2。
/// </summary>
public class ScreenshotPrinter : Editor {

	[MenuItem("Tools/SaveScreenshot #F2")]
	private static void CaptureScreenShot() {

		// ファイルダイアログの表示.
		var filePath = EditorUtility.SaveFilePanel("Save Texture", "", System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png", "png");
		if(filePath.Length <= 0) return;

		// キャプチャを撮る
		ScreenCapture.CaptureScreenshot(filePath); // GameViewにフォーカスがない場合、この時点では撮られない

		// GameViewを取得してくる
		var assembly = typeof(EditorWindow).Assembly;
		var type = assembly.GetType("UnityEditor.GameView");
		var gameView = EditorWindow.GetWindow(type);

		// GameViewを再描画
		gameView.Repaint();
		Debug.Log("SaveScreenShot: " + filePath);
	}
}

#endif