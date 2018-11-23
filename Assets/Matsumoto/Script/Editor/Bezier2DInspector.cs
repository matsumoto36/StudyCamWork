﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

/// <summary>
/// パスを作るためのツールクラス
/// </summary>
[CustomEditor(typeof(Bezier2D))]
public class Bezier2DInspector : Editor {

	private const float HandleSize = 0.2f;
	private const int WindowID = 1234;

	private static readonly Color BezierColor = Color.white;
	private static readonly Color TangentColor = Color.blue;
	private static readonly Color ActiveHandleColor = Color.red;

	private static Rect _windowRect = new Rect(40, 40, 120, 120);

	//アクティブが変更されない設定
	private static bool _isLock;

	private enum State {
		Move, Pan, Add, Remove
	}
	private State _state = State.Move;

	private Bezier2D _bezier;
	private SerializedProperty _points;
	private ReorderableList _reorderableList;

	private bool _listUpdateFlg;
	private bool _isClick;
	private int _focusControl = -1;

	private void OnEnable() {

		_bezier = target as Bezier2D;
		_points = serializedObject.FindProperty("points");

		_reorderableList =
			new ReorderableList(GetBezierPointsIndexOnLine(), typeof(int), true, true, false, true);

		//ヘッダーの描画
		_reorderableList.drawHeaderCallback += rect => {
			EditorGUI.LabelField(rect, "Anchor Points");
		};

		//要素の描画
		_reorderableList.drawElementCallback += (rect, index, isActive, isFocused) => {

			if(isFocused && _listUpdateFlg) {
				_focusControl = index * 3 + 1;
				_listUpdateFlg = false;
			}

			var id = (int)_reorderableList.list[index];
			EditorGUI.LabelField(rect, _points.GetArrayElementAtIndex(id).vector2Value.ToString());
		};

		//背景の描画
		_reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) => {
			if(_state != State.Move || _focusControl != index * 3 + 1) return;
			var tex = new Texture2D(1, 1);
			tex.SetPixel(0, 0, ActiveHandleColor);
			tex.Apply();
			GUI.DrawTexture(rect, tex);
		};

		//要素を選択したとき
		_reorderableList.onSelectCallback += list => {
			if(_state == State.Move)
				_listUpdateFlg = true;
		};

		// - ボタンで削除できるかどうか
		_reorderableList.onCanRemoveCallback += list =>
			_focusControl - 1 >= 0 && (_focusControl - 1) / 3 < list.list.Count;

		// - ボタンが押されたとき
		_reorderableList.onRemoveCallback += list => {
			RemoveAnchorPoint(_focusControl);
		};

