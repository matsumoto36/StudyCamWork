using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPathMover : MonoBehaviour {

	public Bezier2D b;
	public bool isNormalize;

	[Range(0, 1)]
	public float range;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		//if(isNormalize) {
		//	transform.position =
		//		Bezier2D.GetPointNormalize(b.Points[1], b.Points[2], b.Points[3], b.Points[4], range);
		//}
		//else {
		//	transform.position =
		//		Bezier2D.GetPoint(b.Points[1], b.Points[2], b.Points[3], b.Points[4], range);
		//}

		transform.position = b.GetPoint(range);
		
	}
}
