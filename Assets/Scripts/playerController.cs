using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	private Rigidbody2D rb;
	private Animator animator;

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

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
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

		//Gameplay
		float targetVelocityX = directionalInput * moveSpeed;
		rb.velocity = new Vector2 (targetVelocityX , rb.velocity.y);	

		if(jumpPressed){
			if(!inAir){
				rb.velocity = Vector2.zero;
				rb.velocity += jumpVector;
			} else if(inAir && airJumps > 0){
				rb.velocity = Vector2.zero;
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

		//Animations

		//Facing direction
		if(directionalInput < 0){
			transform.localRotation = Quaternion.Euler (0, 180, 0);
		} else if(directionalInput > 0) {
			transform.localRotation = Quaternion.Euler (0, 0, 0);
		}



		//Sprite
		if(inAir && rb.velocity.y > 0.01){
			animator.SetBool ("walking", false);
			animator.SetBool ("falling", false);
			animator.SetBool ("idle", false);
			animator.SetBool ("jumping", true);
		}
		if(inAir && rb.velocity.y < 0.01){
			animator.SetBool ("walking", false);
			animator.SetBool ("falling", true);
			animator.SetBool ("idle", false);
			animator.SetBool ("jumping", false);
		}
		if(!inAir && rb.velocity.x != 0){
			animator.SetBool ("walking", true);
			animator.SetBool ("falling", false);
			animator.SetBool ("idle", false);
			animator.SetBool ("jumping", false);
		}
		if(!inAir && rb.velocity.x == 0){
			animator.SetBool ("walking", false);
			animator.SetBool ("falling", false);
			animator.SetBool ("idle", true);
			animator.SetBool ("jumping", false);
		}
		//Not colliding, moving upwards = jumping
		//Not colliding, moving downwards = falling
		//Colliding, moving sideways = walking
		//Colliding, standing still = idle

		//Parameters: walking, falling, idle, jumping
		//States: playerJumping, playerIdle, playerFalling, playerWalking

		//animator.Play ("playerIdle");
		//animator.SetFloat ("ySpeed", rb.velocity.y);
		//animator.SetFloat ("xSpeed", rb.velocity.x);
	}
}
