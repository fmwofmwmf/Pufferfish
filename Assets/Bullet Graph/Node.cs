using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public abstract class Node : ScriptableObject, ISerializationCallbackReceiver
{
    public abstract string Name { get; }
    [Editable] public string guid;
    [HideInInspector] public Vector2 graphPos;
    
    [Editable] public List<CustomPort> inputPorts = new();
    [Editable] public List<CustomPort> outputPorts = new();
    private Dictionary<string, FieldInfo> nodeFields;
    private int iterationId, activationId;
    
    public virtual void ClearState()
    {
         
    }

    public void Reset()
    {
        DisconnectAll();
        inputPorts.Clear();
        outputPorts.Clear();
        foreach (var f in nodeFields.Values)
        {
            var o = f.GetCustomAttribute<PortAttribute>();
            var l = o.Output ? outputPorts : inputPorts;
            l.Add(AddPort(o.multi, f.Name, f, !o.Output));
        }
        ClearState();
    }

    private CustomPort AddPort(bool multi, string n, FieldInfo f, bool input)
    {
        return new CustomPort
        {
            Edges = new List<CustomEdge>(), multi = multi, defaultValue = new DefaultValueHolder(), fieldName = f.Name,
            FieldType = f.FieldType, displayName = n, input = input
        };
    }

    protected Node()
    {
        nodeFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<PortAttribute>() != null).ToDictionary( k=> k.Name, k => k);
    }

    public TreeStateData Eval(TreeStateData state)
    {
        if (state.activationId != activationId || state.iterationId != iterationId)
        {
            activationId = state.activationId;
            iterationId = state.iterationId;
            return Evaluate(state);
        }

        return state;
    }
    protected abstract TreeStateData Evaluate(TreeStateData state);
    
    public Action<Node> CreateTransferDelegate()
    {
        var sourceParameter = Expression.Parameter(typeof(Node), "source");
        var targetParameter = Expression.Parameter(typeof(Node), "target");
        var expressions = new List<Expression>();

        foreach (var p in inputPorts)
        {
            if (p.Edges.Any())
            {
                var get = p.Edges[0];
                
                var sourceField = Expression.Field(sourceParameter, get.InputPort.AttachedField);
                var targetField = Expression.Field(targetParameter, get.OutputPort.AttachedField);
                var assignExpression = Expression.Assign(targetField, sourceField);
                expressions.Add(assignExpression);
            }
        }

        var body = Expression.Block(expressions);
        var lambda = Expression.Lambda<Action<Node>>(body, sourceParameter, targetParameter);
        return lambda.Compile();
    }
    
    protected TreeStateData GetAllInputs(TreeStateData state)
    {
        // var s = new HashSet<ConnectedPort>();
        // inFields.ForEach(f=>
        // {
        //     s.Add(GetConnection(f.Name));
        // });
        // var outState = state.Clone();
        // foreach (var port in s)
        // {
        //     try
        //     {
        //         if (port.other != null && !port.read)
        //         {
        //             var a = port.other.Eval(state);
        //             outState = outState.Merge(a);
        //
        //             if (a.state.Error) Debug.Log($"oops Upstream: {port}");
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log(e);
        //         outState = outState.Error();
        //         return outState;
        //     }
        // }
        //
        // foreach (var f in inFields)
        // {
        //     if (f != null) GetInput(f.Name);
        // }

        return new();
    }

    public void DisconnectAll()
    {
        inputPorts.ForEach(p => p.DisconnectAll());
        outputPorts.ForEach(p => p.DisconnectAll());
    }
    
    public CustomPort FindInputPort(string field)
    {
        return inputPorts.Find(p => p.fieldName == field);
    }
    
    public CustomPort FindOutputPort(string field)
    {
        return outputPorts.Find(p => p.fieldName == field);
    }
    
    protected void GetInput(string field)
    {
        // var a = fieldNames.FindIndex(s => s == field);
        //
        // if (fieldConnections[a].field != "" && fieldConnections[a].other)
        // {
        //     var other = fieldConnections[a].other;
        //     var f = GetAllFields().Find(s => s.Name == field);
        //     var otherField = other.GetAllFields().Find(s => s.Name == fieldConnections[a].field);
        //     f.SetValue(this, otherField.GetValue(other));
        // }
        // else
        // {
        //     var f = GetAllFields().Find(s => s.Name == field);
        //     var v = fieldConnections[a].defaultValue.ExtractValue(f.FieldType);
        //     f.SetValue(this, v);
        // }
    }

    public void OnBeforeSerialize()
    {
        
    }
    
    public void UpdateDefault(FieldInfo field, object value)
    {
        var a = inputPorts.Find(p => p.fieldName == field.Name);
        a.defaultValue.SetValue(field, value);
    }
    
    
    public void OnAfterDeserialize()
    {
        nodeFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<PortAttribute>() != null).ToDictionary( k=> k.Name, k => k);
        
        bool ok = true;
        inputPorts.ForEach(p =>
        { 
            if (nodeFields.TryGetValue(p.fieldName, out FieldInfo f)) p.Reset(f);
            else
            {
                ok = false; 
            }
        });
        outputPorts.ForEach(p => 
        {
            if (nodeFields.TryGetValue(p.fieldName, out FieldInfo f)) p.Reset(f);
            else {ok = false;}
        });
        Debug.Log(ok);
        if (ok || inputPorts.Count + outputPorts.Count != nodeFields.Count) Reset();
    }
    
    

    public struct TreeStateData
    {
        public Node originator;
        public GameObject attached;
        public int activationId;
        public int iterationId;
        public bool test;
        public TreeState state;
        public TreeStateData Set(TreeState state)
        {
            this.state = state;
            return this;
        }
        public TreeStateData Virt()
        {
            var c = Clone();
            c.state.Virtual = true;
            return c;
        }
        public TreeStateData Repeat()
        {
            var c = Clone();
            c.state.Repeat = true;
            return c;
        }
        public TreeStateData Error()
        {
            var c = Clone();
            c.state.Error = true;
            return c;
        }

        public TreeStateData Clone()
        {
            var s = new TreeStateData {
                activationId = activationId, state = new TreeState(), attached = attached, originator = originator,
                iterationId = iterationId, test = test
            };
            s.Merge(this);
            return s;
        }

        public TreeStateData Merge(TreeStateData other)
        {
            state.Virtual = other.state.Virtual;
            state.Repeat = other.state.Repeat;
            state.Error = other.state.Error;
            return this;
        }
        
        public TreeStateData Reset()
        {
            state.Clear();
            return this;
        }
    }

    public struct TreeState
    {
        public bool Repeat
        {
            get => r;
            set => r = r || value;
        }
        public bool Virtual
        {
            get => f;
            set => f = f || value;
        }
        public bool Error
        {
            get => e;
            set => e = e || value;
        }
        private bool r, f, e;

        public void Clear()
        {
            r = false;
            f = false;
            e = false;
        }
    }

    
}

