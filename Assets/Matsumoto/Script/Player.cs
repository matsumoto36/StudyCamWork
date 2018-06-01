using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Bezier2D path;
	public float speed;

	public bool IsFreeze {
		get; set;
	}

	public float MovedLength {
		get; set;
	}

	void Start() {
		IsFreeze = true;
		GameMaster.gameMaster.OnGameStart += () => IsFreeze = false;
		GameMaster.gameMaster.OnGameClear += () => IsFreeze = true;
		GameMaster.gameMaster.OnGameOver += () => IsFreeze = true;
	}

	// Update is called once per frame
	void Update () {

		if(IsFreeze) return;

		MovedLength += speed * Time.deltaTime;
		MovedLength = Mathf.Clamp(MovedLength, 0, path.GetLength());
		var t = MovedLength / path.GetLength();
		transform.position = path.GetPointNormalize(t);

		if(t >= 1.0f) {
			GameMaster.gameMaster.GameClear();
		}

	}
}
