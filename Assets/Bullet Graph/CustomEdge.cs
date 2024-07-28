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
public class CustomEdge
{
    public string guid;
    [FormerlySerializedAs("inputNode")] public CustomPort inputPort;
    [FormerlySerializedAs("outputNode")] public CustomPort outputPort;
    public Type InType, OutType;

    public CustomEdge(CustomPort i, CustomPort o)
    {
        inputPort = i;
        outputPort = o;
    }

    public void DeleteEdge()
    {
        inputPort.RemoveEdge(this);
        outputPort.RemoveEdge(this);
    }
}
