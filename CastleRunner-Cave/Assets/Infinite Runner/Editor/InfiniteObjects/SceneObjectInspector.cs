using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/*
 * Custom editor insepectors don't support inheritance.. get around that by subclassing
 */
[CustomEditor(typeof(SceneObject))]
public class SceneObjectInspector : InfiniteObjectInspector
{
    // Intentionally left blank, use the parent class
}