		//入れ替えが発生した場合
		_reorderableList.onReorderCallback += list => {

			Undo.RecordObject(_bezier, "Replacement BezierPoint");
			serializedObject.Update();

			var posList = (List<int>)list.list;
			var arraySize = _points.arraySize;
			var nPoints = new Vector2[arraySize];

			for(var i = 0;i < posList.Count;i++) {

				var srcIndex = i * 3;
				var destIndex = posList[i] - 1;
				Debug.Log(srcIndex + " " + destIndex);
				for(var j = 0;j < 3;j++) {
					nPoints[destIndex + j] = _points.GetArrayElementAtIndex(srcIndex + j).vector2Value;
				}
			}

			for(var i = 0;i < arraySize;i++) {
				_points.GetArrayElementAtIndex(i).vector2Value = nPoints[i];
			}

			GetBezierPointsIndexOnLine();
			serializedObject.ApplyModifiedProperties();
			Repaint();
		};
	}

	public override void OnInspectorGUI() {
		//点のリストを表示
		_reorderableList.DoLayoutList();
	}

	/// <summary>
	/// ベジェ曲線のリストのうち、アンカーポイントのインデックスを抽出する
	/// </summary>
	/// <returns>アンカーポイントのインデックス</returns>
	private List<int> GetBezierPointsIndexOnLine() {

		var list = new List<int>();
		var arraySize = _points.arraySize;

		for(var i = 1;i < arraySize;i++) {

			//ハンドル : H
			//アンカー : A とすると、
			//HAHHAHHAHHAとなる

			if((i - 1) % 3 != 0) continue;
			list.Add(i);
		}
		return list;
	}

	private void OnSceneGUI() {

		serializedObject.Update();

		DrawWindow();
		Tools.current = Tool.None;

		switch(_state) {
			case State.Move: MovePointState(); break;
			case State.Pan: PanState(); break;
			case State.Add: AddPointState(); break;
			case State.Remove: RemovePointState(); break;
			default: break;
		}

		//使わないクリック処理を追加することで
		//フォーカスが変更されない状態を実現
		if(_isLock) {
			var e = Event.current;
			var controlID = GUIUtility.GetControlID(FocusType.Passive);

			if(e.GetTypeForControl(controlID) == EventType.MouseDown) {
				if(e.button != 0) return;
				GUIUtility.hotControl = controlID;
				e.Use();
			}
		}

		_reorderableList.list = GetBezierPointsIndexOnLine();

		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// Moveモード時の動作
	/// </summary>
	private void MovePointState() {

		var arraySize = _points.arraySize;
		if(arraySize == 0) return;

		Undo.RecordObject(_bezier, "Edit BezierLine");

		for(var i = 1;i <= _bezier.GetLastBezierPoint();i += 3) {

			var newPoint = _points.GetArrayElementAtIndex(i).vector2Value;

			if(i == _focusControl) {

				DrawTangentLine(
					_points.GetArrayElementAtIndex(i + 1).vector2Value,
					_points.GetArrayElementAtIndex(i).vector2Value,
					_points.GetArrayElementAtIndex(i - 1).vector2Value);

				newPoint = HandlePosition(newPoint, ActiveHandleColor);

			}
			else {
				if(HandleButton(newPoint, BezierColor)) {
					_focusControl = i;
					Repaint();
				}
			}

			var diff = newPoint - _points.GetArrayElementAtIndex(i).vector2Value;
			if(i + 1 < arraySize) {
				_points.GetArrayElementAtIndex(i + 1).vector2Value += diff;
			}
			if(i > 0) {
				_points.GetArrayElementAtIndex(i - 1).vector2Value += diff;
			}

			_points.GetArrayElementAtIndex(i).vector2Value = newPoint;
		}

		if(_focusControl < 0) return;

		var left = _focusControl - 1;
		var right = _focusControl + 1;

		var hasLeft = left >= 0;
		var hasRight = right < arraySize;

		if(hasLeft) {
			_points.GetArrayElementAtIndex(left).vector2Value
				= HandlePosition(_points.GetArrayElementAtIndex(left).vector2Value, TangentColor);

			//Altキーを押したときは点対称になるような位置になる
			if(Event.current.alt && hasRight) {
				_points.GetArrayElementAtIndex(right).vector2Value
					= _points.GetArrayElementAtIndex(_focusControl).vector2Value * 2 - _points.GetArrayElementAtIndex(left).vector2Value;
			}

		}
		if(hasRight) {
			_points.GetArrayElementAtIndex(right).vector2Value
				= HandlePosition(_points.GetArrayElementAtIndex(right).vector2Value, TangentColor);

			if(Event.current.alt && hasLeft) {
				_points.GetArrayElementAtIndex(left).vector2Value
					= _points.GetArrayElementAtIndex(_focusControl).vector2Value * 2 - _points.GetArrayElementAtIndex(right).vector2Value;
			}
		}
	}

	/// <summary>
	/// Panモード時の動作
	/// </summary>
	private void PanState() {

		var centerPos = new Vector2();

		//中心座標を求める
		for(var i = 0;i < _bezier.Points.Count;i++) {
			centerPos += _points.GetArrayElementAtIndex(i).vector2Value;
		}
		centerPos /= _bezier.Points.Count();

		Undo.RecordObject(_bezier, "Pan Bezier");


		//ハンドルで移動した差分を加算
		var newCenterPos = (Vector2)Handles.PositionHandle(centerPos, Quaternion.identity);
		var diff = newCenterPos - centerPos;
		for(var i = 0;i < _bezier.Points.Count;i++) {
			_points.GetArrayElementAtIndex(i).vector2Value += diff;
		}

	}

	/// <summary>
	/// AddPointモード時の動作
	/// </summary>
	/// <param name="bezier"></param>
	private void AddPointState() {

		var e = Event.current;
		var controlID = GUIUtility.GetControlID(FocusType.Passive);
		var arraySize = _points.arraySize;

		switch(e.GetTypeForControl(controlID)) {
			case EventType.MouseDown:
				if(e.button != 0) return;

				GUIUtility.hotControl = controlID;
				Undo.RecordObject(_bezier, "Add BezierPoint");

				_bezier.AddPoint(HandleUtility.GUIPointToWorldRay(e.mousePosition).origin);
				_focusControl = _bezier.Points.Count - 2;
				_isClick = true;

				e.Use();
				break;

			case EventType.MouseDrag:
				if(e.button != 0) return;

				GUIUtility.hotControl = controlID;
				var mousePoint = (Vector2)HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
				_points.GetArrayElementAtIndex(arraySize - 3).vector2Value = mousePoint;
				_points.GetArrayElementAtIndex(arraySize - 1).vector2Value = _points.GetArrayElementAtIndex(arraySize - 2).vector2Value * 2 - mousePoint;

				e.Use();
				break;

			case EventType.MouseUp:
				if(e.button != 0) return;
				_isClick = false;
				break;
			default: break;
		}

		if(arraySize > 1)
			DrawTangentLine(
				_points.GetArrayElementAtIndex(arraySize - 3).vector2Value,
				_points.GetArrayElementAtIndex(arraySize - 2).vector2Value,
				_points.GetArrayElementAtIndex(arraySize - 1).vector2Value);


		if(_isClick && arraySize > 4) {
			DrawTangentLine(
				_points.GetArrayElementAtIndex(arraySize - 6).vector2Value,
				_points.GetArrayElementAtIndex(arraySize - 5).vector2Value,
				_points.GetArrayElementAtIndex(arraySize - 4).vector2Value);
		}
	}

	/// <summary>
	/// RemovePointモード時の動作
	/// </summary>
	private void RemovePointState() {

		var arraySize = _points.arraySize;

		for(var i = 1;i < arraySize;i += 3) {
			if(HandleButton(_points.GetArrayElementAtIndex(i).vector2Value, BezierColor)) {
				RemoveAnchorPoint(i);
			}
		}
	}

	/// <summary>
	/// アンカーポイントを削除する
	/// </summary>
	/// <param name="index"></param>
	private void RemoveAnchorPoint(int index) {
		Undo.RecordObject(_bezier, "Remove BezierPoint");
		_bezier.RemoveAnchorPoint(index);
		_focusControl = -1;
		Repaint();
	}

	/// <summary>
	/// ハンドルを描画する
	/// </summary>
	/// <param name="p0"></param>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	private static void DrawTangentLine(Vector2 p0, Vector2 p1, Vector2 p2) {

		Handles.color = TangentColor;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p1, p2);
	}

	/// <summary>
	/// ドラッグして動かせるようなハンドルを配置
	/// </summary>
	/// <param name="center"></param>
	/// <param name="col"></param>
	/// <returns></returns>
	private Vector2 HandlePosition(Vector2 center, Color col) {

		Handles.color = col;
		return Handles.Slider2D(center, Vector3.forward, Vector3.right, Vector3.up, HandleUtility.GetHandleSize(center) * HandleSize, Handles.CylinderHandleCap, 1f, false);
	}

	/// <summary>
	/// 押しボタンを配置
	/// </summary>
	/// <param name="center"></param>
	/// <param name="col"></param>
	/// <returns></returns>
	private static bool HandleButton(Vector2 center, Color col) {

		Handles.color = col;
		var size = HandleUtility.GetHandleSize(center) * HandleSize;
		return Handles.Button(center, Quaternion.identity, size, size, Handles.SphereHandleCap);
	}

	/// <summary>
	/// 操作するウィンドウを描画する
	/// </summary>
	private void DrawWindow() {

		Handles.BeginGUI();

		//ウィンドウを描画
		_windowRect = GUILayout.Window(WindowID, _windowRect, id => {

			var buttonName = new[] {
				"MovePoint", "Pan", "AddPoint", "RemovePoint"
			};

			for(var i = 0;i < 4;i++) {
				EditorGUI.BeginDisabledGroup(i == (int)_state);

				var isClick = GUILayout.Button(buttonName[i]);
				if(isClick) {
					_state = (State)i;
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

			_isLock = GUILayout.Toggle(_isLock, "Lock");

			GUI.DragWindow();

		}, "BezierEditTool", GUILayout.Width(100));

		Handles.EndGUI();
	}

	/// <summary>
	/// 線を表示する
	/// </summary>
	/// <param name="bezier"></param>
	/// <param name="gizmoType"></param>
	[DrawGizmo(GizmoType.NonSelected | GizmoType.Active)]
	private static void DrawGizmos(Bezier2D bezier, GizmoType gizmoType) {

		//分割数
		const int partition = 32;

		var points = bezier.Points;
		Gizmos.color = BezierColor;

		for(var i = 4;i < points.Count;i += 3) {

			var prevPoint = Bezier2D.GetPoint(points[i - 3], points[i - 2], points[i - 1], points[i], 0);
			for(var j = 1;j <= partition;j++) {
				var point = Bezier2D.GetPoint(points[i - 3], points[i - 2], points[i - 1], points[i], j / (float)partition);
				Gizmos.DrawLine(prevPoint, point);
				prevPoint = point;
			}
		}
	}
}
