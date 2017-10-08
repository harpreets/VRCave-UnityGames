using UnityEngine;
using UnityEditor;

/*
 * Custom editor insepectors don't support inheritance.. get around that by subclassing
 */
[CustomEditor(typeof(PowerUpAppearanceRules))]
public class PowerUpAppearanceRulesInspector : AppearanceRulesInspector
{
    // Intentionally left blank, use the parent class
}