[Serializable]
public struct DefaultValueHolder
{
    // Fields for all Unity serializable types
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public Color colorValue;
    public GameObject gameObjectValue;
    public Transform transformValue;

    // Add other types as needed...

    // The type field to keep track of the stored type
    public string type;

    // Method to set value based on FieldInfo and object
    public DefaultValueHolder SetValue(FieldInfo field, object value)
    {
        Type fieldType = field.FieldType;

        if (fieldType == typeof(int))
        {
            intValue = (int)value;
            type = "int";
        }
        else if (fieldType == typeof(float))
        {
            floatValue = (float)value;
            type = "float";
        }
        else if (fieldType == typeof(string))
        {
            stringValue = (string)value;
            type = "string";
        }
        else if (fieldType == typeof(bool))
        {
            boolValue = (bool)value;
            type = "bool";
        }
        else if (fieldType == typeof(Vector2))
        {
            vector2Value = (Vector2)value;
            type = "Vector2";
        }
        else if (fieldType == typeof(Vector3))
        {
            vector3Value = (Vector3)value;
            type = "Vector3";
        }
        else if (fieldType == typeof(Color))
        {
            colorValue = (Color)value;
            type = "Color";
        }
        else if (fieldType == typeof(GameObject))
        {
            gameObjectValue = (GameObject)value;
            type = "GameObject";
        }
        else if (fieldType == typeof(Transform))
        {
            transformValue = (Transform)value;
            type = "Transform";
        }
        else
        {
            throw new ArgumentException($"Unsupported type: {fieldType}");
        }

        return this;
    }
    
    public object ExtractValue(Type fieldType)
    {
        if (fieldType == typeof(int))
        {
            return intValue;
        }
        else if (fieldType == typeof(float))
        {
            return floatValue;
        }
        else if (fieldType == typeof(string))
        {
            return stringValue;
        }
        else if (fieldType == typeof(bool))
        {
            return boolValue;
        }
        else if (fieldType == typeof(Vector2))
        {
            return vector2Value;
        }
        else if (fieldType == typeof(Vector3))
        {
            return vector3Value;
        }
        else if (fieldType == typeof(Color))
        {
            return colorValue;
        }
        else if (fieldType == typeof(GameObject))
        {
            return gameObjectValue;
        }
        else if (fieldType == typeof(Transform))
        {
            return transformValue;
        }
        else
        {
            throw new ArgumentException($"Unsupported type: {fieldType}");
        }
    }
}
