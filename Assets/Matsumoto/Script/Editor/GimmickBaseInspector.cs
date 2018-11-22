using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GimmickBase), true)]
[CanEditMultipleObjects]
public class GimmickBaseInspector : Editor {

	private const int WindowID = 1234;
	private const float LineWidth = 10;
	private const float HandleSize = 0.2f;

	private static Rect _windowRect = new Rect(40, 40, 120, 120);

	private enum State {
		Preview, PointStart, PointEnd
	}
	private State _state = State.Preview;
	private bool _isLock;

	private GimmickManager _manager;
	private GimmickBase _gimmickBase;
	private GimmickBase[] _gimmicks;
	private Bezier2D _path;

	private SerializedProperty _lineColor;
	private SerializedProperty _startPoint;
	private SerializedProperty _endPoint;

	private void OnEnable() {

		_gimmickBase = (GimmickBase)target;
		_manager = _gimmickBase.GetComponentInParent<GimmickManager>();

		if(_manager) {
			_gimmicks = _manager.GetComponentsInChildren<GimmickBase>();
			_path = _gimmickBase.GetComponentInParent<Bezier2D>();
		}

		_lineColor = serializedObject.FindProperty("GimmickColor");
		_startPoint = serializedObject.FindProperty("StartPoint");
		_endPoint = serializedObject.FindProperty("EndPoint");
	}

	public override void OnInspectorGUI() {

		var right = 1.0f;

		if(_path) {
			right = _path.LineCount;
		}

		serializedObject.Update();

		EditorGUILayout.PropertyField(_lineColor);
		EditorGUILayout.Slider(_startPoint, 0.0f, right);
		EditorGUILayout.Slider(_endPoint, 0.0f, right);

		if(!_manager) {
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

	private void OnSceneGUI() {

		if(!_path) return;

		if(_manager) {

			DrawLine();

			switch(_state) {
				case State.Preview:
					DrawPoint();
					break;
				case State.PointStart:
					DrawButton(
						point => _gimmickBase.EndPoint >= point,
						point => {
							Undo.RecordObject(_gimmickBase, "Change StartPoint");
							_gimmickBase.StartPoint = point;
						});
					break;
				case State.PointEnd:
					DrawButton(
						point => _gimmickBase.StartPoint <= point,
						point => {
							Undo.RecordObject(_gimmickBase, "Change EndPoint");
							_gimmickBase.EndPoint = point;
						});
					break;
				default:
					throw new UnityException("");
			}
		}

		//使わないクリック処理でLock状態を実現
		if(_isLock) {
			var e = Event.current;
			var controlID = GUIUtility.GetControlID(FocusType.Passive);

			if(e.GetTypeForControl(controlID) == EventType.MouseDown) {
				if(e.button != 0) return;
				GUIUtility.hotControl = controlID;
				e.Use();
			}
		}

		DrawWindow();
	}

	/// <summary>
	/// 線がほかの線と被っていないか調べる
	/// </summary>
	/// <returns></returns>
	private bool CheckCollisionLine() {

		return _gimmicks.
			Where(item => item != _gimmickBase)
			.Any(item => item.EndPoint > _gimmickBase.StartPoint && item.StartPoint < _gimmickBase.EndPoint);
	}

	/// <summary>
	/// すべてのアンカーポイントにボタンを配置する
	/// </summary>
	/// <param name="predicate">配置する条件</param>
	/// <param name="buttonAction">押したとき</param>
	private void DrawButton(Func<int, bool> predicate, Action<int> buttonAction) {

		if(!_path) return;

		var points = _path.Points;

		for(var i = 1;i < points.Count;i += 3) {

			var point = (i - 1) / 3;
			if(!predicate(point)) continue;

			var size = HandleUtility.GetHandleSize(points[i]) * HandleSize;
			if(Handles.Button(points[i], Quaternion.identity, size, size, Handles.SphereHandleCap)) {
				buttonAction(point);
			}
		}

	}

	/// <summary>
	/// ギミックの有効範囲に線を引く
	/// </summary>
	private void DrawLine() {

		if(_startPoint.floatValue > _endPoint.floatValue) return;

		DrawGizmos(_gimmickBase, GizmoType.Active);
	}

	/// <summary>
	/// ギミックの開始点と終了点を描画する
	/// </summary>
	private void DrawPoint() {

		if(!_path) return;

		var lineCount = _path.LineCount;

		Handles.color = _gimmickBase.GimmickColor;

		var start = _path.GetPoint(_startPoint.floatValue / lineCount);
		var startSize = HandleUtility.GetHandleSize(start) * HandleSize;
		Handles.SphereHandleCap(0, start, Quaternion.identity, startSize, EventType.Repaint);

		var end = _path.GetPoint(_endPoint.floatValue / lineCount);
		var endSize = HandleUtility.GetHandleSize(end) * HandleSize;
		Handles.SphereHandleCap(0, end, Quaternion.identity, endSize, EventType.Repaint);
	}

	/// <summary>
	/// ツールのウィンドウを描画する
	/// </summary>
	private void DrawWindow() {

		Handles.BeginGUI();

		_windowRect = GUILayout.Window(WindowID, _windowRect, id => {

			var buttonName = new[] {
				"Preview", "SetStartPoint", "SetEndPoint"
			};

			for(var i = 0;i < 3;i++) {
				EditorGUI.BeginDisabledGroup(i == (int)_state);

				var isClick = GUILayout.Button(buttonName[i]);
				if(isClick) {
					_state = (State)i;
					Repaint();
				}

				EditorGUI.EndDisabledGroup();
			}

			_isLock = GUILayout.Toggle(_isLock, "Lock");

			GUI.DragWindow();

		}, "GimmickTool", GUILayout.Width(100));

		Handles.EndGUI();
	}

	[DrawGizmo(GizmoType.NonSelected)]
	private static void DrawGizmos(GimmickBase gimmick, GizmoType gizmoType) {

		var manager = gimmick.GetComponentInParent<GimmickManager>();

		if(!manager) return;

		var path = manager.GetComponent<Bezier2D>();

		if(!path) return;
		if(path.LineCount <= 0) return;

		var startPoint = gimmick.StartPoint;
		var endPoint = gimmick.EndPoint;

		if(startPoint > endPoint) return;

		//線を薄く表示
		var lineCount = path.LineCount;
		var partition = 32 * lineCount;

		var diff = endPoint - startPoint;
		var dt = diff / partition;

		var points = new List<Vector3> {
			path.GetPoint(startPoint / lineCount)
		};

		for(var i = 1;i <= partition;i++) {
			points.Add(path.GetPoint((startPoint + dt * i) / lineCount));
		}

		Handles.color = gimmick.GimmickColor;
		Handles.DrawAAPolyLine(LineWidth, points.ToArray());
	}

}
