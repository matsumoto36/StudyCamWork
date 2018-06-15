using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GimmickBase), true)]
public class GimmickBaseInspector : Editor {

	const float LINE_WIDTH = 10;
	const float HANDLE_SIZE = 0.2f;

	static Rect windowRect = new Rect(40, 40, 120, 120);

	int windowID = 1234;

	enum State {
		Preview, PointS, PointE
	}
	State state = State.Preview;

	bool isLock = false;

	GimmickBase component;
	GimmickBase[] gimmicks;

	Bezier2D path;

	SerializedProperty lineColor;
	SerializedProperty startPoint;
	SerializedProperty endPoint;

	void OnEnable() {

		component = target as GimmickBase;

		lineColor = serializedObject.FindProperty("gimmickColor");
		startPoint = serializedObject.FindProperty("startPoint");
		endPoint = serializedObject.FindProperty("endPoint");

		if(CheckParentIsManager()) {
			path = component.GetComponentInParent<GimmickManager>().Path;
		}
	}

	public override void OnInspectorGUI() {

		var left = 0.0f;
		var right = 1.0f;

		if(path) {
			right = path.LineCount;
		}

		serializedObject.Update();

		EditorGUILayout.PropertyField(lineColor);
		EditorGUILayout.Slider(startPoint, left, right);
		EditorGUILayout.Slider(endPoint, left, right);

		if(!CheckParentIsManager()) {
			//親オブジェクトがマネージャーでない場合は警告
			EditorGUILayout.HelpBox("GimmickManagerの子オブジェクトにしてください！", MessageType.Error);

		}
		else if(CheckCollisionLine()) {
			//線がほかの線と被っている場合は警告
			EditorGUILayout.HelpBox("線が被っています！", MessageType.Error);

		}

		serializedObject.ApplyModifiedProperties();
		DrawDefaultInspector();
	}

	void OnSceneGUI() {

		if(!path) return;

		serializedObject.Update();

		if(CheckParentIsManager()) {

			DrawLine();

			switch(state) {
				case State.Preview:
					DrawPoint();
					break;
				case State.PointS:
					DrawButton((index) => {
						var point = (index - 1) / 3;
						if(endPoint.floatValue >= point)
							startPoint.floatValue = point;
					});
					break;
				case State.PointE:
					DrawButton((index) => {
						var point = (index - 1) / 3;
						if(startPoint.floatValue <= point)
							endPoint.floatValue = point;
					});
					break;
				default:
					break;
			}
		}

		serializedObject.ApplyModifiedProperties();

		//使わないクリック処理でLock状態を実現
		if(isLock) {
			var e = Event.current;
			var controlID = GUIUtility.GetControlID(FocusType.Passive);

			if(e.GetTypeForControl(controlID) == EventType.MouseDown) {
				if(e.button != 0) return;
				GUIUtility.hotControl = controlID;
				e.Use();
			}
		}

		DrawGUI();
	}

	bool CheckParentIsManager() {
		return component.transform.parent &&
			component.GetComponentInParent<GimmickManager>();
	}

	bool CheckCollisionLine() {
		var manager = component.GetComponentInParent<GimmickManager>();

		if(gimmicks == null) {
			gimmicks = manager.GetComponentsInChildren<GimmickBase>();
		}

		foreach(var item in gimmicks) {
			if(item == component) continue;
			if(item.endPoint > component.startPoint &&
				item.startPoint < component.endPoint) return true;
		}
		return false;
	}

	void DrawButton(Action<int> buttonAction) {

		if(!path) return;

		var points = path.Points;

		for(int i = 1;i < points.Count;i+=3) {
			var size = HandleUtility.GetHandleSize(points[i]) * HANDLE_SIZE;
			if(Handles.Button(points[i], Quaternion.identity, size, size, Handles.SphereHandleCap)) {
				buttonAction(i);
			}
		}

	}

	void DrawLine() {

		if(startPoint.floatValue > endPoint.floatValue) return;

		DrawGizmos(component, GizmoType.Active);
	}

	void DrawPoint() {

		if(!path) return;

		var lineCount = path.LineCount;

		Handles.color = component.gimmickColor;

		var start = path.GetPoint(startPoint.floatValue / lineCount);
		var startSize = HandleUtility.GetHandleSize(start) * HANDLE_SIZE;
		Handles.SphereHandleCap(0, start, Quaternion.identity, startSize, EventType.Repaint);

		var end = path.GetPoint(endPoint.floatValue / lineCount);
		var endSize = HandleUtility.GetHandleSize(end) * HANDLE_SIZE;
		Handles.SphereHandleCap(0, end, Quaternion.identity, endSize, EventType.Repaint);
	}

	void DrawGUI() {

		Handles.BeginGUI();

		windowRect = GUILayout.Window(windowID, windowRect, (id) => {

			var buttonName = new string[] {
				"Preview", "SetStartPoint", "SetEndPoint"
			};

			for(int i = 0;i < 3;i++) {
				EditorGUI.BeginDisabledGroup(i == (int)state);

				var isClick = GUILayout.Button(buttonName[i]);
				if(isClick) {
					state = (State)i;
					Repaint();
				}

				EditorGUI.EndDisabledGroup();
			}

			isLock = GUILayout.Toggle(isLock, "Lock");

			GUI.DragWindow();

		}, "GimmickTool", GUILayout.Width(100));

		Handles.EndGUI();
	}

	[DrawGizmo(GizmoType.NonSelected)]
	static void DrawGizmos(GimmickBase gimmick, GizmoType gizmoType) {

		var manager = gimmick.GetComponentInParent<GimmickManager>();

		if(!manager) return;

		var path = manager.Path;

		if(!path) return;
		if(path.LineCount <= 0) return;

		var startPoint = gimmick.startPoint;
		var endPoint = gimmick.endPoint;

		if(startPoint > endPoint) return;

		//線を薄く表示
		var lineCount = path.LineCount;
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
