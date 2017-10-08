using UnityEngine;
using System.Collections;

/*
 * CoroutineData is a quick class used to save off any timinig information needed when the game is paused.
 */
public class CoroutineData { 
	public float startTime; 
	public float duration; 
	public CoroutineData() { startTime = 0; duration = 0; }
	public void calcuateNewDuration() { duration -= Time.time - startTime; }
}

/*
 * The game manager is a singleton which manages the game state. It coordinates with all of the other classes to tell them
 * when to start different game states such as pausing the game or ending the game.
 */
public enum GameOverType { Wall, JumpObstacle, DuckObstacle, Quit };
public class GameManager : MonoBehaviour {
	
	static public GameManager instance;
	
	public delegate void PauseHandler(bool paused);
	public event PauseHandler onPauseGame;

    public bool godMode;
	public bool showTutorial;

    private Character activeCharacter;
    private GameObject character;

    private bool gamePaused;
    private bool gameActive;
	private bool isPowerUpActive;
	
	private InfiniteObjectGenerator infiniteObjectGenerator;
	private PlayerController playerController;
	private GUIManager guiManager;
    private DataManager dataManager;
    private AudioManager audioManager;
    private PowerUpManager powerUpManager;
    private MissionManager missionManager;
    private InputController inputController;
    private CameraController cameraController;
//	private ProjectionCamerasController projectionCamerasController;
//	private ProjectionPortalWindowsController projectionPortalWindowsController;
	private TrackerData kinectTrackerData;
	
	public void Awake()
	{
		instance = this;	
	}
	
	public void Start ()
	{
		infiniteObjectGenerator = InfiniteObjectGenerator.instance;
		guiManager = GUIManager.instance;
		dataManager = DataManager.instance;
        audioManager = AudioManager.instance;
		powerUpManager = PowerUpManager.instance;
        missionManager = MissionManager.instance;
        inputController = InputController.instance;
        cameraController = CameraController.instance;
		kinectTrackerData = TrackerData.instance;
//		projectionCamerasController = ProjectionCamerasController.instance;
//		projectionPortalWindowsController = ProjectionPortalWindowsController.instance;
        activeCharacter = Character.None;
		
		startGame();
	}
	
	void OnLeveWasLoaded(int level){
		if(level == 1){
			startGame();
		}
	}

    private void spawnCharacter()
    {

        if (activeCharacter == dataManager.getSelectedCharacter()) {
            return;
        }

        if (character != null) {
            Destroy(character);
        }

        activeCharacter = dataManager.getSelectedCharacter();
        character = GameObject.Instantiate(dataManager.getCharacterPrefab(activeCharacter)) as GameObject;
			
		addTrackerTransforms();
		
        playerController = PlayerController.instance;
        playerController.init();
    }
	
	private void addTrackerTransforms(){
		TrackerPlayerController trackerPlayerControllerScript;		
		trackerPlayerControllerScript = character.GetComponent<TrackerPlayerController>();
		trackerPlayerControllerScript.head = kinectTrackerData.trackedHead;
		trackerPlayerControllerScript.leftFeet = kinectTrackerData.trackedFeet;
		trackerPlayerControllerScript.leftKnee = kinectTrackerData.trackedKnee;
	}
	
	public void startGame()
	{
        spawnCharacter();
        gameActive = true;
        inputController.startGame();
		dataManager.startGame();
		guiManager.showGUI(GUIState.InGame);
        audioManager.playBackgroundMusic(true);
//        cameraController.setPlayerAsParent(false);
        infiniteObjectGenerator.startGame();
		playerController.startGame();
		isPowerUpActive = false;
	}

    public void toggleTutorial()
    {
        showTutorial = !showTutorial;
        infiniteObjectGenerator.reset();
        if (showTutorial) {
            infiniteObjectGenerator.showStartupObjects(true);
        } else {
            // show the startup objects if there are any
            if (!infiniteObjectGenerator.showStartupObjects(false))
                infiniteObjectGenerator.spawnObjectRun(false);
        }
        infiniteObjectGenerator.readyFromReset();
    }
	
	public void obstacleCollision(ObstacleObject obstacle, Vector3 position)
	{
        if (!powerUpManager.isPowerUpActive(PowerUpTypes.Invincibility) && !godMode) {
            playerController.obstacleCollision(obstacle.getTransform(), position);
			dataManager.obstacleCollision();
            if (dataManager.getCollisionCount() == playerController.maxCollisions) {
                gameOver(obstacle.isJump ? GameOverType.JumpObstacle : GameOverType.DuckObstacle, true);
            } else {
                audioManager.playSoundEffect(SoundEffects.ObstacleCollisionSoundEffect);
            }
		}
	}
	
	public void coinCollected()
	{
        dataManager.addToCoins(1 * (powerUpManager.isPowerUpActive(PowerUpTypes.DoubleCoin) ? 2 : 1));
        audioManager.playSoundEffect(SoundEffects.CoinSoundEffect);
	}

