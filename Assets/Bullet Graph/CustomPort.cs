using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
[Serializable]
public class CustomPort
{
    [NonSerialized] public BulletGraph Graph;
    public Node owner;
    [NonSerialized] public List<CustomEdge> Edges;
    public Type FieldType;
    public DefaultValueHolder defaultValue;
    private FieldInfo attachedField;
    public string fieldName;
    public string displayName;
    public bool input;
    public bool multi;

    public CustomPort(Node owner, bool input, bool multi, FieldInfo info)
    {
        this.owner = owner;
        attachedField = info;
        fieldName = info.Name;
        this.input = input;
        this.multi = multi;
        
        Edges = new List<CustomEdge>();
    }

    public bool Connect(CustomEdge edge)
    {
        if (!multi) DisconnectAll();
        Edges.Add(edge);
        return true;
    }
    
    public CustomEdge FindConnectedEdge(CustomPort other)
    {
        return Edges.Find(e => input ? e.outputPort == other : e.inputPort == other);
    }

    public void DisconnectAll()
    {
        Edges.ForEach(e => e.DeleteEdge());
    }
    
    public void RemoveEdge(CustomEdge edge)
    {
        Edges.Remove(edge);
    }
}
