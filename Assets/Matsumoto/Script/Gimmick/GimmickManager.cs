using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ギミック全体を管理するクラス
/// ギミックを追加する場合はこのクラスの子オブジェクトにすること
/// </summary>
public class GimmickManager : MonoBehaviour {

	public const float LineWidthMin = 0.05f;
	public const float LineWidthMax = 0.2f;
	public const float MoveZ = 18;

	public LineRenderer LinePre;

	private Player _player;
	private GimmickInfo[] _gimmicks;
	private List<LineInfo> _lines;

	private float _startTime;

	public Bezier2D Path { get; private set; }

	public void Init(Bezier2D path, Player player) {

		Path = path;
		_player = player;

		var g = GetComponentsInChildren<GimmickBase>();
		_gimmicks = new GimmickInfo[g.Length];

		for(var i = 0;i < g.Length;i++) {
			_gimmicks[i] = new GimmickInfo(g[i]);
			g[i].Init();
		}

		//昇順にソート
		_gimmicks = _gimmicks
			.OrderBy((item) => item.Gimmick.StartPoint)
			.ToArray();

		SetStartTime();
		SetLine();

		for(var i = g.Length - 1; i >= 0;i--) {
			_gimmicks[i].Gimmick.SpawnModel(player);
		}

		GameMaster.Instance.OnGameStart += () => _startTime = Time.time;
	}

	/// <summary>
	/// ステージ上のギミックの更新
	/// </summary>
	public void GimmickUpdate() {

		foreach(var item in _gimmicks) {

			if(item.IsUsed) continue;

			var gimmickTime = Time.time - _startTime;

			//発動前
			if(!item.IsActive && item.WaitTime > gimmickTime) {

				item.Gimmick.OnRemainingTime(_player, item.WaitTime - gimmickTime);
			}

			//発動した瞬間
			if(!item.IsActive && item.WaitTime < gimmickTime) {

				item.IsActive = true;
				item.Gimmick.OnAttach(_player);
			}

			//発動中
			if(item.IsActive) {

				item.Gimmick.OnApplyUpdate(_player, gimmickTime - item.WaitTime);
			}

			//発動が終了した瞬間
			if(item.IsActive && item.WaitTime + item.Duration < gimmickTime) {

				item.IsActive = false;
				item.IsUsed = true;
				item.Gimmick.OnDetach(_player);
			}
		}

		//線の可視状態を設定
		SetLineVisible();
	}

	/// <summary>
	/// プレイヤーの位置で線の可視状態を設定
	/// </summary>
	private void SetLineVisible() {

		var moveLength = _player.MovedLength;

		foreach(var item in _lines) {
			if(!item.Renderer) continue;

			var fillValue = 1.0f;
			var startLength = item.PathStartLength;
			var endLength = item.PathEndLength;

			//通り過ぎた線の場合
			if(moveLength >= endLength) {
				fillValue = 0.0f;
			}
			//通っている途中の線の場合
			else if(moveLength > startLength) {
				var t = moveLength - startLength;
				fillValue = 1 - (t / (endLength - startLength));
			}
			//可視範囲を設定
			item.Renderer.material.SetFloat("_Fill", fillValue);
		}

	}

	/// <summary>
	/// 開始時間をあらかじめ計算する
	/// </summary>
	private void SetStartTime() {

		var speed = _player.Speed;
		var sumTime = 0.0f;
		var prevPoint = 0.0f;
		foreach (var gimmickInfo in _gimmicks) {

			var gimmick = gimmickInfo.Gimmick;

			//通常ゾーン
			var t = Path.GetPointLength(prevPoint, gimmick.StartPoint) / speed;

			//ギミックのゾーン
			sumTime += t;
			gimmickInfo.WaitTime = sumTime;
			gimmickInfo.Duration = gimmick.GetSectionTime(speed);
			sumTime += gimmickInfo.Duration;
			prevPoint = gimmick.EndPoint;
		}
	}

	/// <summary>
	/// LineRendererを各ポイントに設置
	/// </summary>
	private void SetLine() {

		//普通に引く線はあらかじめメソッドを作っておく
		Func<Bezier2D, float, float, float, LineRenderer> drawLine = (path, from, to, lineZ) => {

			var diff = to - from;
			if (!(diff > 0)) return null;

			var partition = (int)(256 * diff);
			if(partition == 0) partition = 1;

			var dt = diff / partition;
			var point = new Vector3[partition + 1];
			var keyframe = new Keyframe[partition + 1];

			for(var i = 0;i <= partition;i++) {
				point[i] = path.GetPoint((from + (dt * i)) / path.LineCount);
				point[i].z = lineZ;

				keyframe[i] = new Keyframe(i / (float)partition, Mathf.Lerp(LineWidthMin, LineWidthMax, 1 - lineZ / MoveZ));
			}

			//線を引く
			var l = Instantiate(LinePre);
			l.material = new Material(l.material);
			l.positionCount = point.Length;
			l.SetPositions(point);
			l.widthCurve = new AnimationCurve(keyframe);
			return l;

		};

		//ここから線を引く処理開始
		_lines = new List<LineInfo>();
		var prevPoint = 0.0f;
		var z = 0.0f;
		foreach (var gimmickInfo in _gimmicks) {
			var gimmick = gimmickInfo.Gimmick;

			//通常ゾーン
			_lines.Add(new LineInfo(
				drawLine(Path, prevPoint, gimmick.StartPoint, z),
				Path.GetPointLength(0, prevPoint),
				Path.GetPointLength(0, gimmick.StartPoint)
			));

			//ギミックゾーン
			var gimmickLine = Instantiate(LinePre);
			gimmickLine.material = new Material(gimmickLine.material);
			gimmick.EditGimmickLine(gimmickLine, ref z);

			var line = new LineInfo(
				gimmickLine,
				Path.GetPointLength(0, gimmick.StartPoint),
				Path.GetPointLength(0, gimmick.EndPoint)
			) {GimmickInfo = gimmickInfo};

			_lines.Add(line);

			prevPoint = gimmick.EndPoint;
		}

		//最後の線を引く
		_lines.Add(new LineInfo(
			drawLine(Path, prevPoint, Path.LineCount, z),
				Path.GetPointLength(0, prevPoint),
				Path.Length
		));

	}
}

/// <summary>
/// 線の情報をまとめておくクラス
/// </summary>
[Serializable]
public class LineInfo {

	public LineRenderer Renderer;
	public float PathStartLength;
	public float PathEndLength;

	public GimmickInfo GimmickInfo;

	public LineInfo(LineRenderer renderer, float pathStartLength, float pathEndLength) {
		Renderer = renderer;
		PathStartLength = pathStartLength;
		PathEndLength = pathEndLength;
	}
}

/// <summary>
/// ギミックの情報をまとめておくクラス
/// </summary>
public class GimmickInfo {

	public GimmickBase Gimmick;
	public bool IsActive;
	public bool IsUsed;
	public float WaitTime;
	public float Duration;

	public GimmickInfo(GimmickBase gimmick) {
		Gimmick = gimmick;
	}
}