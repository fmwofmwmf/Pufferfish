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
    public BulletGraph g;
    public CustomPort InputPort {get
    {
        ReInit();
        return ip;
    }}
    public CustomPort OutputPort {get
    {
        ReInit();
        return op;
    }}
    private CustomPort ip, op;
    public string inName, outName;
    [SerializeReference] public Node inputNode, outputNode;
    public Type InType, OutType;
    public bool remove;

    public CustomEdge(BulletGraph g, CustomPort inputPort, CustomPort outputPort, Node inNode, Node outNode)
    {
        this.g = g;
        guid = "";
        ip = inputPort;
        op = outputPort;
        inName = inputPort.fieldName;
        outName = outputPort.fieldName;
        inputNode = inNode;
        outputNode = outNode;
        InType = inputPort.FieldType;
        OutType = outputPort.FieldType;
        remove = false;
    }

    public void ReInit()
    {
        ip = inputNode.FindInputPort(inName);
        op = outputNode.FindOutputPort(outName);
    }
    
    public void DeleteEdge()
    {
        remove = true;
        InputPort.RemoveEdge(this);
        OutputPort.RemoveEdge(this);

        inputNode.CalculateTransfer();
        outputNode.CalculateTransfer();
        g.edges.Remove(this);
    }
}
