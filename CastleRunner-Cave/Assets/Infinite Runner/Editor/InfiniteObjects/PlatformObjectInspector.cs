using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/*
 * Adds a custom inspector to the section transitions
 */
[CustomEditor(typeof(PlatformObject))]
public class PlatformObjectInspector : InfiniteObjectInspector
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlatformObject platformObject = (PlatformObject)target;

        bool sectionTransition = EditorGUILayout.Toggle("Is Section Transition", platformObject.sectionTransition);
        if (sectionTransition != platformObject.sectionTransition) {
            platformObject.sectionTransition = sectionTransition;
            EditorUtility.SetDirty(target);
        }

        if (sectionTransition) {
            List<int> fromSection = platformObject.fromSection;
            List<int> toSection = platformObject.toSection;
            if (SectionSelectionInspector.showSectionTransitions(ref fromSection, ref toSection)) {
                platformObject.fromSection = fromSection;
                platformObject.toSection = toSection;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
