using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Bezier2D path;
	public float speed;

	float movedLength;
	public float MovedLength {
		get { return movedLength; }
		set {
			movedLength = value;
			ApplyMove();
		}
	}

	// Update is called once per frame
	public void Move () {

		MovedLength += speed * Time.deltaTime;
		ApplyMove();
	}

	void ApplyMove() {

		movedLength = Mathf.Clamp(MovedLength, 0, path.GetLength());
		var t = movedLength / path.GetLength();
		transform.position = path.GetPointNormalize(t);

		if(t >= 1.0f) {
			GameMaster.gameMaster.GameClear();
		}
	}
}
