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
    public string guid;
    [HideInInspector] public Vector2 graphPos;
    
    public List<CustomPort> inputPorts = new();
    public List<CustomPort> outputPorts = new();
    private Dictionary<string, FieldInfo> nodeFields;
    private int iterationId, activationId;
    private Action transferDelegate;
    private List<Node> next;
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
            var s = f.GetCustomAttribute<PortDecoratorAttribute>();
            var l = o.Output ? outputPorts : inputPorts;
            l.Add(AddPort(o.multi, f.Name, f, !o.Output, s?.Shape ?? CustomPort.Style.round));
        }
        ClearState();
    }

    private CustomPort AddPort(bool multi, string n, FieldInfo f, bool input, CustomPort.Style s)
    {
        return new CustomPort
        {
            Edges = new List<CustomEdge>(), multi = multi, fieldName = f.Name,
            FieldType = f.FieldType, displayName = n, input = input, shape = s,
            readOnly = f.GetCustomAttribute<PortAttribute>().readOnly
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
    
    private Action CreateTransferDelegate()
    {
        next = new();
        var expressions = new List<Expression>();
        
        MethodInfo tryTypeConvertMethod = typeof(Node).GetMethod(nameof(TryTypeConvert), BindingFlags.Static | BindingFlags.Public);
        
        foreach (var p in inputPorts)
        {
            if (p.Edges.Any())
            {
                var get = p.Edges[0];
                if (!get.OutputPort.readOnly && !next.Contains(get.outputNode)) next.Add(get.outputNode);
                var sourceParameter = Expression.Constant(get.outputNode, get.outputNode.GetType());
                var targetParameter = Expression.Constant(get.inputNode, get.inputNode.GetType());
                
                var sourceField = Expression.Field(sourceParameter, get.OutputPort.AttachedField);
                var targetField = Expression.Field(targetParameter, get.InputPort.AttachedField);
                
                var tryTypeConvertCall = Expression.Call(
                    tryTypeConvertMethod,
                    Expression.Convert(sourceField, typeof(object)),
                    Expression.Constant(targetField.Type)
                );

                // Convert the result back to the target field type
                var convertedValue = Expression.Convert(tryTypeConvertCall, targetField.Type);

                var assignExpression = Expression.Assign(targetField, convertedValue);
                
                expressions.Add(assignExpression);
            }
        }

        var body = Expression.Block(expressions);
        var lambda = Expression.Lambda<Action>(body);
        return lambda.Compile();
    }
    
    public static object TryTypeConvert(object input, Type targetType)
    {
        if (input == null) return null;
        var t1 = input.GetType();
        Debug.Log($"{t1} {targetType}");
        if (t1 == targetType) return input;
        
        switch (t1)
        {
            case not null when t1 == typeof(float) && targetType == typeof(Vector2):
                return new Vector2((float)input, (float)input);
            
            case not null when t1 == typeof(float) && targetType == typeof(Vector3):
                return new Vector3((float)input, (float)input, (float)input);
            
            case not null when t1 == typeof(Vector2) && targetType == typeof(float):
                return ((Vector2)input).x;
            
            case not null when t1 == typeof(Vector2) && targetType == typeof(Vector3):
                return (Vector3)(Vector2)input;

            case not null when t1 == typeof(Vector3) && targetType == typeof(float):
                return ((Vector3)input).x;
            
            case not null when t1 == typeof(Vector3) && targetType == typeof(Vector2):
                return (Vector2)(Vector3)input;
        }
        return input;
    }

    public void CalculateTransfer()
    {
        transferDelegate = CreateTransferDelegate();
    }
    
    protected TreeStateData GetAllInputs(TreeStateData state)
    {
        var outState = state.Clone();
        try
        {
            foreach (var node in next)
            {
                Debug.Log(node);
                
                if (node)
                {
                    outState = outState.Merge(node.Eval(state));
            
                    if (outState.state.Error) Debug.Log($"oops Upstream: {node}");
                }
            }

            transferDelegate.Invoke();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            outState = outState.Error();
            return outState;
        }
        
        
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

        return outState;
    }

    public void DisconnectAll()
    {
        inputPorts.ForEach(p => p.DisconnectAll());
        outputPorts.ForEach(p => p.DisconnectAll());
    }
    
    public CustomPort FindInputPort(string field)
    {
        var p = inputPorts.Find(p => p.fieldName == field);
        if (p.AttachedField == null) p.Reset(nodeFields[field]);
        return p;
    }
    
    public CustomPort FindOutputPort(string field)
    {
        var p = outputPorts.Find(p => p.fieldName == field);
        if (p.AttachedField == null) p.Reset(nodeFields[field]);
        return p;
    }

    public void OnBeforeSerialize()
    {
        
    }
    
    public void UpdateDefault(FieldInfo field, object value)
    {
        field.SetValue(value, this);
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

