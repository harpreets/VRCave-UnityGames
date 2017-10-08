using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * Collidable Appearance Rules need everything the Appearance Rules do, plus an option to select the platforms to avoid
 */
[CustomEditor(typeof(CollidableAppearanceRules))]
public class CollidableAppearanceRulesInspector : AppearanceRulesInspector
{
    private PlatformObject targetPlatform = null;
    private bool addNewAvoidPlatform = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        CollidableAppearanceRules collidableAppearanceRules = (CollidableAppearanceRules)target;
        List<PlatformPlacementRule> platformPlacementRules = collidableAppearanceRules.avoidPlatforms;
        if (PlatformPlacementRuleInspector.showPlatforms(ref platformPlacementRules, false)) {
            collidableAppearanceRules.avoidPlatforms = platformPlacementRules;
            EditorUtility.SetDirty(target);
        }
        
        if (addNewAvoidPlatform) {
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
                if ((error = PlatformPlacementRuleInspector.addPlatform(platformPlacementRules, targetPlatform, false)) == 0) {
                    addNewAvoidPlatform = false;
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
                addNewAvoidPlatform = false;
            }
            GUILayout.EndHorizontal();
        }

        if (!addNewAvoidPlatform && GUILayout.Button("Add Avoid Platform")) {
            addError = "";
            addNewAvoidPlatform = true;
        }
    }
}
