
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class NodeEditor: Editor
{
    public bool notAttached;
    public override void OnInspectorGUI()
    {
        if (!serializedObject.targetObject) return;
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
                _ = RunNode(n, r);

            }
            if (GUILayout.Button("Reset"))
            {
                Node n = serializedObject.targetObject as Node;
                n.Reset();
            }
            if (GUILayout.Button("ClearState"))
            {
                Node n = serializedObject.targetObject as Node;
                n.ClearState();
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

    private async Awaitable RunNode(Node n, GameObject g)
    {
        Node.TreeStateData state = new() { originator = n, attached = g, activationId = Random.Range(0, 57890190), test = true , state = new Node.TreeState()};
        state = state.Repeat();
        var i = 0;
        while (state.state.Repeat && i<300)
        {
            state = n.Eval(state.Reset());
            state.iterationId++;
            i++;
            await Task.Delay(1000/60);
        }
        Debug.Log($"finished testing {n}");
    }

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
