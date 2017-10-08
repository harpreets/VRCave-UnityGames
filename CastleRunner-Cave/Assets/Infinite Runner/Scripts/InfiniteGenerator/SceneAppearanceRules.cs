using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * SceneAppearanceRules extends AppearanceRules by checking to see if a scene can fit within the space provided. A scene may not be able to fit if for example
 * there is a turn 10m away and the scene object is 20m in length. This rule also checks for section transitions
 */
public class SceneAppearanceRules : AppearanceRules {

    // A list of platforms that the scene object must spawn near. A size of 0 means it can spawn near any platform
    public List<PlatformPlacementRule> linkedPlatforms;

    private InfiniteObjectManager infiniteObjectManager;
    private float platformSceneWidthBuffer;
	private float zSize;

	public void init(float buffer, float size)
    {
        infiniteObjectManager = InfiniteObjectManager.instance;

		platformSceneWidthBuffer = buffer;
		zSize = size;
	}

    public override void assignIndexToObject(InfiniteObject infiniteObject, int index)
    {
        base.assignIndexToObject(infiniteObject, index);

        for (int i = 0; i < linkedPlatforms.Count; ++i) {
            if (linkedPlatforms[i].assignIndexToObject(infiniteObject, index)) {
                (infiniteObject as PlatformObject).enableLinkedSceneObjectRequired();
                break;
            }
        }
    }

    // distance is the scene distance
    public override bool canSpawnObject(float distance, ObjectSpawnData spawnData)
	{
        if (!base.canSpawnObject(distance, spawnData))
			return false;

        int platformLocalIndex = infiniteObjectHistory.getFirstPlatformIndex();
        if (platformLocalIndex == -1)
            return false;

        // Get the platform that this scene is going to spawn next to. See if the platform requires linked scenes and if it does, if this scene fulfills that requirement.
        PlatformObject platform = infiniteObjectManager.localIndexToInfiniteObject(platformLocalIndex, ObjectType.Platform) as PlatformObject;
        if (platform.linkedSceneObjectRequired()) {
            for (int i = 0; i < linkedPlatforms.Count; ++i) {
                if (linkedPlatforms[i].canSpawnObject(platformLocalIndex)) {
                    return true;
                }
            }
            return false;
        } else if (linkedPlatforms.Count > 0) { // return false if this scene is linked to a platform but the platform doesn't have any linked scenes
            return false;
        }

		// if the platform can't fit, then don't spawn it
		float totalDistance = infiniteObjectHistory.getTotalDistance(false);
        float largestScene = spawnData.largestScene; 
        float sceneBuffer = (spawnData.useWidthBuffer ? platformSceneWidthBuffer : 0); // useWidthBuffer contains the information if we should spawn up to totalDistance
        
        if (totalDistance - distance - sceneBuffer - largestScene >= 0) {
			// largest scene of 0 means we are approaching a turn and it doesn't matter what size object is spawned as long as it fits
            if (largestScene == 0) {
                return totalDistance - distance - sceneBuffer >= zSize;
            } else {
                return largestScene >= zSize;
            }
		}

        return false;
	}
}
