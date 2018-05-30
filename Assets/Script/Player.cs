using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Vector2 moveVec = new Vector2(1, 0);
	public Vector2 velocity = new Vector2();
	public float addSpeed = 50.0f;
	public float maxSpeed = 2;

	new Rigidbody2D rigidbody;
	bool isFreeze = true;

	void Start() {
		rigidbody = GetComponent<Rigidbody2D>();
		isFreeze = true;

		GameMaster.gameMaster.OnGameStart += OnGameStart;
		GameMaster.gameMaster.OnGameEnd += () => {
			Destroy(gameObject);
		};
	}

	void OnGameStart() {
		isFreeze = false;
	}
	
	void Update () {

	}

	void FixedUpdate() {

		if(isFreeze) return;

		velocity = rigidbody.velocity;

		if(Mathf.Abs(rigidbody.velocity.x) < maxSpeed) {
			rigidbody.AddForce(moveVec * addSpeed * Time.deltaTime);
		}

	}

	void OnTriggerEnter2D(Collider2D other) {

		if(other.tag == "Goal") {
			GameMaster.gameMaster.GameClear();
		}

	}

	void OnCollisionEnter2D(Collision2D other) {

		if(other.gameObject.tag == "Wall") {
			moveVec.x *= -1;
			//rigidbody.velocity = new Vector2();
		}
	}
}
