using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * The CollidableAppearanceRules extends AppearanceRules by checking to see if the collidable object can spawn on top of a platform.
 */
public class CollidableAppearanceRules : AppearanceRules {
	
	// platforms in which the object cannot spawn over
	public List<PlatformPlacementRule> avoidPlatforms;
	
	public override void assignIndexToObject(InfiniteObject infiniteObject, int index)
	{
		base.assignIndexToObject(infiniteObject, index);

        for (int i = 0; i < avoidPlatforms.Count; ++i) {
            if (avoidPlatforms[i].assignIndexToObject(infiniteObject, index))
				break;
		}
	}

    public override bool canSpawnObject(float distance, ObjectSpawnData spawnData)
	{
        if (!base.canSpawnObject(distance, spawnData))
			return false;

        for (int i = 0; i < avoidPlatforms.Count; ++i) {
            if (!avoidPlatforms[i].canSpawnObject(infiniteObjectHistory.getLastLocalIndex(ObjectType.Platform)))
				return false;
		}
		
		return true;
	}
}
