using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Custom editor insepectors don't support inheritance.. get around that by subclassing
 */
[CustomEditor(typeof(SceneAppearanceRules))]
public class SceneAppearanceRulesInspector : AppearanceRulesInspector
{
    private PlatformObject targetPlatform = null;
    private bool addNewLinkedPlatform = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        SceneAppearanceRules sceneAppearanceRules = (SceneAppearanceRules)target;
        List<PlatformPlacementRule> platformPlacementRules = sceneAppearanceRules.linkedPlatforms;
        if (PlatformPlacementRuleInspector.showPlatforms(ref platformPlacementRules, true)) {
            sceneAppearanceRules.linkedPlatforms = platformPlacementRules;
            EditorUtility.SetDirty(target);
        }

        if (addNewLinkedPlatform) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Platform");
            targetPlatform = EditorGUILayout.ObjectField(targetPlatform, typeof(PlatformObject), false) as PlatformObject;
            GUILayout.EndHorizontal();

            if (addError.Length > 0) {
                GUI.contentColor = Color.red;
                GUILayout.Label(addError);
                GUI.contentColor = Color.white;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add")) {
                int error;
                if ((error = PlatformPlacementRuleInspector.addPlatform(platformPlacementRules, targetPlatform, true)) == 0) {
                    addNewLinkedPlatform = false;
                    EditorUtility.SetDirty(target);
                } else {
                    switch (error) {
                        case 1:
                            addError = "Error: Target Platform is not set";
                            break;
                        case 2:
                            addError = "Error: Target Platform has already been added";
                            break;
                        default:
                            addError = "Unknown Error";
                            break;
                    }
                }
            }

            if (GUILayout.Button("Cancel")) {
                addNewLinkedPlatform = false;
            }
            GUILayout.EndHorizontal();
        }

        if (!addNewLinkedPlatform && GUILayout.Button("Add Linked Platform")) {
            addError = "";
            addNewLinkedPlatform = true;
        }
    }
}
