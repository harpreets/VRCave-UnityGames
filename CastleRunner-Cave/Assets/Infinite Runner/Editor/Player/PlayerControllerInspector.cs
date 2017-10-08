using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Player Controller inspector editor - mainly used to show the custom Distance Value List inspector
 */
[CustomEditor(typeof(PlayerController))]
public class PlayerControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerController playerController = (PlayerController)target;

        GUILayout.Label("Forward Speeds", "BoldLabel");
        DistanceValueList forwardSpeedList = playerController.forwardSpeeds;
        if (DistanceValueListInspector.showLoopToggle(ref forwardSpeedList, DistanceValueType.Speed)) {
            playerController.forwardSpeeds = forwardSpeedList;
            EditorUtility.SetDirty(target);
        }
        DistanceValueListInspector.showDistanceValues(ref forwardSpeedList, DistanceValueType.Speed);

        if (DistanceValueListInspector.showAddNewValue(ref forwardSpeedList, DistanceValueType.Speed)) {
            playerController.forwardSpeeds = forwardSpeedList;
            EditorUtility.SetDirty(target);
        }
    }
}
