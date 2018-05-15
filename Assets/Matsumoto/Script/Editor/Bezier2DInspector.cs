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
	static Color normalHandleColor = Color.white;
	static Color activeHandleColor = Color.red;

	static bool isLock = false;


	enum State {
		Move, Add, Remove
	}
	State state = State.Move;

	Bezier2D bezier;
	List<Vector2> points;

	ReorderableList reorderableList;
	bool listUpdateFlg = false;

	bool isClick = false;

	int focusControl = -1;
	int windowID = 1234;

	void OnEnable() {

		bezier = target as Bezier2D;
		points = bezier.points;

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

			EditorGUI.LabelField(rect, index + " " + points[(int)reorderableList.list[index]].ToString());
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

			var poslist = (List<int>)list.list;
			var nPoints = new Vector2[points.Count];

			for(int i = 0;i < poslist.Count;i++) {

				var srcIndex = i * 3;
				var destIndex = poslist[i] - 1;
				Debug.Log(srcIndex + " " + destIndex);
				for(int j = 0;j < 3;j++) {
					nPoints[destIndex + j] = points[srcIndex + j];

				}

			}

			for(int i = 0;i < points.Count;i++) {
				points[i] = nPoints[i];
			}

			poslist = GetBezierPointsIndexOnLine();
			Repaint();
		};
	}

	public override void OnInspectorGUI() {
		reorderableList.DoLayoutList();
	}

	List<int> GetBezierPointsIndexOnLine() {

		var list = new List<int>();

		for(int i = 1;i < points.Count;i++) {
			if((i - 1) % 3 != 0) continue;
			list.Add(i);
		}
		return list;
	}

	void OnSceneGUI() {

		DrawGUI();

		Tools.current = Tool.None;

		switch(state) {
			case State.Move:
				MovePointState(bezier);
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
	}

	void AddPointState(Bezier2D bezier) {

		var e = Event.current;
		var controlID = GUIUtility.GetControlID(FocusType.Passive);

		if(e.GetTypeForControl(controlID) == EventType.MouseDown) {

			if(e.button != 0) return;

			GUIUtility.hotControl = controlID;

			Undo.RecordObject(bezier, "Add BezierPoint");

			bezier.AddPoint(HandleUtility.GUIPointToWorldRay(e.mousePosition).origin);
			focusControl = bezier.points.Count - 2;
			Debug.Log(focusControl);
			isClick = true;

			e.Use();
		}

		if(e.GetTypeForControl(controlID) == EventType.MouseDrag) {

			if(e.button != 0) return;

			GUIUtility.hotControl = controlID;

			var mousePoint = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
			points[points.Count - 3] = mousePoint;
			points[points.Count - 1] = points[points.Count - 2] * 2 - points[points.Count - 3];

			e.Use();

		}

		if(e.GetTypeForControl(controlID) == EventType.MouseUp) {

			if(e.button != 0) return;

			isClick = false;
		}

		if(points.Count > 1)
			DrawTangentLine(points.Count - 2, points);

		if(isClick && points.Count > 4) {
			DrawTangentLine(points.Count - 5, points);
		}
	}

	void MovePointState(Bezier2D bezier) {

		if(points.Count == 0) return;

		Undo.RecordObject(bezier, "Edit BezierLine");

		for(int i = 1;i <= bezier.GetLastBezierPoint();i+=3) {

			Vector2 newPoint = points[i];

			if(i == focusControl) {
				DrawTangentLine(i, points);
				newPoint = HandlePosition(points[i], activeHandleColor);

			}
			else {
				if(HandleButton(points[i], bezierColor)) {
					focusControl = i;
					Repaint();
				}
			}

			if(i + 1 < points.Count) {
				points[i + 1] += newPoint - points[i];
			}
			if(i > 0) {
				points[i - 1] += newPoint - points[i];
			}

			points[i] = newPoint;
		}

		if(focusControl < 0) return;

		var left = focusControl - 1;
		var right = focusControl + 1;

		var hasLeft = left >= 0;
		var hasRight = right < points.Count;

		if(hasLeft) {
			points[left] = HandlePosition(points[left], tangentColor);

			if(Event.current.alt && hasRight) {
				points[right] = points[focusControl] * 2 - points[left];
			}

		}
		if(hasRight) {
			points[right] = HandlePosition(points[right], tangentColor);

			if(Event.current.alt && hasLeft) {
				points[left] = points[focusControl] * 2 - points[right];
			}
		}
	}

	void RemovePointState(Bezier2D bezier) {

		for(int i = 1;i < points.Count;i+=3) {
			if(HandleButton(points[i], bezierColor)) {
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

	void DrawTangentLine(int i, List<Vector2> points) {

		Handles.color = tangentColor;

		if(i > 0)
			Handles.DrawLine(points[i - 1], points[i]);

		if(i + 1 < points.Count)
			Handles.DrawLine(points[i], points[i + 1]);

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
				"MovePoint", "AddPoint", "RemovePoint"
			};

			for(int i = 0;i < 3;i++) {
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
			var points = bezier.points;
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
		var points = bezier.points;
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
