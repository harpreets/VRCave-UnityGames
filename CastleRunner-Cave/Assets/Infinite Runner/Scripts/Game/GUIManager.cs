using UnityEngine;
using System.Collections;

/*
 * The GUI manager is a singleton class which manages the NGUI objects
 */
public enum GUIState { MainMenu, InGame, EndGame, Store, PowerUps, Characters, Stats, Pause, Tutorial, Missions, Inactive }
public enum TutorialType { Jump, Slide, Strafe, Attack, Turn, GoodLuck }
public class GUIManager : MonoBehaviour {
	
	static public GUIManager instance;
	
	public GameObject mainMenuPanel;
	public GameObject inGamePanel;
	public GameObject endGamePanel;
    public GameObject storePanel;
    public GameObject powerUpsPanel;
    public GameObject charactersPanel;
    public GameObject statsPanel;
    public GameObject missionsPanel;
    public GameObject pausePanel;
    public GameObject tutorialPanel;
	
	// in game:
	public UILabel inGameScore;
	public UILabel inGameCoins;
	
    // pause:
    public UILabel pauseScore;
    public UILabel pauseCoins;

	// end game:
	public UILabel endGameScore;
    public UILabel endGameCoins;
    public UILabel endGameMultiplier;

    // store:
    public GUIClickEventReceiver storeBackButtonReceiver;

    // power ups:
    public UILabel powerUpsTotalCoins;
    public UILabel powerUpsCoinDoublerCost;
    public GameObject powerUpsCoinDoublerGroup;
    public UILabel powerUpsCoinMagnetCost;
    public GameObject powerUpsCoinMagnetGroup;
    public UILabel powerUpsInvincibilityCost;
    public GameObject powerUpsInvincibilityGroup;

    // characters:
    public UILabel charactersTotalCoins;
    public UILabel character1ButtonTitle;
    public UILabel character2ButtonTitle;
    public UILabel character2Cost;

	// stats:
	public UILabel statsHighScore;
	public UILabel statsCoins;
	public UILabel statsPlayCount;

    // tutorial:
    public UILabel tutorialLabel;

    // missions:
    public UILabel missionsScoreMultiplier;
    public UILabel missionsActiveMission1;
    public UILabel missionsActiveMission2;
    public UILabel missionsActiveMission3;
    public GUIClickEventReceiver missionsBackButtonReceiver;
	
	private GUIState guiState;
	
	private DataManager dataManager;
    private MissionManager missionManager;

	public void Awake()
	{
		instance = this;	
	}
	
	public void Start ()
	{
		dataManager = DataManager.instance;
        missionManager = MissionManager.instance;
		
		guiState = GUIState.MainMenu;
		
		// hide everything except the main menu
#if UNITY_3_5
		mainMenuPanel.SetActiveRecursively(true);
		inGamePanel.SetActiveRecursively(false);
		endGamePanel.SetActiveRecursively(false);
        storePanel.SetActiveRecursively(false);
        powerUpsPanel.SetActiveRecursively(false);
        charactersPanel.SetActiveRecursively(false);
		statsPanel.SetActiveRecursively(false);
        missionsPanel.SetActiveRecursively(false);
        pausePanel.SetActiveRecursively(false);
        tutorialPanel.SetActiveRecursively(false);
#else
        activeRecursively(mainMenuPanel.transform, true);
        activeRecursively(inGamePanel.transform, false);
        activeRecursively(inGamePanel.transform, false);
        activeRecursively(endGamePanel.transform, false);
        activeRecursively(storePanel.transform, false);
        activeRecursively(powerUpsPanel.transform, false);
        activeRecursively(charactersPanel.transform, false);
        activeRecursively(statsPanel.transform, false);
        activeRecursively(missionsPanel.transform, false);
        activeRecursively(pausePanel.transform, false);
        activeRecursively(tutorialPanel.transform, false);
#endif
    }

    private void changeGUIState(bool activate, GUIState state)
    {
        GameObject activePanel = panelFromState(state);

        if (activePanel != null) {
#if UNITY_3_5
            activePanel.SetActiveRecursively(activate);
#else
            activeRecursively(activePanel.transform, activate);
#endif
        }
    }

#if !UNITY_3_5
    private void activeRecursively(Transform obj, bool active)
    {
        foreach (Transform child in obj) {
            activeRecursively(child, active);
        }
        obj.gameObject.SetActive(active);
    }
#endif
	
	private GameObject panelFromState(GUIState state)
	{
		switch (state) {
		case GUIState.MainMenu:
			return mainMenuPanel;
		case GUIState.InGame:
			return inGamePanel;
		case GUIState.EndGame:
			return endGamePanel;
		case GUIState.Store:
            return storePanel;
        case GUIState.PowerUps:
            return powerUpsPanel;
        case GUIState.Characters:
            return charactersPanel;
		case GUIState.Stats:
			return statsPanel;
		case GUIState.Pause:
            return pausePanel;
        case GUIState.Tutorial:
            return tutorialPanel;
        case GUIState.Missions:
            return missionsPanel;
		}
		return null; // how'd we get here?
	}
	
