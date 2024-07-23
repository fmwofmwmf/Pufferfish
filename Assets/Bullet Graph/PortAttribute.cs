using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Field)]
public class PortAttribute : Attribute
{
    public bool output;
    public bool readOnly;
    public bool right;

    public PortAttribute(bool output)
    {
        this.output = output;
        readOnly = false;
        right = output;
    }
    public PortAttribute(bool output, bool read)
    {
        this.output = output;
        readOnly = read;
        right = output;
    }
    public PortAttribute(bool output, bool read, bool left)
    {
        this.output = output;
        readOnly = read;
        right = left;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class EditableAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string path;
    public NodeAttribute(string path)
    {
        this.path = path;
    }
}