using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
	private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
	private int level = 1;                                  //Current level number, expressed in game as "Day 1".



	private float levelStartDelay = 2f;
	private Text levelText;
	private Text restartText;
	private Button restartButton;
	private GameObject levelImage;
	public bool resetPlayer;

	public float timer = 60.0f;

	//Awake is always called before any Start functions
	void Awake()
	{
		//Check if instance already exists
		if (instance == null)

			//if not, set instance to this
			instance = this;

		//If instance already exists and it's not this:
		else if (instance != this)

			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy(gameObject);    

		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);

		//Get a component reference to the attached BoardManager script
		boardScript = GetComponent<BoardManager>();

		//Call the InitGame function to initialize the first level 
		//InitGame();
	}

	//Initializes the game for each level.
	void InitGame()
	{
		//doingSetup = true;
		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		restartButton = GameObject.Find ("RestartButton").GetComponent<Button> ();
		restartText = GameObject.Find ("RestartText").GetComponent<Text> ();
		restartText.enabled = false;
		restartButton.enabled = false;
		levelText.text = "Level " + level;
		Invoke ("HideLevelImage", levelStartDelay);

		//Call the SetupScene function of the BoardManager script, pass it current level number.
		boardScript.SetupScene(level);
	}

	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);

		//Set doingSetup to false allowing player to move again.
		//doingSetup = false;
	}

	//This is called each time a scene is loaded.
	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		//Call InitGame to initialize our level.
		InitGame();
		//Add one to our level number.
		level++;
	}
	void OnEnable()
	{
		//Tell our ‘OnLevelFinishedLoading’ function to start listening for a scene change event as soon as this script is enabled.
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}
	void OnDisable()
	{
		//Tell our ‘OnLevelFinishedLoading’ function to stop listening for a scene change event as soon as this script is disabled. 
			//Remember to always have an unsubscription for every delegate you subscribe to!
			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	public void GameOver(){
		SoundManager.instance.musicSource.Stop ();

		//Set levelText to display number of levels passed and game over message
		levelText.text = "You ran out of time at level " + (level-1) + ".";

		restartText.enabled = true;
		restartButton.enabled = true;
		restartButton.onClick.AddListener (TaskOnClick);

		//Enable black background image gameObject.
		levelImage.SetActive(true);


		//Disable this GameManager.
		//enabled = false;
	}

	void TaskOnClick(){
		level = 1;
		timer = 60;
		SoundManager.instance.musicSource.Play ();
		levelImage.SetActive (false);
		resetPlayer = true;
	}
}
