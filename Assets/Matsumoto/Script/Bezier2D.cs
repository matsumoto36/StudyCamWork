using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier2D : MonoBehaviour {

	//ベジエ曲線を構成する頂点群
	[SerializeField] List<Vector2> points = new List<Vector2>();
	public List<Vector2> Points {
		get { return points; }
		set {
			points = value;
			pointsChangeFlg = true;
		}
	}

	//ベジエ曲線の長さ
	float length = 0;
	//線ごとの長さ
	float[] lengthAtLine;
	public float Length {
		get {
			if(pointsChangeFlg) {
				length = GetLength();
				pointsChangeFlg = false;
			}

			return length;
		}
	}

	/// <summary>
	/// ベジエ曲線の数
	/// </summary>
	public int LineCount {
		get { return Mathf.Max(0, points.Count / 3 - 1); }
	}

	//頂点が変更されたかを検知するフラグ
	bool pointsChangeFlg = true;

	/// <summary>
	/// 頂点を追加
	/// </summary>
	/// <param name="position"></param>
	public void AddPoint(Vector2 position) {

		Points.Add(position);
		Points.Add(position);
		Points.Add(position);
	}

	/// <summary>
	/// 指定した頂点を削除
	/// </summary>
	/// <param name="index"></param>
	public void RemovePoint(int index) {

		if((index - 1) % 3 != 0) return;

		Points.RemoveRange(index - 1, 3);

	}

	/// <summary>
	/// ベジエ曲線を構成する最大の点を取得
	/// </summary>
	/// <returns></returns>
	public int GetLastBezierPoint() {
		return Mathf.Max(0, (Points.Count - 1) / 3 * 3 + 1);
	}

	/// <summary>
	/// 曲線の長さを取得する
	/// </summary>
	/// <param name="partition">分割(精度)</param>
	/// <returns></returns>
	public float GetLength(int partition = 32) {

		var length = 0.0f;
		var tempLength = 0.0f;

		lengthAtLine = new float[LineCount];

		for(int i = 1;i < Points.Count - 3;i+=3) {
			var prevPos = GetPoint(Points[i], Points[i + 1], Points[i + 2], Points[i + 3], 0);
			tempLength = 0;
			for(int j = 1;j <= partition;j++) {

				var pos = GetPoint(Points[i], Points[i + 1], Points[i + 2], Points[i + 3], (float)j / partition);
				tempLength += (pos - prevPos).magnitude;
				prevPos = pos;
			}

			var n = (i - 1) / 3;
			lengthAtLine[n] = tempLength;
			length += tempLength;
		}

		return length;
	}

	/// <summary>
	/// 2点間の距離を返す(正規化されていない、ギミック用)
	/// </summary>
	/// <param name="startPoint"></param>
	/// <param name="endPoint"></param>
	/// <param name="partition"></param>
	/// <returns></returns>
	public float GetPointLength(float startPoint, float endPoint, int partition = 32) {

		var diff = endPoint - startPoint;
		var part = (int)(partition * diff);
		if(part <= 0) return diff;

		var dt = diff / part;
		var prevLinePoint = GetPoint(startPoint / LineCount);

		var l = 0.0f;
		for(int i = 0;i <= part;i++) {
			var p = GetPoint((startPoint + dt * i) / LineCount);
			l += (prevLinePoint - p).magnitude;
			prevLinePoint = p;
		}
		return l;
	}

	/// <summary>
	/// 2点間の距離を返す
	/// </summary>
	/// <param name="startPoint"></param>
	/// <param name="endPoint"></param>
	/// <param name="partition"></param>
	/// <returns></returns>
	public float GetPointNormalizeLength(float startPoint, float endPoint, int partition = 32) {

		var diff = endPoint - startPoint;
		var part = (int)(partition * diff);
		if(part <= 0) return diff;

		var dt = diff / part;
		var prevLinePoint = GetPoint(startPoint / LineCount);

		var l = 0.0f;
		for(int i = 0;i <= part;i++) {
			var p = GetPointNormalize((startPoint + dt * i) / LineCount);
			l += (prevLinePoint - p).magnitude;
			prevLinePoint = p;
		}
		return l;
	}

	/// <summary>
	/// 割合を指定して曲線内の点を取得する
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public Vector2 GetPointNormalize(float t) {

		var lineCount = LineCount;
		if(lineCount <= 0) return new Vector2();
		if(t <= 0.0f) return points[1];
		if(t >= 1.0f) return points[GetLastBezierPoint()];

		var length = GetLength() * t;
		var sumLength = 0.0f;
		var section = 0;

		for(section = 0;section < lineCount;section++) {
			sumLength += lengthAtLine[section];
			if(length < sumLength) {
				break;
			}
		}

		var startPoint = section * 3 + 1;
		var ratio =
			(length - (sumLength - lengthAtLine[section])) / lengthAtLine[section];

		return GetPointNormalize(
			points[startPoint],
			points[startPoint + 1],
			points[startPoint + 2],
			points[startPoint + 3],
			ratio);
	}

	public Vector2 GetPoint(float t) {

		var lineCount = LineCount;
		if(lineCount <= 0) return new Vector2();
		if(t <= 0.0f) return points[1];
		if(t >= 1.0f) return points[GetLastBezierPoint()];

		var section = (int)(lineCount * t);

		var startPoint = section * 3 + 1;
		var ratio = lineCount * t - section;

		return GetPoint(
			points[startPoint],
			points[startPoint + 1],
			points[startPoint + 2],
			points[startPoint + 3],
			ratio);
	}

	/// <summary>
	/// 距離が等速なベジエ曲線の点を取得
	/// </summary>
	/// <param name="p0"></param>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	/// <param name="p3"></param>
	/// <param name="t"></param>
	/// <param name="partition"></param>
	/// <returns></returns>
	public static Vector2 GetPointNormalize(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, int partition = 32) {

		if(t <= 0.0f) return p0;
		if(t >= 1.0f) return p3;
		if(partition < 4) {
			Debug.LogAssertion("最低分割数より下回っています");
			return p0;
		}

		var partArray = new float[partition + 1];
		var part_i = 1.0f / partition;

		var p = p0;
		var q = new Vector2();
		var tt = 0.0f;
		var i = 0;

		partArray[0] = 0.0f;

		for(i = 1;i <= partition;i++) {
			tt += part_i;
			q = GetPoint(p0, p1, p2, p3, tt);

			var diff = q - p;
			partArray[i] = partArray[i - 1]
				+ Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);

			p = q;
		}

		//partArrayを0.0～1.0の範囲に正規化する
		for(i = 1;i <= partition;i++) {
			partArray[i] /= partArray[partition];
		}

		//tを距離として、該当するpartArray区画を導く
		for(i = 0;i < partition;i++) {
			if(partArray[i] <= t && t <= partArray[i + 1]) break;

		}

		//区画内は線形補間で算出
		var x = (t - partArray[i]) / (partArray[i + 1] - partArray[i]);
		x = (i * (1 - x) + (1 + i) * x) * part_i;
		return GetPoint(p0, p1, p2, p3, x);
	}

	/// <summary>
	/// ベジエ曲線の点を取得
	/// </summary>
	/// <param name="p0"></param>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	/// <param name="p3"></param>
	/// <param name="t"></param>
	/// <returns></returns>
	public static Vector2 GetPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {

		if(t <= 0.0f) return p0;
		if(t >= 1.0f) return p3;

		var oneMinusT = 1f - t;
		return oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}
}
