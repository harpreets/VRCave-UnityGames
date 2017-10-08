using UnityEngine;
using System.Collections;

/*
 * Static data is a singleton class which holds any data which is not directly game related, such as the power up length and cost
 */
public class StaticData : MonoBehaviour {

	static public StaticData instance;
	
	public int totalPowerUpLevels;
	public float[] powerUpLength;
    public int[] powerUpCost;
    public Color[] powerUpColor;

    public int[] characterCost;
    public GameObject[] characterPrefab;

    public string[] missionDescription;
    public int[] missionGoal;
	
	public void Awake()
	{
		instance = this;	
	}
	
	public float getPowerUpLength(PowerUpTypes powerUpType, int level)
	{
        return powerUpLength[((int)powerUpType * (totalPowerUpLevels + 1)) + level];
	}
	
	public int getPowerUpCost(PowerUpTypes powerUpType, int level)
	{
		return powerUpCost[((int)powerUpType * totalPowerUpLevels) + level];
	}

    public Color getPowerUpColor(PowerUpTypes powerUpType)
    {
        return powerUpColor[(int)powerUpType];
    }
	
	public int getTotalPowerUpLevels()
	{
		return totalPowerUpLevels;
	}

    public int getCharacterCost(Character character)
    {
        return characterCost[(int)character];
    }

    public GameObject getCharacterPrefab(Character character)
    {
        return characterPrefab[(int)character];
    }

    public string getMissionDescription(MissionType missionType)
    {
        return missionDescription[(int)missionType];
    }

    public int getMissionGoal(MissionType missionType)
    {
        return missionGoal[(int)missionType];
    }
}
