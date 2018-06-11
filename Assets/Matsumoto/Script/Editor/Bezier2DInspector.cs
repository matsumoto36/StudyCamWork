using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Bezier2D))]
public class Bezier2DInspector : Editor {

	const float HANDLE_SIZE = 0.2f;

	static Rect windowRect = new Rect(40, 40, 120, 120);

	static Color bezierColor = Color.white;
	static Color tangentColor = Color.blue;
	static Color activeHandleColor = Color.red;

	static bool isLock = false;


	enum State {
		Move, Pan, Add, Remove
	}
	State state = State.Move;

	Bezier2D bezier;

	SerializedProperty points;

	ReorderableList reorderableList;
	bool listUpdateFlg = false;

	bool isClick = false;

	int focusControl = -1;
	int windowID = 1234;

	void OnEnable() {

		bezier = target as Bezier2D;
		points = serializedObject.FindProperty("points");

		reorderableList = 
			new ReorderableList(GetBezierPointsIndexOnLine(), typeof(int), true, true, false, true);

		//ヘッダーの描画
		reorderableList.drawHeaderCallback += (rect) => {
			EditorGUI.LabelField(rect, "Points");
		};

		//要素の描画
		reorderableList.drawElementCallback += (rect, index, isActive, isFocused) => {

			if(isFocused && listUpdateFlg) {
				focusControl = index * 3 + 1 ;
				listUpdateFlg = false;
			}

			EditorGUI.LabelField(rect, points.GetArrayElementAtIndex((int)reorderableList.list[index]).vector2Value.ToString());
		};

		//背景の描画
		reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) => {
			if(state == State.Move && focusControl == index * 3 + 1) {
				var tex = new Texture2D(1, 1);
				tex.SetPixel(0, 0, activeHandleColor);
				tex.Apply();
				GUI.DrawTexture(rect, tex);
			}
		};

		//要素を選択したとき
		reorderableList.onSelectCallback += (list) => {
			if(state == State.Move)
				listUpdateFlg = true;
		};

		// - ボタンで削除できるかどうか
		reorderableList.onCanRemoveCallback += (list) => {
			return focusControl - 1 >= 0 && (focusControl - 1) / 3 < list.list.Count;
		};

		// - ボタンが押されたとき
		reorderableList.onRemoveCallback += (list) => {
			RemovePoint(focusControl);
		};

