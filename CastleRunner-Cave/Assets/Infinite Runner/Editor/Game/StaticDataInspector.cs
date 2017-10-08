using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * An inspector class which allows you to easily set the static data
 */
[CustomEditor(typeof(StaticData))]
public class StaticDataInspector : Editor
{
    private int powerUpLevels;

    public void OnEnable()
    {
        StaticData staticData = (StaticData)target;
        if (staticData.totalPowerUpLevels == 0) {
            staticData.totalPowerUpLevels = 1;
        }

        if (staticData.powerUpCost == null) {
            staticData.powerUpLength = new float[(int)PowerUpTypes.None * (staticData.totalPowerUpLevels + 1)];
            staticData.powerUpCost = new int[(int)PowerUpTypes.None * staticData.totalPowerUpLevels];
            staticData.powerUpColor = new Color[(int)PowerUpTypes.None * staticData.totalPowerUpLevels];
        }

        if (staticData.characterCost == null) {
            staticData.characterCost = new int[(int)Character.None];
            staticData.characterPrefab = new GameObject[(int)Character.None];
        }

        if (staticData.missionDescription == null) {
            staticData.missionDescription = new string[(int)MissionType.None];
            staticData.missionGoal = new int[(int)MissionType.None];
        }

        powerUpLevels = staticData.totalPowerUpLevels;

        if (staticData.missionDescription.Length != (int)MissionType.None) {
            staticData.missionDescription = new string[(int)MissionType.None];
            staticData.missionGoal = new int[(int)MissionType.None];
        }
    }

    public override void OnInspectorGUI()
    {
        // Power Ups:
        GUILayout.Label("Power Ups", "BoldLabel");

        StaticData staticData = (StaticData)target;
        for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
            GUILayout.Label(Enum.GetName(typeof(PowerUpTypes), i), "BoldLabel");
            staticData.powerUpColor[i] = EditorGUILayout.ColorField("Particle Color", staticData.powerUpColor[i]);
            for (int j = 0; j < powerUpLevels; ++j) {
                staticData.powerUpCost[(i * powerUpLevels) + j] = EditorGUILayout.IntField(string.Format("Level {0} Cost:", j), staticData.powerUpCost[(i * powerUpLevels) + j]);

                staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1] = EditorGUILayout.FloatField(string.Format("Level {0} Length:", j), staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1]);
            }
        }
        if (GUILayout.Button("Add Level")) {
            addPowerUpLevel();
        }
        if (powerUpLevels > 1 && GUILayout.Button("Remove Level")) {
            removePowerUpLevel();
        }

        GUILayout.Space(20);

        // Missions:
        GUILayout.Label("Missions", "BoldLabel");

        if (staticData.missionDescription.Length == 0) {
            staticData.missionDescription = new string[(int)MissionType.None];
            staticData.missionGoal = new int[(int)MissionType.None];
        }
        for (int i = 0; i < (int)MissionType.None; ++i) {
            GUILayout.Label(Enum.GetName(typeof(MissionType), i), "BoldLabel");
            staticData.missionDescription[i] = EditorGUILayout.TextField("Description", staticData.missionDescription[i]);
            staticData.missionGoal[i] = EditorGUILayout.IntField("Goal", staticData.missionGoal[i]);
        }

        GUILayout.Space(20);

        // Characters:
        GUILayout.Label("Characters", "BoldLabel");

        if (staticData.characterCost.Length == 0) {
            staticData.characterCost = new int[(int)Character.None];
            staticData.characterPrefab = new GameObject[(int)Character.None];
        }
        for (int i = 0; i < (int)Character.None; ++i) {
            GUILayout.Label(Enum.GetName(typeof(Character), i), "BoldLabel");
            staticData.characterCost[i] = EditorGUILayout.IntField("Cost", staticData.characterCost[i]);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefab");
            staticData.characterPrefab[i] = EditorGUILayout.ObjectField(staticData.characterPrefab[i], typeof(GameObject), false) as GameObject;
            GUILayout.EndHorizontal();
        }
    }

    // add a new level to the end of each power up type
    private void addPowerUpLevel()
    {
        StaticData staticData = (StaticData)target;
        
        int[] cost = new int[staticData.powerUpCost.Length + (int)PowerUpTypes.None];
        float[] length = new float[staticData.powerUpLength.Length + (int)PowerUpTypes.None];
        for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
            for (int j = 0; j < powerUpLevels + 1; ++j) {
                if (j == powerUpLevels) {
                    cost[i * (powerUpLevels + 1) + j] = 0;
                    length[(i * (powerUpLevels + 2)) + j + 1] = 0;
                } else {
                    cost[i * (powerUpLevels + 1) + j] = staticData.powerUpCost[i * powerUpLevels + j];
                    length[(i * (powerUpLevels + 2)) + j + 1] = staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1];
                }
            }
        }

        powerUpLevels++;

        staticData.powerUpCost = cost;
        staticData.powerUpLength = length;
        staticData.totalPowerUpLevels = powerUpLevels;

        EditorUtility.SetDirty(staticData);
    }

    // remove the last level from each power up type
    private void removePowerUpLevel()
    {
        StaticData staticData = (StaticData)target;

        int[] cost = new int[staticData.powerUpCost.Length - (int)PowerUpTypes.None];
        float[] length = new float[staticData.powerUpLength.Length - (int)PowerUpTypes.None];
        for (int i = 0; i < (int)PowerUpTypes.None; ++i) {
            for (int j = 0; j < powerUpLevels - 1; ++j) {
                cost[i * (powerUpLevels - 1) + j] = staticData.powerUpCost[i * powerUpLevels + j];
                length[(i * powerUpLevels) + j + 1] = staticData.powerUpLength[(i * (powerUpLevels + 1)) + j + 1];
            }
        }

        powerUpLevels--;

        staticData.powerUpCost = cost;
        staticData.powerUpLength = length;
        staticData.totalPowerUpLevels = powerUpLevels;

        EditorUtility.SetDirty(staticData);
    }
}
