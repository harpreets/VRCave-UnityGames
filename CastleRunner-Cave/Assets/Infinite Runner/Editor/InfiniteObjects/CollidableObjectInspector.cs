using UnityEngine;
using UnityEditor;

/*
 * Custom editor insepectors don't support inheritance.. get around that by subclassing
 */
[CustomEditor(typeof(CollidableObject))]
public class CollidableObjectInspector : InfiniteObjectInspector
{
    // Intentionally left blank, use the parent class
}
