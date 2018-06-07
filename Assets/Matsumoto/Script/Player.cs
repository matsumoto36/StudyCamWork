using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Bezier2D path;
	public float speed;
	//public LineRenderer lineMaskLinerenderer;

	float lineMaskDistance = 0.01f;
	int placedCount = 0;

	float movedLength;
	public float MovedLength {
		get { return movedLength; }
		set {

			movedLength = value;
			//var length = path.GetLength();

			//while(movedLength - placedCount * lineMaskDistance >= lineMaskDistance) {

			//	var pos = path.GetPointNormalize(placedCount * lineMaskDistance / length);
			//	lineMaskLinerenderer.positionCount = placedCount + 1;
			//	lineMaskLinerenderer.SetPosition(placedCount, pos);
			//	placedCount++;
			//}

			ApplyMove();
		}
	}

	public void Init() {
		//lineMaskLinerenderer = Instantiate(lineMaskLinerenderer);
	}

	// Update is called once per frame
	public void Move () {

		MovedLength += speed * Time.deltaTime;
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