	public void showGUI(GUIState state)
	{
		// activate the new gui state, deactivate the old.
		changeGUIState(true, state);
		changeGUIState(false, guiState);
		
		switch (state) {
		case GUIState.EndGame:
			endGameScore.text = dataManager.getScore().ToString();
			endGameCoins.text = dataManager.getLevelCoins().ToString();
            endGameMultiplier.text = string.Format("{0}x", missionManager.getScoreMultiplier());
			break;
			
		case GUIState.Store:			
			// go back to the correct menu that we came from
			if (guiState == GUIState.MainMenu) {
				storeBackButtonReceiver.clickType = ClickType.MainMenu;
			} else if (guiState == GUIState.EndGame) {
				storeBackButtonReceiver.clickType = ClickType.EndGame;
			}
			break;

        case GUIState.Pause:
			pauseScore.text = dataManager.getScore().ToString();
			pauseCoins.text = dataManager.getLevelCoins().ToString();
            break;
			
		case GUIState.Stats:
			statsHighScore.text = dataManager.getHighScore().ToString();
			statsCoins.text = dataManager.getTotalCoins().ToString();
			statsPlayCount.text = dataManager.getPlayCount().ToString();
			break;

        case GUIState.Missions:
            if (guiState == GUIState.MainMenu) {
                missionsBackButtonReceiver.clickType = ClickType.MainMenu;
            } else { // coming from GUIState.EndGame
                missionsBackButtonReceiver.clickType = ClickType.EndGame;
            }
            missionsScoreMultiplier.text = string.Format("{0}x", missionManager.getScoreMultiplier());
            missionsActiveMission1.text = dataManager.getMissionDescription(missionManager.getMission(0));
            missionsActiveMission2.text = dataManager.getMissionDescription(missionManager.getMission(1));
            missionsActiveMission3.text = dataManager.getMissionDescription(missionManager.getMission(2));

            break;

        case GUIState.PowerUps:
        case GUIState.Characters:
            guiState = state;
            refreshStoreGUI();
            break;
        }
		
		guiState = state;
	}
	
	public void setInGameScore(int score)
	{
		inGameScore.text = score.ToString();
	}
	
	public void setInGameCoinCount(int coins)
	{
		inGameCoins.text = coins.ToString();
	}

    public void showTutorial(bool show, TutorialType tutorial)
    {
        changeGUIState(show, GUIState.Tutorial);
        if (show) {
            switch (tutorial) {
                case TutorialType.Jump:
#if (UNITY_IPHONE || UNITY_ANDROID)
                    tutorialLabel.text = "Swipe up to jump";
#else
                    tutorialLabel.text = "Press the up arrow to jump";
#endif
                    break;
                case TutorialType.Slide:
#if (UNITY_IPHONE || UNITY_ANDROID)
                    tutorialLabel.text = "Swipe down to slide";
#else
                    tutorialLabel.text = "Press the down arrow to slide";
#endif
                    break;
                case TutorialType.Strafe:
#if (UNITY_IPHONE || UNITY_ANDROID)
                    tutorialLabel.text = "Tilt to turn";
#else
                    tutorialLabel.text = "Move the mouse to collect coins";
#endif
                    break;
                case TutorialType.Attack:
#if (UNITY_IPHONE || UNITY_ANDROID)
                    tutorialLabel.text = "Tap to attack";
#else
                    tutorialLabel.text = "Attack with the left mouse button";
#endif
                    break;
                case TutorialType.Turn:
#if (UNITY_IPHONE || UNITY_ANDROID)
                    tutorialLabel.text = "Swipe left or right to turn";
#else
                    tutorialLabel.text = "Press the left or right arrows to turn";
#endif
                    break;
                case TutorialType.GoodLuck:
                    tutorialLabel.text = "Good luck!";
                    break;
            }
        }
    }
	
	public void refreshStoreGUI()
	{
        if (guiState == GUIState.PowerUps) {
            powerUpsTotalCoins.text = dataManager.getTotalCoins().ToString();

            int cost = dataManager.getPowerUpCost(PowerUpTypes.DoubleCoin);
            if (cost != -1) {
                powerUpsCoinDoublerCost.text = string.Format("{0} Coins", cost);
            } else {
#if UNITY_3_5
                powerUpsCoinDoublerGroup.gameObject.SetActiveRecursively(false);
#else
                powerUpsCoinDoublerGroup.SetActive(false);
#endif
            }

            cost = dataManager.getPowerUpCost(PowerUpTypes.CoinMagnet);
            if (cost != -1) {
                powerUpsCoinMagnetCost.text = string.Format("{0} Coins", cost);
            } else {
#if UNITY_3_5
                powerUpsCoinMagnetGroup.SetActiveRecursively(false);
#else
                powerUpsCoinMagnetGroup.SetActive(false);
#endif
            }

            cost = dataManager.getPowerUpCost(PowerUpTypes.Invincibility);
            if (cost != -1) {
                powerUpsInvincibilityCost.text = string.Format("{0} Coins", cost);
            } else {
#if UNITY_3_5
                powerUpsInvincibilityGroup.SetActiveRecursively(false);
#else
                powerUpsInvincibilityGroup.SetActive(false);
#endif
            }
        } else { // characters
            charactersTotalCoins.text = dataManager.getTotalCoins().ToString();

            int cost = dataManager.getCharacterCost(Character.Character2);
            if (cost > 0) {
                character2Cost.text = string.Format("{0} Coins", cost);
                character2ButtonTitle.text = "Purchase";
                character1ButtonTitle.text = "Selected";
            } else {
                if (dataManager.getSelectedCharacter() == Character.Character1) {
                    character1ButtonTitle.text = "Selected";
                    character2ButtonTitle.text = "Select";
                } else {
                    character1ButtonTitle.text = "Select";
                    character2ButtonTitle.text = "Selected";
                }
#if UNITY_3_5
                character2Cost.gameObject.SetActiveRecursively(false);
#else
                character2Cost.gameObject.SetActive(false);
#endif
            }
        }
	}

    public void gameOver()
    {
#if UNITY_3_5
        if (tutorialPanel.active) {
            tutorialPanel.active = false;
        }
#else
		if (tutorialPanel.activeSelf) {
            tutorialPanel.SetActive(false);
        }
#endif
    }
}
