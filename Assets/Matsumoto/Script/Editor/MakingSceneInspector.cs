using UnityEditor;
using UnityEngine;
using System.Collections;

/// <summary>
/// ステージ作成のユーティリティを表示するクラス
/// </summary>
[CustomEditor(typeof(MakingScene), true)]
public class MakingSceneInspector : Editor {

	const string _templatePath = "Prefab/Stage/Path/Template";
	SerializedProperty _loadStudioSet;

	void OnEnable() {

		_loadStudioSet = serializedObject.FindProperty("LoadStudioSet");
		

	}

	public override void OnInspectorGUI() {

		serializedObject.Update();

		EditorGUILayout.PropertyField(_loadStudioSet);

		if(GUILayout.Button("Create Stage Template")) {
			CreateTemplete();
		}

		serializedObject.ApplyModifiedProperties();

	}

	/// <summary>
	/// テンプレートの作成
	/// </summary>
	void CreateTemplete() {
		var p = Instantiate(Resources.Load(_templatePath));
		Undo.RegisterCreatedObjectUndo(p, "Create Template");
		p.name = "NewStage";
		Selection.activeGameObject = (GameObject)p;
	}
}
