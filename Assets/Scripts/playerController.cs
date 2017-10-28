using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour {

	//Components
	Rigidbody2D rb;

	//Outer variables

	//Inner variables
	private int jumpSpeed = 40;
	private int minJumpSpeed = 10;


	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown (KeyCode.Space)){
			rb.velocity += new Vector2 (0, jumpSpeed);
		}

		if (Input.GetKeyUp (KeyCode.Space) && rb.velocity.y > minJumpSpeed) {
			rb.velocity = new Vector2 (rb.velocity.x, minJumpSpeed);
		}



	}
}
