using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Unityの操作を追加する拡張クラス
/// </summary>
public static class UnityEditorExtension {

	/// <summary>
	/// 主にプレハブのフィールドを更新するために使う
	/// </summary>
	[MenuItem("Assets/Force Update Prefab")]
	private static void ForceUpdatePrefab() {

		var prefabs = Selection.objects
			//パスを取得
			.Select(item =>
				new KeyValuePair<GameObject, string>((GameObject)item, AssetDatabase.GetAssetPath(item.GetInstanceID()))
			)
			//プレハブのみを抽出
			.Where(item => {
				var pair = item.Value.Split('.');
				if (pair.Length < 2) return false;
				return pair[1] == "prefab";
			})
			.ToList();

		foreach (var pair in prefabs) {
			//変更を通知することで更新させる
			EditorUtility.SetDirty(pair.Key);
			Debug.Log(pair.Value + " を更新しました。");
		}

		AssetDatabase.SaveAssets();
		Debug.Log("合計 " + prefabs.Count + " 個のプレハブの更新を行いました。");
	}

}
