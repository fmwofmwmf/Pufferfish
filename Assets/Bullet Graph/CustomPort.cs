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
    [NonSerialized] public List<CustomEdge> Edges;
    public Type FieldType;
    public FieldInfo AttachedField;
    public string fieldName;
    public string displayName;
    public bool input;
    public bool multi;
    public bool readOnly;
    public Style shape;

    public void Reset(FieldInfo info)
    {
        fieldName = info.Name;
        displayName = info.Name;
        AttachedField = info;
        FieldType = info.FieldType;
        readOnly = info.GetCustomAttribute<PortAttribute>().readOnly;
        Debug.Log(readOnly);
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
        for (int i = Edges.Count - 1; i >= 0; i--)
        {
            Edges[i].DeleteEdge();
        }
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
