using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier2D : MonoBehaviour {

	public List<Vector2> points = new List<Vector2>();

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void AddPoint(Vector2 position) {

		points.Add(position);
		points.Add(position);
		points.Add(position);
	}

	public void RemovePoint(int index) {

		if((index - 1) % 3 != 0) return;

		points.RemoveRange(index - 1, 3);

	}

	public int GetLastBezierPoint() {
		return Mathf.Max(0, (points.Count - 1) / 3 * 3 + 1);
	}

	public static Vector2 GetPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
		var oneMinusT = 1f - t;
		return oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}
}
