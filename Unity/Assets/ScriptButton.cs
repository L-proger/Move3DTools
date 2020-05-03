using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[Serializable]
public struct InspectorButton {

}

[CustomPropertyDrawer(typeof(InspectorButton))]
public class InspectorButtonDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (GUI.Button(position, property.name)) {
            var target = property.serializedObject.targetObject;
            var searchName = "On" + property.name;
            var method = target.GetType().GetMethod("on" + property.name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            
            if(method != null) {
                method.Invoke(target, null);
            } else {
                Debug.Log("Method not found: " + searchName);
            }
        }
    }
}


