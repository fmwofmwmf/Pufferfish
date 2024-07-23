using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CreateAssetMenu()]
public class BulletGraph : ScriptableObject
{
    public List<Node> nodes = new();

    public Node CreateNode(Type type)
    {
        Node node = CreateInstance(type) as Node;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        nodes.Add(node);
        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();
        node.Start();
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
        node.DetachAll();
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }

    public void Connect(Node node1, Node node2, FieldInfo port1, FieldInfo port2)
    {
        node1.Connect(port1, port2, node2);
        node2.Connect(port2, port1, node1);
    }
    public void Disconnect(Node node1, Node node2, FieldInfo port1, FieldInfo port2)
    {
        node1.Disconnect(port1.Name);
        node2.Disconnect(port2.Name);
    }
    public void GetConnected(Node node)
    {
        
    }
}
