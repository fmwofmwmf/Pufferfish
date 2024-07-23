﻿
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class NodeEditor: Editor
{
    public bool notAttached;
    public override void OnInspectorGUI()
    {
        // Get the target object type
        var type = target.GetType();

        // Get all fields from the target object
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (notAttached)
        { 
            EditorGUILayout.LabelField($"Node {serializedObject.targetObject.name}");
            if (GUILayout.Button("Evaluate"))
            {
                Node n = serializedObject.targetObject as Node;
                
                GameObject g = GameObject.Find("BulletTest");

                if (g != null) DestroyImmediate(g);
                var r = GameObject.CreatePrimitive(PrimitiveType.Cube);
                r.name = "BulletTest";
                n.Eval(new(){originator = n, attached = r, activationId = Random.Range(0, 57890190), test = true});
            }
            if (GUILayout.Button("Reset"))
            {
                Node n = serializedObject.targetObject as Node;
                n.Reset();
            }
        }

        foreach (var field in fields)
        {
            // Check if the field has the ShowInCustomEditor attribute
            var attributes = field.GetCustomAttributes(typeof(EditableAttribute), false);
            if (attributes.Length > 0)
            {
                // Draw the field if it has the attribute
                SerializedProperty property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
        }

        // Apply any property modifications
        serializedObject.ApplyModifiedProperties();
    }

    // private Awaitable RunNode()
    // {
    //     
    //     return null;
    // }

    private object DrawField(FieldInfo field, object value)
    {
        if (value is int)
        {
            return EditorGUILayout.IntField(field.Name, (int)value);
        }
        else if (value is float)
        {
            return EditorGUILayout.FloatField(field.Name, (float)value);
        }
        else if (value is string)
        {
            return EditorGUILayout.TextField(field.Name, (string)value);
        }
        else if (value is bool)
        {
            return EditorGUILayout.Toggle(field.Name, (bool)value);
        }
        else if (value is Vector3)
        {
            return EditorGUILayout.Vector3Field(field.Name, (Vector3)value);
        }
        else if (value is Enum)
        {
            return EditorGUILayout.EnumPopup(field.Name, (Enum)value);
        }
        // Add support for other types as needed
        else
        {
            EditorGUILayout.LabelField(field.Name, "Unsupported type: " + field.FieldType);
            return value;
        }
    }
}