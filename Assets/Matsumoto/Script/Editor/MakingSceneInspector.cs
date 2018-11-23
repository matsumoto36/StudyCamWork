using UnityEditor;
using UnityEngine;
using System.Collections;

/// <summary>
/// ステージ作成のユーティリティを表示するクラス
/// </summary>
[CustomEditor(typeof(MakingScene), true)]
public class MakingSceneInspector : Editor {

	private const string TemplatePath = "Prefab/Stage/Path/Template";
	private SerializedProperty _loadStudioSet;

	private void OnEnable() {

		_loadStudioSet = serializedObject.FindProperty("LoadStudioSet");
		

	}

	public override void OnInspectorGUI() {

		serializedObject.Update();

		EditorGUILayout.PropertyField(_loadStudioSet);

		if(GUILayout.Button("Create Stage Template")) {
			CreateTemplate();
		}

		serializedObject.ApplyModifiedProperties();

	}

	/// <summary>
	/// テンプレートの作成
	/// </summary>
	private static void CreateTemplate() {
		var p = Instantiate(Resources.Load(TemplatePath));
		Undo.RegisterCreatedObjectUndo(p, "Create Template");
		p.name = "NewStage";
		Selection.activeGameObject = (GameObject)p;
	}
}