    public void activatePowerUp(PowerUpTypes powerUpType, bool activate)
    {
        if (activate) {
            // deactivate the current power up (if a power up is active) and activate the new one
            powerUpManager.deactivatePowerUp();
            powerUpManager.activatePowerUp(powerUpType);
            audioManager.playSoundEffect(SoundEffects.PowerUpSoundEffect);
        }
        playerController.activatePowerUp(powerUpType, activate, dataManager.getPowerUpColor(powerUpType));
    }
	
	public void changeCharacter(int num){
//		Vector3 targetPosition = playerController.targetPosition;
//		float totalMoveDistance = playerController.totalMoveDistance;
		Destroy(character);
		activeCharacter = (Character)num;
		character = GameObject.Instantiate(dataManager.getCharacterPrefab(activeCharacter)) as GameObject;
		addTrackerTransforms();
		playerController = PlayerController.instance;
    	playerController.init();
//		playerController.targetPosition = targetPosition;
//		playerController.totalMoveDistance = totalMoveDistance;
		inputController.startGame();
//		playerController.deregisterPause();
		onPauseGame = null;
		playerController.startGame();
	}
	
	public void gameOver(GameOverType gameOverType, bool waitForFrame)
	{
        if (!gameActive && waitForFrame)
            return;
        gameActive = false;

        if (waitForFrame) {
            StartCoroutine(waitForFrameGameOver(gameOverType));
        } else {
            inputController.gameOver();
            // Mission Manager's gameOver must be called before the Data Manager's gameOver so the Data Manager can grab the 
            // score multiplier from the Mission Manager to determine the final score
            missionManager.gameOver();
            dataManager.gameOver();
            playerController.gameOver(gameOverType);
            audioManager.playBackgroundMusic(false);
            audioManager.playSoundEffect(SoundEffects.GameOverSoundEffect);
            guiManager.gameOver();
//            cameraController.setPlayerAsParent(false);
        }
	}
	
	// Game over may be called from a trigger so wait for the physics loop to end
    private IEnumerator waitForFrameGameOver(GameOverType gameOverType)
	{
		yield return new WaitForEndOfFrame();

        gameOver(gameOverType, false);

        // Wait a second for the ending animations to play
        yield return new WaitForSeconds(1.0f);

        guiManager.showGUI(GUIState.EndGame);
	}
	
	public void restartGame(bool start)
	{
        if (gamePaused) {
            if (onPauseGame != null)
                onPauseGame(false);
            gameOver(GameOverType.Quit, false);
        }

		dataManager.reset();
		infiniteObjectGenerator.reset();
		powerUpManager.reset();
		playerController.reset();
        cameraController.reset();
        if (showTutorial) {
            infiniteObjectGenerator.showStartupObjects(true);
        } else {
            // show the startup objects if there are any
            if (!infiniteObjectGenerator.showStartupObjects(false))
                infiniteObjectGenerator.spawnObjectRun(false);
        }
        infiniteObjectGenerator.readyFromReset();

        if (start)
            startGame();
	}

    public void backToMainMenu(bool restart)
	{
        if (gamePaused) {
            if (onPauseGame != null)
                onPauseGame(false);
            gameOver(GameOverType.Quit, false);
        }

        if (restart)
            restartGame(false);
		guiManager.showGUI(GUIState.MainMenu);
	}
	
	public void pauseGame(bool pause)
    {
        guiManager.showGUI(pause ? GUIState.Pause : GUIState.InGame);
        audioManager.playBackgroundMusic(!pause);
		if (onPauseGame != null)
            onPauseGame(pause);
        inputController.enabled = !pause;
        gamePaused = pause;
	}
	
	public void upgradePowerUp(PowerUpTypes powerUpType)
	{
        // Can't upgrade if the player can't afford the power up
        int cost = dataManager.getPowerUpCost(powerUpType);
        if (dataManager.getTotalCoins() < cost) {
            return;
        }
		dataManager.upgradePowerUp(powerUpType);
        dataManager.adjustTotalCoins(-cost);
		guiManager.refreshStoreGUI();
	}

    public void purchaseSelectCharacter(Character character)
    {
        int characterCost = dataManager.getCharacterCost(character);
        if (characterCost > 0) { // a character already bought will select that character for use
            if (dataManager.getSelectedCharacter() != character) {
                dataManager.setSelectedCharacter(character);
            }

            guiManager.refreshStoreGUI();
        } else if (characterCost <= dataManager.getTotalCoins()) {
            dataManager.purchaseCharacter(character);
            dataManager.setSelectedCharacter(character);
            dataManager.adjustTotalCoins(-characterCost);

            guiManager.refreshStoreGUI();
        }
    }
	
	public void OnApplicationPause(bool pause)
	{
        if (gamePaused)
            return;

		if (onPauseGame != null)
			onPauseGame(pause);
	}
}
