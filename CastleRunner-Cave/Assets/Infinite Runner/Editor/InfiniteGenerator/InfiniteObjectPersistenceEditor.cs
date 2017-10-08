using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/*
 * Quickly saves the infinite objects on the screen and the important infinite object generator/history variables so you can restore the state.
 * This is useful for tutorials or objects that you always want to show at the start of the game
 */
public class InfiniteObjectPersistenceEditor : EditorWindow {
    [MenuItem("Window/Infinite Object Persistence")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<InfiniteObjectPersistenceEditor>();
    }

    private string saveLocation = "";

    public void OnGUI()
    {
        GUILayout.Label("Infinite Object Persistence", "BoldLabel");
        GUILayout.Label("1. Generate desired tracks using rules and probabilities");
        GUILayout.Label("2. Hit play in Unity");
        GUILayout.Label("3. Add extra objects (such as tutorial triggers)");
        GUILayout.Label("4. Click \"Persist\"");
        GUILayout.Space(10);
        if (GUILayout.Button("Persist")) {
            saveLocation = EditorUtility.SaveFilePanelInProject("Save Location", "InfiniteObjectPersistence", "prefab", "");
            if (saveLocation.Length == 0)
                return;

            GameObject infiniteObjectsGroup = GameObject.Find("Infinite Objects");
            if (infiniteObjectsGroup != null) {
                GameObject persistGameObject = new GameObject();

                InfiniteObjectPersistence persistence = persistGameObject.AddComponent<InfiniteObjectPersistence>() as InfiniteObjectPersistence;
                // Persist the Infinite Object Manager Data
                InfiniteObjectGenerator infiniteObjectGenerator = infiniteObjectsGroup.GetComponent<InfiniteObjectGenerator>();
                infiniteObjectGenerator.saveInfiniteObjectPersistence(ref persistence);

                // Persist the Infinite Object History Data
                InfiniteObjectHistory infiniteObjectHistory = infiniteObjectsGroup.GetComponent<InfiniteObjectHistory>();
                infiniteObjectHistory.saveInfiniteObjectPersistence(ref persistence);

                for (int i = infiniteObjectsGroup.transform.childCount - 1; i >= 0; --i) {
                    infiniteObjectsGroup.transform.GetChild(i).parent = persistGameObject.transform;
                }

                EditorUtility.SetDirty(persistGameObject);
                PrefabUtility.CreatePrefab(saveLocation, persistGameObject);

                for (int i = persistGameObject.transform.childCount - 1; i >= 0; --i) {
                    persistGameObject.transform.GetChild(i).parent = infiniteObjectsGroup.transform;
                }

                DestroyImmediate(persistGameObject);

                Debug.Log("Infinite Object Data Persisted!");
            } else {
                Debug.Log("Error: Unable to find the Infinite Objects Game Object");
            }
        }
    }
}
