#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// スクリーンショットを撮るエディタ拡張。
/// メニューの Tools/SaveScreenshot から撮影出来る。
/// または Shift + F2。
/// </summary>
public class ScreenshotPrinter : Editor {

	[MenuItem("Tools/SaveScreenshot #F2")]
	private static void CaptureScreenshot() {

		// ファイルダイアログの表示.
		string filePath = EditorUtility.SaveFilePanel("Save Texture", "", System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png", "png");
		if(filePath.Length > 0) {

			// キャプチャを撮る
			ScreenCapture.CaptureScreenshot(filePath); // GameViewにフォーカスがない場合、この時点では撮られない
			
			// GameViewを取得してくる
			var assembly = typeof(UnityEditor.EditorWindow).Assembly;
			var type = assembly.GetType("UnityEditor.GameView");
			var gameview = EditorWindow.GetWindow(type);
			// GameViewを再描画
			gameview.Repaint();

			Debug.Log("SaveScreenShot: " + filePath);
		}
	}
}

#endif