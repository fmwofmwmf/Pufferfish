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
    public bool multi;

    public PortAttribute(bool output)
    {
        this.output = output;
        readOnly = false;
        multi = true;
    }
    public PortAttribute(bool output, bool read)
    {
        this.output = output;
        readOnly = read;
        multi = true;
    }
    public PortAttribute(bool output, bool read, bool multi)
    {
        this.output = output;
        readOnly = read;
        this.multi = multi;
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