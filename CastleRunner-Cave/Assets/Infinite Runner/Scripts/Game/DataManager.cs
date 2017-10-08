using UnityEngine;
using System.Collections;

/*
 * The data manager is a singleton which manages the data across games. It will persist any permanent data such as the
 * total number of coins or power up level
 */
public class DataManager : MonoBehaviour {
	
	static public DataManager instance;
	
	public float scoreMult;
	
	private float score;
	private int totalCoins;
	private int levelCoins;
//	private int powerupCoins;
	private int collisions;

	private int[] currentPowerupLevel;
	
	private GUIManager guiManager;
    private SocialManager socialManager;
    private MissionManager missionManager;
	private StaticData staticData;
//	public bool zorbModeActive;
	
	public void Awake()
	{
		instance = this;
	}
	
	public void Start()
	{	
		guiManager = GUIManager.instance;
        socialManager = SocialManager.instance;
        missionManager = MissionManager.instance;
		staticData = StaticData.instance;
		
		score = 0;
		levelCoins = 0;
		collisions = 0;
		totalCoins = PlayerPrefs.GetInt("Coins", 0);
		
		currentPowerupLevel = new int[(int)PowerUpTypes.None];
		for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
			currentPowerupLevel[i] = PlayerPrefs.GetInt(string.Format("PowerUp{0}",i), 0);
        }

        // first character is always available
        if (getCharacterCost(Character.Character1) > 0)
            purchaseCharacter(Character.Character1);
	}
	
	public void startGame()
	{
        StartCoroutine("scoreTimer");
        GameManager.instance.onPauseGame += onPauseGame;
	}
	
	private IEnumerator scoreTimer()
	{
		WaitForSeconds scoreWait = new WaitForSeconds(0.1f);
		
		float prevTime = Time.time;
		while (true) {
			yield return scoreWait;
			score += (Time.time - prevTime) * scoreMult;
			guiManager.setInGameScore(Mathf.RoundToInt(score));
			prevTime = Time.time;
		}
	}
	
	public int getScore()
	{
        return Mathf.RoundToInt(score * missionManager.getScoreMultiplier());	
	}
	
	public void obstacleCollision()
	{
//		if(!zorbModeActive)
			collisions++;
	}
	
	public int getCollisionCount()
	{
		return collisions;	
	}
	
	public void addToCoins(int amount)
	{
		levelCoins += amount;
//		powerupCoins += amount;
		guiManager.setInGameCoinCount(levelCoins);
//		guiManager.setInGameZorbBarStatus();
	}
	
	public int getLevelCoins()
	{
		return levelCoins;	
	}
	
//	public int getPowerupCoins()
//	{
//		return powerupCoins;	
//	}
	
//	public void resetPowerupCoins()
//	{
//		powerupCoins = 0;	
//	}

    public void adjustTotalCoins(int amount)
    {
        totalCoins += amount;
        PlayerPrefs.SetInt("Coins", totalCoins);
    }
	
	public int getTotalCoins()
	{
		return totalCoins;
	}
	
	public int getHighScore()
	{
		return PlayerPrefs.GetInt("HighScore", 0);
	}
	
	public int getPlayCount()
	{
		return PlayerPrefs.GetInt("PlayCount");	
	}
	
	public void gameOver()
    {
        StopCoroutine("scoreTimer");
        GameManager.instance.onPauseGame -= onPauseGame;

		// save the high score, coin count, and play count
		if (getScore() > getHighScore()) {
			PlayerPrefs.SetInt("HighScore", getScore());
            socialManager.recordScore(getScore());
		}
		
		totalCoins += levelCoins;
		PlayerPrefs.SetInt("Coins", totalCoins);
		
		int playCount = PlayerPrefs.GetInt("PlayCount", 0);
		playCount++;
        PlayerPrefs.SetInt("PlayCount", playCount);
	}
	
	public void reset()
	{
		score = 0;
		levelCoins = 0;
		collisions = 0;
		
		guiManager.setInGameScore(Mathf.RoundToInt(score));
		guiManager.setInGameCoinCount(levelCoins);
//		guiManager.setInGameZorbBarStatus();
	}
	
	public int getPowerUpLevel(PowerUpTypes powerUpTypes)
	{
		return currentPowerupLevel[(int)powerUpTypes];	
	}
	
	public float getPowerUpLength(PowerUpTypes powerUpType)
	{
		return staticData.getPowerUpLength(powerUpType, currentPowerupLevel[(int)powerUpType]);
	}
	
	public int getPowerUpCost(PowerUpTypes powerUpType)
	{
		if (currentPowerupLevel[(int)powerUpType] < staticData.getTotalPowerUpLevels()) {
			return staticData.getPowerUpCost(powerUpType, currentPowerupLevel[(int)powerUpType]);
		}
		return -1; // out of power up upgrades
	}

    public Color getPowerUpColor(PowerUpTypes powerUpType)
    {
        return staticData.getPowerUpColor(powerUpType);
    }
	
	public void upgradePowerUp(PowerUpTypes powerUpType)
	{
		currentPowerupLevel[(int)powerUpType]++;
		PlayerPrefs.SetInt(string.Format("PowerUp{0}",(int)powerUpType), currentPowerupLevel[(int)powerUpType]);
	}

    public int getCharacterCost(Character character)
    {
        if (PlayerPrefs.GetInt(string.Format("CharacterPurchased{0}", (int)character), 0) == 1)
            return 0; // no cost if the character is already purchased
        return staticData.getCharacterCost(character);
    }

    public void purchaseCharacter(Character character)
    {
        PlayerPrefs.SetInt(string.Format("CharacterPurchased{0}", (int)character), 1);
    }

    public void setSelectedCharacter(Character character)
    {
        if (PlayerPrefs.GetInt(string.Format("CharacterPurchased{0}", (int)character), 0) == 1) {
            PlayerPrefs.SetInt("SelectedCharacter", (int)character);
        }
    }

    public Character getSelectedCharacter()
    {
        return (Character)PlayerPrefs.GetInt("SelectedCharacter", 0);
    }

    public GameObject getCharacterPrefab(Character character)
    {
        return staticData.getCharacterPrefab(character);
    }

    public int getMissionGoal(MissionType missionType)
    {
        return staticData.getMissionGoal(missionType);
    }

    public string getMissionDescription(MissionType missionType)
    {
        return staticData.getMissionDescription(missionType);
    }

    public void onPauseGame(bool paused)
    {
        if (paused) {
            StopCoroutine("scoreTimer");
        } else {
            StartCoroutine("scoreTimer");
        }
    }
}
