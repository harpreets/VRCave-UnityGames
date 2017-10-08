using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
/*
 * A static class which will show the editor insector for the Platform Placement Rules
 */
public class PlatformPlacementRuleInspector : Editor {

    public static bool showPlatforms(ref List<PlatformPlacementRule> platformPlacementRules, bool linkedPlatform)
    {
        if (platformPlacementRules == null)
            return false;

        GUILayout.Label(string.Format("Platforms {0}", (linkedPlatform ? "Linked" : "Avoided")), "BoldLabel");
        if (platformPlacementRules.Count == 0) {
            GUILayout.Label(string.Format("No platforms {0}", (linkedPlatform ? "linked" : "avoided")));
        }

        PlatformPlacementRule platformPlacementRule;
        for (int i = 0; i < platformPlacementRules.Count; ++i) {
            platformPlacementRule = platformPlacementRules[i];

            // quick cleanup if the platform has gone null
            if (platformPlacementRule.platform == null) {
                platformPlacementRules.RemoveAt(i);
                return true;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("  {0}", platformPlacementRule.platform.name));
            if (GUILayout.Button("Remove")) {
                platformPlacementRules.RemoveAt(i);
                return true;
            }
            GUILayout.EndHorizontal();
        }

        return false;
    }

    public static int addPlatform(List<PlatformPlacementRule> platformPlacementRules, InfiniteObject platform, bool linkedPlatform)
    {
        // Make sure there aren't any duplicates
        for (int i = 0; i < platformPlacementRules.Count; ++i) {
            if (platformPlacementRules[i].platform == platform)
                return 2;
        }

        platformPlacementRules.Add(new PlatformPlacementRule(platform, linkedPlatform));
        return 0;
    }
}
