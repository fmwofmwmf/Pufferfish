using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Field)]
public class PortAttribute : Attribute
{
    public bool Output;
    public bool readOnly;
    public bool multi;

    public PortAttribute(bool output)
    {
        this.Output = output;
        readOnly = false;
        multi = output;
    }
    public PortAttribute(bool output, bool read)
    {
        this.Output = output;
        readOnly = read;
        multi = output;
    }
    public PortAttribute(bool output, bool read, bool multi)
    {
        this.Output = output;
        readOnly = read;
        this.multi = multi;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class PortDecoratorAttribute : Attribute
{
    public CustomPort.Style Shape;
    public PortDecoratorAttribute(CustomPort.Style s)
    {
        Shape = s;
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

// Dummy class for logic ports
public class PortLogic {}