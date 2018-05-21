using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GimmickBase), true)]
public class GimmickBaseInspector : Editor {

	const float LINE_WIDTH = 10;

	GimmickBase component;

	SerializedProperty lineColor;
	SerializedProperty startPoint;
	SerializedProperty endPoint;

	void OnEnable() {

		component = target as GimmickBase;

		lineColor = serializedObject.FindProperty("gimmickColor");
		startPoint = serializedObject.FindProperty("startPoint");
		endPoint = serializedObject.FindProperty("endPoint");

	}

	public override void OnInspectorGUI() {

		var left = 0.0f;
		var right = 1.0f;

		serializedObject.Update();

		if(component.targetPath) {
			right = component.targetPath.LineCount;
		}

		if(!CheckParentIsManager()) {
			//親オブジェクトがマネージャーでない場合は警告
			EditorGUILayout.HelpBox("GimmickManagerの子オブジェクトにしてください！", MessageType.Error);
		}

		EditorGUILayout.PropertyField(lineColor);
		EditorGUILayout.Slider(startPoint, left, right);
		EditorGUILayout.Slider(endPoint, left, right);

		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspector();
	}

	void OnSceneGUI() {

		if(!component.targetPath) return;

		serializedObject.Update();


		serializedObject.ApplyModifiedProperties();

		if(CheckParentIsManager())
			DrawLine();
	}

	bool CheckParentIsManager() {
		return component.transform.parent &&
			component.GetComponentInParent<GimmickManager>();
	}

	void DrawButton() {

	}

	void DrawLine() {

		if(startPoint.floatValue > endPoint.floatValue) return;

		DrawGizmos(component, GizmoType.Active);

		var path = component.targetPath;
		var lineCount = path.LineCount;
		var sizeRatio = 0.2f;

		Handles.color = component.gimmickColor;

		var start = path.GetPoint(startPoint.floatValue / lineCount);
		var startSize = HandleUtility.GetHandleSize(start) * sizeRatio;
		Handles.SphereHandleCap(0, start, Quaternion.identity, startSize, EventType.Repaint);

		var end = path.GetPoint(endPoint.floatValue / lineCount);
		var endSize = HandleUtility.GetHandleSize(start) * sizeRatio;
		Handles.SphereHandleCap(0, end, Quaternion.identity, endSize, EventType.Repaint);
	}

	[DrawGizmo(GizmoType.NonSelected)]
	static void DrawGizmos(GimmickBase gimmick, GizmoType gizmoType) {

		if(!gimmick.targetPath) return;
		if(gimmick.targetPath.LineCount <= 0) return;

		var startPoint = gimmick.startPoint;
		var endPoint = gimmick.endPoint;

		if(startPoint > endPoint) return;

		var path = gimmick.targetPath;
		var lineCount = gimmick.targetPath.LineCount;
		var pertition = 32 * lineCount;

		var diff = endPoint - startPoint;
		var dt = diff / pertition;
		var points = new List<Vector3>();

		points.Add(path.GetPoint(startPoint / lineCount));
		for(int i = 1;i <= pertition;i++) {

			points.Add(path.GetPoint((startPoint + dt * i) / lineCount));
		}

		Handles.color = gimmick.gimmickColor;
		Handles.DrawAAPolyLine(LINE_WIDTH, points.ToArray());
	}

}
