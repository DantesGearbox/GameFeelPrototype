using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	private Rigidbody2D rb;
	private Animator animator;
	public LayerMask collisionMask;

	private bool jumpPressed = false;
	private bool jumpLetGo = false;
	private Vector2 jumpVector = new Vector2 (0, 35); 
	private Vector2 wallJumpVectorRight = new Vector2 (-30, 30);
	private Vector2 wallJumpVectorLeft = new Vector2 (30, 30); 
	private Vector2 airJumpVector = new Vector2 (0, 30);
	private KeyCode jumpKey = KeyCode.Space;
	private string horizontalAxis = "Horizontal";
	private string verticalAxis = "Vertical";
	public int minJumpVelocity = 6;
	private float directionalInput;
	public int moveSpeed = 12;
	private bool inAir = false;
	private int airJumps = 1;
	public float slideDownSpeed = -3.0f;

	private float accelerationTimeAirborne = .1f;
	private float accelerationTimeGrounded = .05f;
	private float velocityXSmoothing;

	private BoxCollider2D bc;
	private RaycastOrigins raycastOrigins;
	private NearbyWalls nearbyWalls;
	private float rayLengthUpDown = 0.1f;
	private float rayLengthLeftRight = 0.1f;

	private Vector3 originalCameraPosition;
	public float shakeAmt = 0.1f;
	public Camera mainCamera;

	public Vector3 playerStartingPosition = new Vector3 (0f, 0f, -2f);
	public AudioClip groundjump;
	public AudioClip airjump;
	public AudioClip walking;

	private float restartLevelDelay = 1.0f;
	public bool stopMoving = false;

	private float maxTime = 60.0f;
	public float timer;
	public Text textTimer;
	private float timePerClock = 5.0f;

	//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
	private void OnTriggerEnter2D (Collider2D other)
	{
		//Check if the tag of the trigger collided with is Exit.
		if(other.tag == "Exit")
		{
			//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
			GameManager.instance.timer = timer-maxTime;
			Invoke ("Restart", restartLevelDelay);
			Invoke ("startPlayerMoving", restartLevelDelay + 2); //THIS IS PART OF THE HACK BELOW

			//Disable the player object since level is over.
			stopMoving = true;
		}
		else if(other.tag == "Clock")
		{
			//Add pointsPerFood to the players current food total.
			timer += timePerClock;
			textTimer.text = ""+timer;

			//Disable the food object the player collided with.
			other.gameObject.SetActive (false);
		}
	}

	//Restart reloads the scene when called.
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game.
		SceneManager.LoadScene (0);
	}

	// THIS IS A HACK//
	private void startPlayerMoving(){
		transform.position = playerStartingPosition;
		stopMoving = false;
		rb.gravityScale = 10f;
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
		bc = GetComponent<BoxCollider2D>();
		textTimer.text = "Time Left: "+(int)timer;
		timer = GameManager.instance.timer + maxTime;
		stopMoving = true;
		Invoke ("startPlayerMoving", 2);
	}

	void checkIfGameOver(){
		if(timer < 0){
			GameManager.instance.GameOver ();
			stopMoving = true;
			timer = maxTime;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if(GameManager.instance.resetPlayer){
			startPlayerMoving ();
			GameManager.instance.resetPlayer = false;
		}

		Debug.Log (timer);

		if(Input.GetKeyDown (jumpKey)){
			jumpPressed = true;
		}

		if(Input.GetKeyUp (jumpKey)){
			jumpLetGo = true;
		}

		if(!stopMoving){
			timer -= Time.deltaTime;
			textTimer.text = "Time Left: "+(int)timer;	
		}

		checkIfGameOver ();

		updateRaycastOrigins();
		updateNearbyWalls();

		if(colliding ()){
			airJumps = 1;
		}

		if(!nearbyWalls.nearBot){
			inAir = true;
		} else {
			inAir = false;
		}

		directionalInput = Input.GetAxisRaw (horizontalAxis);


		//Animations

		//Facing direction
		if(directionalInput < 0){
			transform.localRotation = Quaternion.Euler (0, 180, 0);
		} else if(directionalInput > 0) {
			transform.localRotation = Quaternion.Euler (0, 0, 0);
		}

		//Sprite
		if(inAir && rb.velocity.y > 0.01f){
			//Debug.Log ("jumping!");
			//animator.SetTrigger ("jumping");
			if(airJumps > 0){
				animator.Play ("playerJumping");	
			} else {
				animator.Play ("playerJumpingNoJump");	
			}
		}
		else if(inAir && rb.velocity.y < 0.01f && !collidingLeftOrRight()){
			//Debug.Log ("falling!");
			//animator.SetTrigger ("falling");
			if(airJumps > 0){
				animator.Play ("playerFalling");	
			} else {
				animator.Play ("playerFallingNoJump");	
			}
		}
		else if(!inAir && (rb.velocity.x > 0.1f || rb.velocity.x < -0.1f) && nearbyWalls.nearBot){
			//Debug.Log ("walk!");
			//animator.SetBool ("walking", true);
			animator.Play ("playerWalking");
		}
		else if(!inAir && (rb.velocity.x < 0.1f || rb.velocity.x > -0.1f) && nearbyWalls.nearBot){
			//Debug.Log ("idle!");
			//animator.SetBool ("idle", false);
			animator.Play ("playerIdle");
		}
		else if(collidingLeftOrRight() && rb.velocity.y < 0.0f){
			//Debug.Log ("cling");
			//animator.SetTrigger ("clinging");
			animator.Play ("playerClinging");
		}
	}

	//Also called at a specific interval
	void FixedUpdate(){

		if (stopMoving){
			rb.velocity = Vector3.zero;
			rb.gravityScale = 0f;
			return;	
		}

		float targetVelocityX = directionalInput * moveSpeed;
		float velX = Mathf.SmoothDamp (rb.velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeGrounded);

		if(velX > 0){
			//SoundManager.instance.RandomizeSfx (walking);
		}

		rb.velocity = new Vector2 (velX , rb.velocity.y);

		if(jumpPressed){
			if (nearbyWalls.nearRight && !nearbyWalls.nearBot) {
				rb.velocity = new Vector2 (rb.velocity.x , 0);
				rb.velocity += wallJumpVectorRight;
				SoundManager.instance.RandomizeSfx (groundjump);
			}
			if (nearbyWalls.nearLeft && !nearbyWalls.nearBot) {
				rb.velocity = new Vector2 (rb.velocity.x , 0);
				rb.velocity += wallJumpVectorLeft;
				SoundManager.instance.RandomizeSfx (groundjump);
			}
			
			if(!inAir){
				rb.velocity = new Vector2 (rb.velocity.x , 0);
				rb.velocity += jumpVector;
				SoundManager.instance.RandomizeSfx (groundjump);
				shakeCam ();
			} else if(inAir && airJumps > 0 && !collidingLeftOrRight ()){
				rb.velocity = new Vector2 (rb.velocity.x , 0);
				rb.velocity += airJumpVector;
				SoundManager.instance.RandomizeSfx (airjump);
				shakeCam ();
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

		if(collidingLeftOrRight()){
			if(rb.velocity.y < slideDownSpeed){
				rb.velocity = new Vector2(rb.velocity.x, slideDownSpeed);
			}
		}
	}

	void shakeCam(){
		InvokeRepeating("CameraShake", 0, .01f);
		Invoke("StopShaking", 0.1f);
	}

	void CameraShake()
	{
		if(shakeAmt>0) 
		{
			originalCameraPosition = mainCamera.transform.position;
			float quakeAmt = (Random.value*2-1)*shakeAmt*2 - shakeAmt;
			Vector3 pp = mainCamera.transform.position;
			pp.y+= quakeAmt; // can also add to x and/or z
			pp.x+= quakeAmt;
			mainCamera.transform.position = pp;
		}
	}

	void StopShaking()
	{
		CancelInvoke("CameraShake");
		//mainCamera.transform.position = originalCameraPosition;
	}


	private bool collisionUp()
	{
		bool hit = false;

		if (Physics2D.Raycast(raycastOrigins.topLeft, Vector2.up, rayLengthUpDown, collisionMask))
		{
			hit = true;
		}
		else if (Physics2D.Raycast(raycastOrigins.topRight, Vector2.up, rayLengthUpDown, collisionMask))
		{
			hit = true;
		}

		Debug.DrawRay(raycastOrigins.topLeft, Vector2.up * rayLengthUpDown, Color.red);
		Debug.DrawRay(raycastOrigins.topRight, Vector2.up * rayLengthUpDown, Color.red);

		return hit;
	}

	private bool collisionDown()
	{
		bool hit = false;

		if (Physics2D.Raycast(raycastOrigins.botLeft, Vector2.down, rayLengthUpDown, collisionMask))
		{
			hit = true;
		}
		else if (Physics2D.Raycast(raycastOrigins.botRight, Vector2.down, rayLengthUpDown, collisionMask))
		{
			hit = true;
		}

		Debug.DrawRay(raycastOrigins.botLeft, Vector2.down * rayLengthUpDown, Color.red);
		Debug.DrawRay(raycastOrigins.botRight, Vector2.down * rayLengthUpDown, Color.red);

		return hit;
	}

	private bool collisionLeft()
	{
		bool hit = false;

		if (Physics2D.Raycast(raycastOrigins.topLeft, Vector2.left, rayLengthLeftRight, collisionMask))
		{
			hit = true;
		}
		else if (Physics2D.Raycast(raycastOrigins.botLeft, Vector2.left, rayLengthLeftRight, collisionMask))
		{
			hit = true;
		}

		Debug.DrawRay(raycastOrigins.botLeft, Vector2.left * rayLengthLeftRight, Color.red);
		Debug.DrawRay(raycastOrigins.topLeft, Vector2.left * rayLengthLeftRight, Color.red);

		return hit;
	}

	private bool collisionRight()
	{
		bool hit = false;

		if (Physics2D.Raycast(raycastOrigins.topRight, Vector2.right, rayLengthLeftRight, collisionMask))
		{
			hit = true;
		}
		else if (Physics2D.Raycast(raycastOrigins.botRight, Vector2.right, rayLengthLeftRight, collisionMask))
		{
			hit = true;
		}

		Debug.DrawRay(raycastOrigins.botRight, Vector2.right * rayLengthLeftRight, Color.red);
		Debug.DrawRay(raycastOrigins.topRight, Vector2.right * rayLengthLeftRight, Color.red);

		return hit;
	}

	private void updateRaycastOrigins()
	{
		Bounds bounds = bc.bounds;

		raycastOrigins.botLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.botRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}


	struct RaycastOrigins
	{
		public Vector2 botLeft, botRight, topLeft, topRight;
	}

	private void updateNearbyWalls()
	{

		//Debug.Log (collisionLeft() + ", " + collisionRight() + ", " + collisionUp() + ", " + collisionDown());


		nearbyWalls.nearLeft = collisionLeft();
		nearbyWalls.nearRight = collisionRight();
		nearbyWalls.nearTop = collisionUp();
		nearbyWalls.nearBot = collisionDown();
	}

	private bool colliding(){
		return nearbyWalls.nearLeft || nearbyWalls.nearRight || nearbyWalls.nearTop || nearbyWalls.nearBot;
	}

	private bool collidingLeftOrRight(){
		return nearbyWalls.nearLeft || nearbyWalls.nearRight;
	}

	struct NearbyWalls
	{
		public bool nearLeft, nearRight, nearTop, nearBot;
	}

}
