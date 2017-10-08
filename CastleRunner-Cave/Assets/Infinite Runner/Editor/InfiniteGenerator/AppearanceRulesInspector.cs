using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * This editor script will allow you to quickly add appearance rules while ensuring the values work within the game
 */
[CustomEditor(typeof(AppearanceRules))]
public class AppearanceRulesInspector : Editor
{
    private bool addNewRule = false;
    private bool avoidObjectRule = true;
    private InfiniteObject targetObject = null;
    private int minDistance = 0;
    private bool minDistanceSameObjectType = false;
    private int minObjectSeperation = 0;
    private bool useEndDistance = false;
    private int startDistance = 0;
    private float startProbability = 1;
    private int endDistance = 0;
    private float endProbability = 1;
    protected string addError = "";
    private string[] ruleTypeStrings = new string[] { "Avoid Object", "Probability Adj" };

    public override void OnInspectorGUI()
    {
        List<ObjectRuleMap> appearanceRules = ((AppearanceRules)target).avoidObjectRuleMaps;
        if (appearanceRules != null && appearanceRules.Count > 0) {
            GUILayout.Label("Avoid Object Rules", "BoldLabel");
            showAppearanceRules(appearanceRules, true);
        }

        appearanceRules = ((AppearanceRules)target).probabilityAdjustmentMaps;
        if (appearanceRules != null && appearanceRules.Count > 0) {
            GUILayout.Label("Probability Adjustment Rules", "BoldLabel");
            showAppearanceRules(appearanceRules, false);
        }

        if (addNewRule) {
            avoidObjectRule = GUILayout.SelectionGrid((avoidObjectRule ? 0 : 1), ruleTypeStrings, 2) == 0;

            minDistanceSameObjectType = GUILayout.Toggle(minDistanceSameObjectType, "Use Min Distance with same Object Type");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Min Distance", GUILayout.Width(150));
            string stringValue = GUILayout.TextField(minDistance.ToString());
            try {
                minDistance = int.Parse(stringValue);
            } catch (Exception) { }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Min Object Separation", GUILayout.Width(150));
            stringValue = GUILayout.TextField(minObjectSeperation.ToString());
            try {
                minObjectSeperation = int.Parse(stringValue);
            } catch (Exception) { }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Object");
            targetObject = EditorGUILayout.ObjectField(targetObject, typeof(InfiniteObject), false) as InfiniteObject;
            GUILayout.EndHorizontal();

            useEndDistance = GUILayout.Toggle(useEndDistance, "Use End Distance");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Start Distance", GUILayout.Width(120));
            stringValue = GUILayout.TextField(startDistance.ToString());
            try {
                startDistance = int.Parse(stringValue);
            } catch (Exception) { }
            GUILayout.EndHorizontal();

            if (avoidObjectRule && useEndDistance) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("End Distance", GUILayout.Width(120));
                stringValue = GUILayout.TextField(endDistance.ToString());
                try {
                    endDistance = int.Parse(stringValue);
                } catch (Exception) { }
                GUILayout.EndHorizontal();
            } else if (!avoidObjectRule) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Start Probability Adj", GUILayout.Width(120));
                stringValue = GUILayout.TextField(startProbability.ToString());
                try {
                    startProbability = int.Parse(stringValue);
                } catch (Exception) { }
                GUILayout.EndHorizontal();

                if (useEndDistance) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("End Distance", GUILayout.Width(120));
                    stringValue = GUILayout.TextField(endDistance.ToString());
                    try {
                        endDistance = int.Parse(stringValue);
                    } catch (Exception) { }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("End Probability Adj", GUILayout.Width(120));
                    stringValue = GUILayout.TextField(endProbability.ToString());
                    try {
                        endProbability = int.Parse(stringValue);
                    } catch (Exception) { }
                    GUILayout.EndHorizontal();
                }
            }

            if (addError.Length > 0) {
                GUI.contentColor = Color.red;
                GUILayout.Label(addError);
                GUI.contentColor = Color.white;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add")) {
                int error;
                if ((error = addRule()) == 0) {
                    addNewRule = false;
                } else {
                    switch (error) {
                        case 1:
                            addError = "Error: start distance must be\ngreater than end distance";
                            break;
                        case 2:
                            addError = "Error: Target Object is not set";
                            break;
                        case 3:
                            addError = "Error: Both Min Distance and\nMin Object Seperation are 0";
                            break;
                        case 4:
                            addError = "Error: the rule distances overlap\na different set of rule distances";
                            break;
                        case 5:
                            addError = "Error: another rule already exists\nwhich does not have the end distance set";
                            break;
                        default:
                            addError = "Unknown Error";
                            break;
                    }
                }
            }

            if (GUILayout.Button("Cancel")) {
                addNewRule = false;
            }
            GUILayout.EndHorizontal();
        }

        if (!addNewRule && GUILayout.Button("New Rule")) {
            addError = "";
            addNewRule = true;
        }
    }

    public void showAppearanceRules(List<ObjectRuleMap> appearanceRules, bool avoidObjectRules)
    {
        bool parentBreak = false;
        for (int i = 0; i < appearanceRules.Count; ++i) {
            if (appearanceRules[i].rules.Count == 0)
                continue;

            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("{0} Rules", (appearanceRules[i].targetObject != null ? appearanceRules[i].targetObject.name : "All")));
            if (GUILayout.Button("Remove")) {
                appearanceRules.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();

            List<ScoreObjectCountRule> scoreObjectCountRules = appearanceRules[i].rules;
            for (int j = 0; j < scoreObjectCountRules.Count; ++j) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("  Rule {0}", (j + 1)));
                if (GUILayout.Button("Remove")) {
                    scoreObjectCountRules.RemoveAt(j);
                    // Remove the parent as well if there are no sub-rules left
                    if (scoreObjectCountRules.Count == 0) {
                        appearanceRules.RemoveAt(i);
                        parentBreak = true;
                    }
                    EditorUtility.SetDirty(target);
                    break;
                }
                GUILayout.EndHorizontal();

                GUILayout.Label(string.Format("    Min Distance {0}", scoreObjectCountRules[j].minDistance));
                if (scoreObjectCountRules[j].minDistanceSameObjectType) {
                    GUILayout.Label("    Uses min distance for same object type");
                } else {
                    GUILayout.Label("    Uses min distance for object specified");
                }
                GUILayout.Label(string.Format("    Min Object Separation {0}", scoreObjectCountRules[j].minObjectSeparation));

                DistanceValue prob = scoreObjectCountRules[j].probability;
                if (avoidObjectRules) {
                    if (prob.useEndDistance) {
                        GUILayout.Label(string.Format("    Affects Distance {0} - {1}", prob.startDistance, prob.endDistance));
                    } else {
                        GUILayout.Label(string.Format("    Affects Distance {0} - End", prob.startDistance));
                    }
                } else { // probability adjustment uses the full set of probability variables
                    if (prob.useEndDistance) {
                        GUILayout.Label(string.Format("    Affects Distance {0} - {1}", prob.startDistance, prob.endDistance));
                        GUILayout.Label(string.Format("    With Probability {0} - {1}", Math.Round(prob.startValue, 2), Math.Round(prob.endValue, 2)));
                    } else {
                        GUILayout.Label(string.Format("    Affects Distance {0} - End", prob.startDistance));
                        GUILayout.Label(string.Format("    With Probability {0}", Math.Round(prob.startValue, 2)));
                    }
                }
            }
            if (parentBreak)
                break;
        }
    }

    private int addRule()
    {
        if (startDistance >= endDistance && useEndDistance) {
            return 1;
        }

        if (((!minDistanceSameObjectType || minObjectSeperation > 0) && targetObject == null))
            return 2;

        // no point in adding the rule if both of these values are zero
        if (minDistance == 0 && minObjectSeperation == 0)
            return 3;

        List<ObjectRuleMap> appearanceRules;
        if (avoidObjectRule) {
            appearanceRules = ((AppearanceRules)target).avoidObjectRuleMaps;
        } else {
            appearanceRules = ((AppearanceRules)target).probabilityAdjustmentMaps;
        }

        int ruleInsertIndex = 0;
        int subRuleInsertIndex = 0;
        bool parentBreak = false;
        bool infiniteObjectFound = false;
        for ( ; ruleInsertIndex < appearanceRules.Count; ++ruleInsertIndex) {
            if (targetObject == appearanceRules[ruleInsertIndex].targetObject) {
                List<ScoreObjectCountRule> scoreObjectCountRules = appearanceRules[ruleInsertIndex].rules;
                for ( ; subRuleInsertIndex < scoreObjectCountRules.Count; ++subRuleInsertIndex) {
                    // error if the current probability is overlapping an existing probability or within it
                    DistanceValue prob = scoreObjectCountRules[subRuleInsertIndex].probability;
                    if ((startDistance < prob.startDistance && endDistance > prob.startDistance) ||
                        (startDistance > prob.startDistance && endDistance < prob.endDistance) ||
                        (startDistance < prob.endDistance && endDistance > prob.endDistance) ||
                        (!prob.useEndDistance && startDistance > prob.startDistance) ||
                        (!useEndDistance && startDistance < prob.endDistance)) {
                            return 4;
                    }

                    // two probabilities can't ignore the end distance
                    if (!useEndDistance && !prob.useEndDistance) {
                        return 5;
                    }

                    // found our place
                    if (endDistance <= prob.startDistance) {
                        parentBreak = true;
                        break;
                    }
                }
                infiniteObjectFound = true;
                break;
            }
            if (parentBreak)
                break;
        }

        DistanceValue distanceProb = new DistanceValue(startDistance, startProbability, endDistance, endProbability, useEndDistance);
        ScoreObjectCountRule scoreObjectCountRule = new ScoreObjectCountRule(minDistance, minDistanceSameObjectType, minObjectSeperation, distanceProb);
        if (infiniteObjectFound) {
            List<ScoreObjectCountRule> scoreObjectCountRules = appearanceRules[ruleInsertIndex].rules;
            scoreObjectCountRules.Insert(subRuleInsertIndex, scoreObjectCountRule);
        } else {
            ObjectRuleMap objectRuleMap = new ObjectRuleMap(targetObject, scoreObjectCountRule);
            appearanceRules.Insert(ruleInsertIndex, objectRuleMap);
        }

        EditorUtility.SetDirty(target);
        return 0;
    }
}
