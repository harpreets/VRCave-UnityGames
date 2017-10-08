using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * If a power up hasn't been purchased yet then it cannot spawn.
 */
public class PowerUpAppearanceRules : CollidableAppearanceRules
{
    private PowerUpTypes powerUpType;
    private DataManager dataManager;
	
	public override void init()
	{
        base.init();

        dataManager = DataManager.instance;

        powerUpType = GetComponent<PowerUpObject>().powerUpType;
	}

    public override bool canSpawnObject(float distance, ObjectSpawnData spawnData)
	{
        if (dataManager.getPowerUpLevel(powerUpType) == 0)
            return false;

        if (!base.canSpawnObject(distance, spawnData))
			return false;

        return true;
	}
}
