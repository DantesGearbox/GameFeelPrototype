using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour {

	private int movespeed = 5;
	private int moveAmount = 180;
	private int move = 0;
	private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		rb.velocity = new Vector2 (0,  movespeed);
	}
	
	// Update is called once per frame
	void Update () {
		move++;
		if(move > moveAmount){
			movespeed = movespeed * -1;
			rb.velocity = new Vector2 (0,  movespeed);
			move = 0;
		}
	}
}
