using UnityEngine;
using UnityEditor;

/*
 * Custom editor insepectors don't support inheritance.. get around that by subclassing
 */
[CustomEditor(typeof(PlatformAppearanceRules))]
public class PlatformAppearanceRulesInspector : AppearanceRulesInspector
{
    // Intentionally left blank, use the parent class
}
