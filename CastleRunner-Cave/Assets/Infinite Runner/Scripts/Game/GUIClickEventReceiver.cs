using UnityEngine;
using System.Collections;

/*
 * The user pressed a button, perform some action
 */
public enum ClickType { StartGame, Stats, Store, DoubleCoinUpgrade, CoinMagnetUpgrade, InvincibilityUpgrade, EndGame, Restart, MainMenu, 
                        MainMenuRestart, Pause, Resume, ToggleTutorial, Missions, PowerUps, Characters, Character1Select, Character2Select }
public class GUIClickEventReceiver : MonoBehaviour {
	
	public ClickType clickType;
	
	public void OnClick()
	{
        bool playSoundEffect = true;
		switch (clickType) {
		case ClickType.StartGame:
			GameManager.instance.startGame();
			break;
		case ClickType.Store:
			GUIManager.instance.showGUI(GUIState.Store);
			break;
		case ClickType.Stats:
			GUIManager.instance.showGUI(GUIState.Stats);
			break;
		case ClickType.DoubleCoinUpgrade:
			GameManager.instance.upgradePowerUp(PowerUpTypes.DoubleCoin);
			break;
		case ClickType.CoinMagnetUpgrade:
			GameManager.instance.upgradePowerUp(PowerUpTypes.CoinMagnet);
			break;
		case ClickType.InvincibilityUpgrade:
			GameManager.instance.upgradePowerUp(PowerUpTypes.Invincibility);
			break;
		case ClickType.EndGame:
			GUIManager.instance.showGUI(GUIState.EndGame);
			break;
		case ClickType.Restart:
			GameManager.instance.restartGame(true);
            break;
        case ClickType.MainMenu:
            GameManager.instance.backToMainMenu(false);
            break;
		case ClickType.MainMenuRestart:
			GameManager.instance.backToMainMenu(true);
			break;
		case ClickType.Pause:
			GameManager.instance.pauseGame(true);
            playSoundEffect = false;
			break;
		case ClickType.Resume:
			GameManager.instance.pauseGame(false);
			break;
        case ClickType.ToggleTutorial:
            GameManager.instance.toggleTutorial();
            break;
        case ClickType.Missions:
            GUIManager.instance.showGUI(GUIState.Missions);
            break;
        case ClickType.PowerUps:
            GUIManager.instance.showGUI(GUIState.PowerUps);
            break;
        case ClickType.Characters:
            GUIManager.instance.showGUI(GUIState.Characters);
            break;
        case ClickType.Character1Select:
            GameManager.instance.purchaseSelectCharacter(Character.Character1);
            break;
        case ClickType.Character2Select:
            GameManager.instance.purchaseSelectCharacter(Character.Character2);
            break;
		}

        if (playSoundEffect)
            AudioManager.instance.playSoundEffect(SoundEffects.GUITapSoundEffect);
	}
}
