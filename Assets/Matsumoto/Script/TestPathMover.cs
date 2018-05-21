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

		transform.position = b.GetPointNormalize(range);
		
	}
}