		//入れ替えが発生した場合
		reorderableList.onReorderCallback += (list) => {

			Undo.RecordObject(bezier, "Replacement BezierPoint");
			serializedObject.Update();

			var poslist = (List<int>)list.list;
			var arraySize = points.arraySize;
			var nPoints = new Vector2[arraySize];

			for(int i = 0;i < poslist.Count;i++) {

				var srcIndex = i * 3;
				var destIndex = poslist[i] - 1;
				Debug.Log(srcIndex + " " + destIndex);
				for(int j = 0;j < 3;j++) {
					nPoints[destIndex + j] = points.GetArrayElementAtIndex(srcIndex + j).vector2Value;

				}

			}

			for(int i = 0;i < arraySize;i++) {
				points.GetArrayElementAtIndex(i).vector2Value = nPoints[i];
			}

			poslist = GetBezierPointsIndexOnLine();
			serializedObject.ApplyModifiedProperties();
			Repaint();
		};
	}

	public override void OnInspectorGUI() {
		reorderableList.DoLayoutList();
	}

	List<int> GetBezierPointsIndexOnLine() {

		var list = new List<int>();
		var arraySize = points.arraySize;

		for(int i = 1;i < arraySize;i++) {
			if((i - 1) % 3 != 0) continue;
			list.Add(i);
		}
		return list;
	}

	void OnSceneGUI() {

		serializedObject.Update();

		DrawGUI();
		Tools.current = Tool.None;

		switch(state) {
			case State.Move:
				MovePointState(bezier);
				break;
			case State.Pan:
				PanState(bezier);
				break;
			case State.Add:
				AddPointState(bezier);
				break;
			case State.Remove:
				RemovePointState(bezier);
				break;
			default:
				break;
		}

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

		reorderableList.list = GetBezierPointsIndexOnLine();

		serializedObject.ApplyModifiedProperties();
	}

	void MovePointState(Bezier2D bezier) {

		var arraySize = points.arraySize;

		if(arraySize == 0) return;

		Undo.RecordObject(bezier, "Edit BezierLine");

		for(int i = 1;i <= bezier.GetLastBezierPoint();i+=3) {

			Vector2 newPoint = points.GetArrayElementAtIndex(i).vector2Value;

			if(i == focusControl) {

				DrawTangentLine(
					points.GetArrayElementAtIndex(i + 1).vector2Value,
					points.GetArrayElementAtIndex(i).vector2Value,
					points.GetArrayElementAtIndex(i - 1).vector2Value);

				newPoint = HandlePosition(newPoint, activeHandleColor);

			}
			else {
				if(HandleButton(newPoint, bezierColor)) {
					focusControl = i;
					Repaint();
				}
			}

			var diff = newPoint - points.GetArrayElementAtIndex(i).vector2Value;
			if(i + 1 < arraySize) {
				points.GetArrayElementAtIndex(i + 1).vector2Value += diff;
			}
			if(i > 0) {
				points.GetArrayElementAtIndex(i - 1).vector2Value += diff;
			}

			points.GetArrayElementAtIndex(i).vector2Value = newPoint;
		}

		if(focusControl < 0) return;

		var left = focusControl - 1;
		var right = focusControl + 1;

		var hasLeft = left >= 0;
		var hasRight = right < arraySize;

		if(hasLeft) {
			points.GetArrayElementAtIndex(left).vector2Value 
				= HandlePosition(points.GetArrayElementAtIndex(left).vector2Value, tangentColor);

			if(Event.current.alt && hasRight) {
				points.GetArrayElementAtIndex(right).vector2Value
					= points.GetArrayElementAtIndex(focusControl).vector2Value * 2 - points.GetArrayElementAtIndex(left).vector2Value;
			}

		}
		if(hasRight) {
			points.GetArrayElementAtIndex(right).vector2Value
				= HandlePosition(points.GetArrayElementAtIndex(right).vector2Value, tangentColor);

			if(Event.current.alt && hasLeft) {
				points.GetArrayElementAtIndex(left).vector2Value
					= points.GetArrayElementAtIndex(focusControl).vector2Value * 2 - points.GetArrayElementAtIndex(right).vector2Value;
			}
		}
	}

	void PanState(Bezier2D bezier) {

		var centerPos = new Vector2();
		for(int i = 0;i < bezier.Points.Count;i++) {
			centerPos += points.GetArrayElementAtIndex(i).vector2Value;
		}
		centerPos /= bezier.Points.Count();

		Undo.RecordObject(bezier, "Pan Bezier");

		var newCenterPos = (Vector2)Handles.PositionHandle(centerPos, Quaternion.identity);

		var diff = newCenterPos - centerPos;
		for(int i = 0;i < bezier.Points.Count;i++) {
			points.GetArrayElementAtIndex(i).vector2Value += diff;
		}

	}

	void AddPointState(Bezier2D bezier) {

		var e = Event.current;
		var controlID = GUIUtility.GetControlID(FocusType.Passive);
		var arraySize = points.arraySize;

		if(e.GetTypeForControl(controlID) == EventType.MouseDown) {

			if(e.button != 0) return;

			GUIUtility.hotControl = controlID;

			Undo.RecordObject(bezier, "Add BezierPoint");

			bezier.AddPoint(HandleUtility.GUIPointToWorldRay(e.mousePosition).origin);
			focusControl = bezier.Points.Count - 2;
			isClick = true;

			e.Use();
		}

		if(e.GetTypeForControl(controlID) == EventType.MouseDrag) {

			if(e.button != 0) return;

			GUIUtility.hotControl = controlID;

			var mousePoint = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			points.GetArrayElementAtIndex(arraySize - 3).vector2Value = mousePoint;
			points.GetArrayElementAtIndex(arraySize - 1).vector2Value = points.GetArrayElementAtIndex(arraySize - 2).vector2Value * 2 - mousePoint;

			e.Use();

		}

		if(e.GetTypeForControl(controlID) == EventType.MouseUp) {

			if(e.button != 0) return;

			isClick = false;
		}

		if(arraySize > 1)
			DrawTangentLine(
				points.GetArrayElementAtIndex(arraySize - 3).vector2Value,
				points.GetArrayElementAtIndex(arraySize - 2).vector2Value,
				points.GetArrayElementAtIndex(arraySize - 1).vector2Value);


		if(isClick && arraySize > 4) {
			DrawTangentLine(
				points.GetArrayElementAtIndex(arraySize - 6).vector2Value,
				points.GetArrayElementAtIndex(arraySize - 5).vector2Value,
				points.GetArrayElementAtIndex(arraySize - 4).vector2Value);
		}
	}

	void RemovePointState(Bezier2D bezier) {

		var arraySize = points.arraySize;

		for(int i = 1;i < arraySize;i+=3) {
			if(HandleButton(points.GetArrayElementAtIndex(i).vector2Value, bezierColor)) {
				RemovePoint(i);
			}
		}
	}

	void RemovePoint(int index) {
		Undo.RecordObject(bezier, "Remove BezierPoint");
		bezier.RemovePoint(index);
		focusControl = -1;
		Repaint();
	}

	void DrawTangentLine(Vector2 p0, Vector2 p1, Vector2 p2) {

		Handles.color = tangentColor;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p1, p2);
	}

	Vector2 HandlePosition(Vector2 center, Color col) {

		Handles.color = col;
		return Handles.Slider2D(center, Vector3.forward, Vector3.right, Vector3.up, HandleUtility.GetHandleSize(center) * HANDLE_SIZE, Handles.CylinderHandleCap, 1f, false);
	}

	bool HandleButton(Vector2 center, Color col) {

		Handles.color = col;
		var size = HandleUtility.GetHandleSize(center) * HANDLE_SIZE;
		return Handles.Button(center, Quaternion.identity, size, size, Handles.SphereHandleCap);
	}

	void DrawGUI() {

		Handles.BeginGUI();

		windowRect = GUILayout.Window(windowID, windowRect, (id) => {

			var buttonName = new string[] {
				"MovePoint", "Pan", "AddPoint", "RemovePoint"
			};

			for(int i = 0;i < 4;i++) {
				EditorGUI.BeginDisabledGroup(i == (int)state);

				var isClick = GUILayout.Button(buttonName[i]);
				if(isClick) {
					state = (State)i;
					Debug.Log("State is " + state);
					Repaint();
				}

				EditorGUI.EndDisabledGroup();
			}

			var bezier = target as Bezier2D;
			var points = bezier.Points;
			EditorGUI.BeginDisabledGroup(points.Count <= 0);
			if(GUILayout.Button("Clear")) {
				Undo.RecordObject(bezier, "Clear BezierLine");
				points.Clear();
			}
			EditorGUI.EndDisabledGroup();

			isLock = GUILayout.Toggle(isLock, "Lock");

			GUI.DragWindow();

		}, "BezierEditTool", GUILayout.Width(100));

		Handles.EndGUI();
	}


	bool GetKey(KeyCode key, EventType type) {
		var e = Event.current;
		var controlID = GUIUtility.GetControlID(FocusType.Keyboard);

		if(e.GetTypeForControl(controlID) == type) {

			return e.keyCode == key;
		}

		return false;
	}

	[DrawGizmo(GizmoType.NonSelected | GizmoType.Active)]
	static void DrawGizmos(Bezier2D bezier, GizmoType gizmoType) {

		var pertition = 32;
		var points = bezier.Points;
		Gizmos.color = bezierColor;

		for(int i = 4;i < points.Count;i += 3) {

			var prevPoint = Bezier2D.GetPoint(points[i - 3], points[i - 2], points[i - 1], points[i], 0);
			for(int j = 1;j <= pertition;j++) {
				var point = Bezier2D.GetPoint(points[i - 3], points[i - 2], points[i - 1], points[i], j / (float)pertition);
				Gizmos.DrawLine(prevPoint, point);
				prevPoint = point;
			}
		}
	}
}
