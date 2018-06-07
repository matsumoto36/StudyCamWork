using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    Vector3 vec;
    int move;

	// Use this for initialization
	void Start () {
        move = 1;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            vec.x = move;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            vec.x = -move;
        }
        else
        {
            vec.x = 0;
        }

        transform.position += vec;
	}
}
