using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// ギミック関連のツールクラス
/// </summary>
[CustomEditor(typeof(GimmickManager))]
public class GimmickManagerInspector : Editor {

	private const string PrefabBasePath = "Prefab/Gimmick/";

	private static Rect _windowRect = new Rect(200, 40, 120, 90);


	private readonly string[] _gimmickNames = new[] {
		"SpeedGimmick", "FocusGimmick", "TeleportGimmick"
	};

	private GimmickManager _gimmickManager;
	private int _windowID = 1235;

	private void OnEnable() {
		_gimmickManager = (GimmickManager)target;
	}

	private void OnSceneGUI() {
		DrawWindow();
	}

	/// <summary>
	/// ウィンドウを描画する
	/// </summary>
	private void DrawWindow() {

		Handles.BeginGUI();

		//ウィンドウを描画
		_windowRect = GUILayout.Window(_windowID, _windowRect, (id) => {


			for(var i = 0;i < 3;i++) {
				if (GUILayout.Button("Add " + _gimmickNames[i]))
					AddGimmick(_gimmickNames[i]);
			}

			if(GUILayout.Button("Clear All Gimmicks")) 
				ClearAddGimmicks();

			GUI.DragWindow();

		}, "GimmickTool", GUILayout.Width(100));

		Handles.EndGUI();
	}

	/// <summary>
	/// 指定のギミックを追加する
	/// </summary>
	/// <param name="prefabName">追加したいギミックの名前</param>
	private void AddGimmick(string prefabName) {

		var g = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load(PrefabBasePath + prefabName));
		Undo.RegisterCreatedObjectUndo(g, "Create " + prefabName);
		g.name = prefabName;
		g.transform.parent = _gimmickManager.transform;

	}
		
	/// <summary>
	/// すべてのギミックを消す
	/// </summary>
	private void ClearAddGimmicks() {

		//イテレータが書き換わってしまうので、一旦配列に入れておく
		var list = _gimmickManager.transform
			.OfType<Transform>()
			.ToList();

		foreach (var item in list) {
			Undo.DestroyObjectImmediate(item.gameObject);
		}
	}
}