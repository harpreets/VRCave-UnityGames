using UnityEngine;
using System.Collections;

/*
 * The power up manager is a singleton which manages the state of the power ups. 
 */
public enum PowerUpTypes { DoubleCoin, CoinMagnet, Invincibility, None }
public class PowerUpManager : MonoBehaviour {
	
	static public PowerUpManager instance;
	
	private PowerUpTypes activePowerUp;
	private CoroutineData activePowerUpData;

    private GameManager gameManager;
    private DataManager dataManager;
	
	public void Awake()
	{
		instance = this;
	}
	
	void Start ()
	{
        gameManager = GameManager.instance;
		dataManager = DataManager.instance;
		GameManager.instance.onPauseGame += gamePaused;	
	
		activePowerUp = PowerUpTypes.None;
		activePowerUpData = new CoroutineData();
	}
	
	public void reset()
	{
		if (activePowerUp != PowerUpTypes.None) {
			StopCoroutine("runPowerUp");
			deactivatePowerUp();	
		}
	}
	
	public bool isPowerUpActive(PowerUpTypes powerUpType)
	{
		return activePowerUp == powerUpType;
	}
	
	public void activatePowerUp(PowerUpTypes powerUpType)
	{
		activePowerUp = powerUpType;
		activePowerUpData.duration = dataManager.getPowerUpLength(powerUpType);
		StartCoroutine("runPowerUp");
	}
	
	private IEnumerator runPowerUp()
	{
		activePowerUpData.startTime = Time.time;
		yield return new WaitForSeconds(activePowerUpData.duration);

        deactivatePowerUp();
	}
	
	public void deactivatePowerUp()
	{
		if (activePowerUp == PowerUpTypes.None)
            return;

        // Be sure the coroutine is stopped, deactivate may be called before runPowerUp is finished
        StopCoroutine("runPowerUp");
        gameManager.activatePowerUp(activePowerUp, false);
		activePowerUp = PowerUpTypes.None;
		activePowerUpData.duration = 0;
	}
	
	private void gamePaused(bool paused)
	{
		if (activePowerUp != PowerUpTypes.None) {
			if (paused) {
				StopCoroutine("runPowerUp");
				activePowerUpData.calcuateNewDuration();
			} else {
                StartCoroutine("runPowerUp");
			}
		}
	}
}
