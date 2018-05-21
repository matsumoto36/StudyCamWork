using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Bezier2D path;
	public float speed;

	public float MovedLength {
		get; private set;
	}

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {

		MovedLength += speed * Time.deltaTime;
		MovedLength = Mathf.Clamp(MovedLength, 0, path.GetLength());
		var t = MovedLength / path.GetLength();
		transform.position = path.GetPointNormalize(t);
	}
}
