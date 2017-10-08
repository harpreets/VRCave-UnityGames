using UnityEngine;
using System.Collections;

/**
 * After each run gameOver() will be called and the MissionManager will check the active missions to determine if they have been satisifed. If they are satisfied, the scoreMultiplier
 * is incremented by scoreMultiplierIncrement and that value is multiplied by the points to give you your final score.
 * 
 * ID                       Description
 * NoviceRunner             run for 500 points
 * CompetentRunner          run for 1500 points
 * ExpertRunner             run for 5000 points
 * RunnerComplete           running complete
 * NoviceCoinCollector      collect 50 coins
 * CompetentCoinCollector   collect 150 coins
 * ExpertCoinCollector      collect 500 coins
 * CoinCollectorComplete    coin collector complete
 * NovicePlayCount          play 5 games
 * CompetentPlayCount       play 15 games
 * ExpertPlayCount          play 50 games
 * PlayCountComplete        play count complete
 **/
public enum MissionType { NoviceRunner, CompetentRunner, ExpertRunner, RunnerComplete, NoviceCoinCollector, CompetentCoinCollector, ExpertCoinCollector, 
                          CoinCollectorComplete, NovicePlayCount, CompetentPlayCount, ExpertPlayCount, PlayCountComplete, None }
public class MissionManager : MonoBehaviour {

    static public MissionManager instance;

    // callback for any class that is interested when a mission is complete (such as the social manager)
    public delegate void MissionCompleteHandler(MissionType missionType);
    public event MissionCompleteHandler onMissionComplete;

    // The amount the score should be multiplied by each time a challenge is complete
    public float scoreMultiplierIncrement;

    private MissionType[] activeMissions;
    private float scoreMultiplier;

    private DataManager dataManager;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        dataManager = DataManager.instance;

        activeMissions = new MissionType[3]; // 3 active missions at a time
        scoreMultiplier = 1;
        for (int i = 0; i < activeMissions.Length; ++i) {
            activeMissions[i] = (MissionType)PlayerPrefs.GetInt(string.Format("Mission{0}", i), -1);
            // there are no active missions if the game hasn't been started yet
            if ((int)activeMissions[i] == -1) {
                activeMissions[i] = (MissionType)(i * 4); // 4 missions per set
            }
            scoreMultiplier += ((int)activeMissions[i] % 4) * scoreMultiplierIncrement;
        }
    }

    public void gameOver()
    {
        checkForCompletedMissions();
    }

    // loop through the active missions and determine if the previous run satisfied the mission requirements
    private void checkForCompletedMissions()
    {
        for (int i = 0; i < activeMissions.Length; ++i) {
            switch (activeMissions[i]) {
                case MissionType.NoviceRunner:
                case MissionType.CompetentRunner:
                case MissionType.ExpertRunner:
                    if (dataManager.getScore() > dataManager.getMissionGoal(activeMissions[i])) {
                        missionComplete(activeMissions[i]);
                    }
                    break;
                case MissionType.NoviceCoinCollector:
                case MissionType.CompetentCoinCollector:
                case MissionType.ExpertCoinCollector:
                    if (dataManager.getLevelCoins() > dataManager.getMissionGoal(activeMissions[i])) {
                        missionComplete(activeMissions[i]);
                    }
                    break;
                case MissionType.NovicePlayCount:
                case MissionType.CompetentPlayCount:
                case MissionType.ExpertPlayCount:
                    if (dataManager.getPlayCount() > dataManager.getMissionGoal(activeMissions[i])) {
                        missionComplete(activeMissions[i]);
                    }
                    break;
            }
        }
    }

    private void missionComplete(MissionType missionType)
    {
        int missionSet = (int)missionType / 4;
        if ((int)missionType % 3 != 0) { // don't increment if the player has already reached the max
            activeMissions[missionSet] = missionType + 1;
            scoreMultiplier += scoreMultiplierIncrement;
        }
        PlayerPrefs.SetInt(string.Format("Mission{0}", missionSet), (int)activeMissions[missionSet]);

        if (onMissionComplete != null) {
            onMissionComplete(missionType);
        }
    }

    public float getScoreMultiplier()
    {
        return scoreMultiplier;
    }

    public MissionType getMission(int mission)
    {
        return activeMissions[mission];
    }
}
