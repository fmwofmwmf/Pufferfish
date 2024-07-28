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
public struct CustomPort
{
    [NonSerialized] public List<CustomEdge> Edges;
    public Type FieldType;
    public DefaultValueHolder defaultValue;
    public FieldInfo AttachedField;
    public string fieldName;
    public string displayName;
    public bool input;
    public bool multi;
    public Style shape;

    public void Reset(FieldInfo info)
    {
        fieldName = info.Name;
        displayName = info.Name;
        AttachedField = info;
        FieldType = info.FieldType;
        Edges = new List<CustomEdge>();
    }

    public bool Connect(CustomEdge edge)
    {
        if (!multi) DisconnectAll();
        EdgeCheck();
        Edges.Add(edge);
        return true;
    }
    
    public CustomEdge FindConnectedEdge(CustomPort other)
    {
        bool i = input;
        EdgeCheck();
        return Edges.Find(e => (i ? e.OutputPort : e.InputPort).AttachedField == other.AttachedField);
    }

    public void DisconnectAll()
    {
        EdgeCheck();
        Edges.ForEach(e => e.DeleteEdge());
    }
    
    public void RemoveEdge(CustomEdge edge)
    {
        EdgeCheck();
        Edges.Remove(edge);
    }

    private void EdgeCheck()
    {
        if (Edges == null) Edges = new();
    }
    
    public enum Style
    {
        round,
        square,
        cap
    }
}
