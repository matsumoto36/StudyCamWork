using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier2D : MonoBehaviour {

	const int DEFAULT_PARTITION = 32;

	[SerializeField] List<Vector2> points = new List<Vector2>();
	public List<Vector2> Points {
		get { return points; }
		set {
			points = value;
			pointsChangeFlg = true;
		}
	}

	float length = 0;
	public float Length {
		get {
			if(pointsChangeFlg) {
				length = GetLength(DEFAULT_PARTITION);
				pointsChangeFlg = false;
			}

			return length;
		}
	}

	bool pointsChangeFlg = true;



	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void AddPoint(Vector2 position) {

		Points.Add(position);
		Points.Add(position);
		Points.Add(position);
	}

	public void RemovePoint(int index) {

		if((index - 1) % 3 != 0) return;

		Points.RemoveRange(index - 1, 3);

	}

	public int GetLastBezierPoint() {
		return Mathf.Max(0, (Points.Count - 1) / 3 * 3 + 1);
	}

	/// <summary>
	/// 曲線の長さを取得する
	/// </summary>
	/// <param name="partition">分割(精度)</param>
	/// <returns></returns>
	public float GetLength(int partition) {

		var length = 0.0f;

		for(int i = 1;i < Points.Count;i+=3) {

			var prevPos = GetPoint(Points[i], Points[i + 1], Points[i + 2], Points[i + 3], 0);
			for(int j = 1;j <= partition;j++) {

				var pos = GetPoint(Points[i], Points[i + 1], Points[i + 2], Points[i + 3], (float)j / partition);
				length += (pos - prevPos).magnitude;
			}
		}

		return length;
	}

	public Vector2 GetPoint(float t) {

		var lineCount = points.Count / 3 - 1;
		if(lineCount <= 0) return new Vector2();
		if(t <= 0.0f) return points[1];
		if(t >= 1.0f) return points[GetLastBezierPoint()];

		var section = (int)(lineCount * t);
		var startPoint = section * 3 + 1;
		var part = section == 0 ? 0 : 1.0f - 1.0f / (section + 1);

		return GetPointNormalize(
			points[startPoint],
			points[startPoint + 1],
			points[startPoint + 2],
			points[startPoint + 3],
			t * lineCount - section);
	}

	public static Vector2 GetPointNormalize(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, int partition = 16) {

		if(t <= 0.0f) return p0;
		if(t >= 1.0f) return p3;
		if(partition < 4) {
			Debug.LogAssertion("最低分割数より下回っています");
			return p0;
		}

		const float EPS = 0.0001f;
		var partArray = new float[partition + 1];
		var part_i = 1.0f / partition;

		var p = p0;
		var xy = new Vector2();
		var tt = 0.0f;
		var i = 0;

		partArray[0] = 0.0f;

		for(i = 1;i <= partition;i++) {
			tt += part_i;
			xy = GetPoint(p0, p1, p2, p3, tt);

			var diff = xy - p;
			partArray[i] = partArray[i - 1]
				+ Mathf.Sqrt(diff.x * diff.x * diff.y * diff.y);

			p = xy;
		}

		//partArrayを0.0～1.0の範囲に正規化する
		var x = 1.0f / partArray[partition];
		for(i = 1;i <= partition;i++) {
			partArray[i] *= x;
		}

		//tを距離として、該当するpartArray区画を導く
		for(i = 0;i < partition;i++) {
			if(t >= partArray[i] && t <= partArray[i + 1]) break;
			if(i >= partition) {
				return GetPoint(p0, p1, p2, p3, t);
			}
		}

		//区画内は線形補間で算出
		x = (partArray[i + 1] - partArray[i]);
		if(x < EPS) x = EPS;
		x = (t - partArray[i]) / x;
		return GetPoint(p0, p1, p2, p3, (i * (1.0f - x) + (i + 1) * x) * part_i);
	}

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
