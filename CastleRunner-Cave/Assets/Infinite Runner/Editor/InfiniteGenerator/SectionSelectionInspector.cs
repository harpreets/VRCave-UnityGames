using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SectionSelection))]
public class SectionSelectionInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SectionSelection sectionSelection = (SectionSelection)target;

        if (sectionSelection.sectionSelectionType == SectionSelectionType.None) {
            return;
        }
        
        bool probabilityType = false;
        if (sectionSelection.sectionSelectionType == SectionSelectionType.ProbabilityRandom ||
            sectionSelection.sectionSelectionType == SectionSelectionType.ProbabilityLoop) {

            GUILayout.Label("Active Sections", "BoldLabel");
            int start = EditorGUILayout.IntField("Start Section", sectionSelection.startSection);
            if (start != sectionSelection.startSection) {
                sectionSelection.startSection = start;
            }
            int end = EditorGUILayout.IntField("End Section", sectionSelection.endSection);
            if (end != sectionSelection.endSection) {
                sectionSelection.endSection = end;
            }
            probabilityType = true;
        }

        GUILayout.Label(string.Format("Section Change {0}", probabilityType ? "Probability" : ""), "BoldLabel");
        DistanceValueList sectionList = sectionSelection.sectionList;
        if (DistanceValueListInspector.showLoopToggle(ref sectionList, probabilityType ? DistanceValueType.Probability : DistanceValueType.Section)) {
            sectionSelection.sectionList = sectionList;
            EditorUtility.SetDirty(target);
        }
        DistanceValueListInspector.showDistanceValues(ref sectionList, probabilityType ? DistanceValueType.Probability : DistanceValueType.Section);

        if (DistanceValueListInspector.showAddNewValue(ref sectionList, probabilityType ? DistanceValueType.Probability : DistanceValueType.Section)) {
            sectionSelection.sectionList = sectionList;
            EditorUtility.SetDirty(target);
        }
    }

    // For static references to this inspector:
    private static bool addSection = false;
    private static bool addTransition = false;
    private static int sectionID;
    private static int transitionFromSection;
    private static int transitionToSection;

    public static bool showSections(ref List<int> sections, bool fromInfiniteObjectGenerator)
    {
        if (sections.Count == 0) {
            if (fromInfiniteObjectGenerator) {
                GUILayout.Label("No sections created");
            } else {
                GUILayout.Label("Object appears in all sections");
            }
        } else {
            for (int i = 0; i < sections.Count; ++i) {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                GUILayout.Label(sections[i].ToString());
                if (GUILayout.Button("X", GUILayout.Width(30))) {
                    sections.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        if (addSection) {
            if (showAddSectionOptions(ref sections))
                return true;
        } else if (GUILayout.Button("Add Section")) {
            sectionID = 0;
            addSection = true;
        }
        return false;
    }

    private static bool showAddSectionOptions(ref List<int> sections)
    {
        sectionID = EditorGUILayout.IntField("Section ID", sectionID);
        if (GUILayout.Button("Add")) {
            sections.Add(sectionID);
            sections.Sort();
            addSection = false;
            return true;
        }
        return false;
    }

    public static bool showSectionTransitions(ref List<int> fromSection, ref List<int> toSection)
    {
        if (fromSection.Count == 0) {
            GUILayout.Label("Transitions from/to any section");
        } else {
            for (int i = 0; i < fromSection.Count; ++i) {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                GUILayout.Label(string.Format("From {0} to {1}", fromSection[i], toSection[i]));
                if (GUILayout.Button("X", GUILayout.Width(30))) {
                    fromSection.RemoveAt(i);
                    toSection.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        if (addTransition) {
            if (showSectionTransitionOptions(ref fromSection, ref toSection))
                return true;
        } else if (GUILayout.Button("Add Section Transition")) {
            transitionFromSection = transitionToSection = 0;
            addTransition = true;
        }
        return false;
    }

    private static bool showSectionTransitionOptions(ref List<int> fromSection, ref List<int> toSection)
    {
        transitionFromSection = EditorGUILayout.IntField("From section", transitionFromSection);
        transitionToSection = EditorGUILayout.IntField("To section", transitionToSection);
        if (GUILayout.Button("Add")) {
            fromSection.Add(transitionFromSection);
            toSection.Add(transitionToSection);
            addTransition = false;
            return true;
        }
        return false;
    }
}
