using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * This editor script will allow you to add sections to different infinite objects
 */
[CustomEditor(typeof(InfiniteObject))]
public class InfiniteObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Sections:");
        InfiniteObject infiniteObject = (InfiniteObject)target;
        List<int> sections = infiniteObject.sections;
        if (SectionSelectionInspector.showSections(ref sections, false)) {
            infiniteObject.sections = sections;
            EditorUtility.SetDirty(target);
        }
    }
}
