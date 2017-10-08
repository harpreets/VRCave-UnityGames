using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * SceneAppearanceRules extends AppearanceRules by making sure the platform section transitions aling up with any section changes
 */
public class PlatformAppearanceRules : AppearanceRules {
	
	private PlatformObject platformObject;
	
	public override void init()
	{
        base.init();

		platformObject = GetComponent<PlatformObject>();
	}

    // distance is the scene distance
    public override bool canSpawnObject(float distance, ObjectSpawnData spawnData)
	{
        if (!base.canSpawnObject(distance, spawnData))
			return false;

        // If section transition is true a transition object must be found
        if (spawnData.sectionTransition) {
            if (platformObject.sectionTransition) {
                // any transition is a section transition if there are no specific section transitions defined
                if (platformObject.fromSection.Count == 0) {
                    return true;
                }
                // return true if the from section equals the previous section and matches up with the to section which equals the current section
                // fromSection and toSection must be equal in size
                for (int i = 0; i < platformObject.fromSection.Count; ++i) {
                    if (platformObject.fromSection[i] == spawnData.prevSection && platformObject.toSection[i] == spawnData.section) {
                        return true;
                    }
                }
            }
            return false;
        }

        return !platformObject.sectionTransition;
	}
}
