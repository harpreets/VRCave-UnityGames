using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Infinite Object Generator inspector editor - mainly used to show the custom Distance Value List inspector
 */
[CustomEditor(typeof(InfiniteObjectGenerator))]
public class InfiniteObjectGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InfiniteObjectGenerator infiniteObjectGenerator = (InfiniteObjectGenerator)target;
        GUILayout.Label("No Collidable Probabilities", "BoldLabel");
        DistanceValueList distanceProbabilityList = infiniteObjectGenerator.noCollidableProbability;
        if (DistanceValueListInspector.showLoopToggle(ref distanceProbabilityList, DistanceValueType.Probability)) {
            infiniteObjectGenerator.noCollidableProbability = distanceProbabilityList;
            EditorUtility.SetDirty(target);
        }
        DistanceValueListInspector.showDistanceValues(ref distanceProbabilityList, DistanceValueType.Probability);

        if (DistanceValueListInspector.showAddNewValue(ref distanceProbabilityList, DistanceValueType.Probability)) {
            infiniteObjectGenerator.noCollidableProbability = distanceProbabilityList;
            EditorUtility.SetDirty(target);
        }
    }
}
