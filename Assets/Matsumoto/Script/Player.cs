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

	public void Init() {
	}

	// Update is called once per frame
	public void Move () {

		MovedLength += speed * Time.deltaTime;
	}

	void ApplyMove() {

		movedLength = Mathf.Clamp(movedLength, 0, path.Length);
		var t = movedLength / path.Length;
		transform.position = path.GetPointNormalize(t);

		if(t >= 1.0f) {
			GameMaster.gameMaster.GameClear();
		}
	}
}
