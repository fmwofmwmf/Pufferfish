using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CreateAssetMenu()]
public class BulletGraph : ScriptableObject
{
    [SerializeReference] public List<Node> nodes = new();
    public List<CustomEdge> edges = new();

    public Node CreateNode(Type type, Vector2 position)
    {
        Node node = CreateInstance(type) as Node;
        node.graphPos = position;
        node.Reset();
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        nodes.Add(node);
        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();
        node.Reset();
        return node;
    }

    public void Reset() 
    {
        foreach (var n in nodes)
        {
            n.Reset();
        }
    }

    public void DeleteNode(Node node)
    {
        nodes.Remove(node);
        node.DisconnectAll();
        edges.RemoveAll(e=> e.inputNode == node || e.outputNode == node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }

    public void Connect(Node inNode, Node outNode, string inPort, string outPort)
    {
        if (inNode == outNode)
        {
            Debug.Log("Do not connect a node to itself!");
            return;
        }
        CustomPort p1 = inNode.FindInputPort(inPort);
        CustomPort p2 = outNode.FindOutputPort(outPort);
        CustomEdge e = new CustomEdge(this, p1, p2, inNode, outNode);
        edges.Add(e);
        
        p1.Connect(e);
        p2.Connect(e);
    }
    public void Disconnect(Node inNode, Node outNode, string inPort, string outPort)
    {
        CustomPort p1 = inNode.FindInputPort(inPort);
        CustomPort p2 = outNode.FindOutputPort(outPort);
        CustomEdge e = p1.FindConnectedEdge(p2);
        edges.Remove(e);
        p1.RemoveEdge(e);
        p2.RemoveEdge(e);
    }
}
