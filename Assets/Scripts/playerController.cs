using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private Rigidbody2D rb;
	private bool jumpPressed = false;
	private bool jumpLetGo = false;
	public Vector2 jumpVector = new Vector2 (0, 40); 
	public Vector2 airJumpVector = new Vector2 (0, 40);
	public KeyCode jumpKey = KeyCode.Space;
	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";
	public int minJumpVelocity = 10;
	private float directionalInput;
	public int moveSpeed = 12;
	private bool inAir = false;
	public int airJumps = 1;
	//public int airSpecials = 1;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}

	void OnCollisionEnter2D(Collision2D coll){
		inAir = false;
		airJumps = 1;
	}

	void OnCollisionExit2D(Collision2D coll){
		inAir = true;
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown (jumpKey)){
			jumpPressed = true;
		}

		if(Input.GetKeyUp (jumpKey)){
			jumpLetGo = true;
		}

		directionalInput = Input.GetAxisRaw (horizontalAxis);

	}

	//Also called at a specific interval
	void FixedUpdate(){

		float targetVelocityX = directionalInput * moveSpeed;
		rb.velocity = new Vector2 (targetVelocityX , rb.velocity.y);	

		if(jumpPressed){
			rb.velocity = Vector2.zero;
			if(!inAir){
				rb.velocity += jumpVector;
			} else if(inAir && airJumps > 0){
				rb.velocity += airJumpVector;
				airJumps--;
			}
			jumpPressed = false;
		}

		if(jumpLetGo){
			if(rb.velocity.y > minJumpVelocity){
				rb.velocity = new Vector2(rb.velocity.x, minJumpVelocity);
			}
			jumpLetGo = false;
		}
	}
}
