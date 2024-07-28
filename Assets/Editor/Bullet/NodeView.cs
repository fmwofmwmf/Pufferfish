using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelected;
    public Node Node;
    public Dictionary<string, VisualPort> Ports;
    public BulletGraphView Graph;
    public NodeView(BulletGraphView g, Node node)
    {
        Node = node;
        Graph = g;
        title = node.Name;
        viewDataKey = node.guid;
        style.overflow = Overflow.Visible;
        mainContainer.style.overflow = Overflow.Visible;
        style.left = node.graphPos.x;
        style.top = node.graphPos.y;
        Ports = new();
        CreateButtons();
        CreateInputPorts();
        CreateOutputPorts();
    }

    void CreateInputPorts()
    {
        foreach (var port in Node.inputPorts)
        {
            var p = VisualPort.Create(Direction.Input, port.multi? Port.Capacity.Multi : Port.Capacity.Single, port, Node);
            if (port.Edges.Any()) p.Collapse();
            p.port.portName = TypeName(port.displayName, port.FieldType);
            Ports[port.fieldName] = p;
            inputContainer.Add(p);
        }
    }
    
    void CreateOutputPorts()
    {
        foreach (var port in Node.outputPorts)
        {
            var p = VisualPort.Create(Direction.Output, port.multi? Port.Capacity.Multi : Port.Capacity.Single, port, Node);
            p.port.portName = TypeName(port.displayName, port.FieldType);
            Ports[port.fieldName] = p;
            outputContainer.Add(p);
        }
    }

    void CreateButtons()
    {
        var editor = Editor.CreateEditor(Node, typeof(NodeEditor));
        IMGUIContainer container = new IMGUIContainer(() =>
        {
            editor.OnInspectorGUI();
        });
        container.style.backgroundColor = new StyleColor(new Color(56/255f, 56/255f, 56/255f, .7f));
        Add(container);
    }

    public void DisconnectAll()
    {
        Ports.Values.ToList().ForEach(v => Graph.DeleteElements(v.port.connections));
    }

    string TypeName(string fieldName, Type f)
    {
        string n;
        switch (f)
        {
            case { } t when t == typeof(string):
                n = "str";
                break;
            case { } t when t == typeof(int):
                n = "int";
                break;
            case { } t when t == typeof(float):
                n = "1";
                break;
            case { } t when t == typeof(Vector2):
                n = "2";
                break;
            case { } t when t == typeof(Vector3):
                n = "3";
                break;
            default:
                n = f.Name;
                break;
        }
        
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return $"{textInfo.ToTitleCase(fieldName)}({n})";
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Node.graphPos = newPos.min;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (OnNodeSelected != null)
        {
            OnNodeSelected.Invoke(this);
        }
    }
}


public class VisualPort : VisualElement
{
    public Port port;
    public VisualElement textField { get; private set; }
    public string Field;

    private VisualPort(Direction portDirection, Port.Capacity portCapacity, CustomPort attached, Node n)
    {
        port = Port.Create<Edge>(Orientation.Horizontal, portDirection, portCapacity, attached.FieldType);
        Add(port);
        style.overflow = Overflow.Visible;
        Field = attached.fieldName;
        if (portDirection == Direction.Input)
        {
            textField = CreateField(attached.fieldName, attached.FieldType, n);
            if (textField != null) Add(textField);
        }
    }
    
    public static VisualPort Create(Direction portDirection, Port.Capacity portCapacity, CustomPort attached, Node n)
    {
        return new VisualPort(portDirection, portCapacity, attached, n);
    }

    public void Collapse()
    {
        if (textField != null) textField.visible = false;
    }
    
    public void UnCollapse()
    {
        if (textField != null) textField.visible = true;
    }

    private VisualElement CreateField(string portName, Type fieldType, Node n)
    {
        VisualElement fieldContainer = new VisualElement();
        fieldContainer.style.overflow = Overflow.Visible;
        fieldContainer.style.flexDirection = FlexDirection.Row;
        fieldContainer.style.position = Position.Absolute;
        
        fieldContainer.style.top = 0;
        fieldContainer.style.backgroundColor = new StyleColor(new Color(56/255f, 56/255f, 56/255f, .7f));
        fieldContainer.style.paddingBottom = 3;
        fieldContainer.style.paddingTop = 3;
        fieldContainer.style.paddingLeft = 3;
        fieldContainer.style.paddingRight = 3;
        // Create a field based on port name or any other logic
        
        CustomPort p = n.FindInputPort(portName);
        if (fieldType == typeof(int))
        {
            IntegerField inputField = new IntegerField();
            
            
            inputField.value = (int)p.defaultValue.ExtractValue(typeof(int));
            fieldContainer.Add(new Label("Int"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.intValue = evt.newValue);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else if (fieldType == typeof(float))
        {
            FloatField inputField = new FloatField();

            inputField.value = (float)p.defaultValue.ExtractValue(typeof(float));
            fieldContainer.Add(new Label("Float"));
            inputField.RegisterValueChangedCallback(evt=> p.defaultValue.floatValue = evt.newValue);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else if (fieldType == typeof(string))
        {
            TextField inputField = new TextField();
            
            inputField.value = (string)p.defaultValue.ExtractValue(typeof(string));
            fieldContainer.Add(new Label("String"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.stringValue = evt.newValue);
            fieldContainer.Add(inputField);
        }
        else if (fieldType == typeof(bool))
        {
            Toggle inputField = new Toggle();
            
            inputField.value = (bool)p.defaultValue.ExtractValue(typeof(bool));
            fieldContainer.Add(new Label("Boolean"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.boolValue = evt.newValue);
            fieldContainer.Add(inputField);
        }
        else if (fieldType == typeof(Vector2))
        {
            Vector2Field inputField = new Vector2Field();
            
            inputField.value = (Vector2)p.defaultValue.ExtractValue(typeof(Vector2));
            fieldContainer.Add(new Label("(2)"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.vector2Value = evt.newValue);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -120; // Adjust as needed
        }
        else if (fieldType == typeof(Vector3))
        {
            Vector3Field inputField = new Vector3Field();

            inputField.value = (Vector3)p.defaultValue.ExtractValue(typeof(Vector3));
            fieldContainer.Add(new Label("(3)"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.vector3Value = evt.newValue);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -160; // Adjust as needed
        }
        else if (fieldType == typeof(Color))
        {
            ColorField inputField = new ColorField();

            inputField.value = (Color)p.defaultValue.ExtractValue(typeof(Color));
            fieldContainer.Add(new Label("Color"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.colorValue = evt.newValue);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else if (fieldType == typeof(GameObject))
        {
            ObjectField inputField = new ObjectField
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false
            };

            inputField.value = (GameObject)p.defaultValue.ExtractValue(typeof(GameObject));
            fieldContainer.Add(new Label(""));
            inputField.RegisterValueChangedCallback(evt=> p.defaultValue.gameObjectValue = evt.newValue as GameObject);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -160; // Adjust as needed
        }
        else if (fieldType == typeof(Transform))
        {
            ObjectField inputField = new ObjectField
            {
                objectType = typeof(Transform),
                allowSceneObjects = false
            };

            inputField.value = (Transform)p.defaultValue.ExtractValue(typeof(Transform));
            fieldContainer.Add(new Label("Transform"));
            inputField.RegisterValueChangedCallback(evt => p.defaultValue.transformValue = evt.newValue as Transform);
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else
        {
            return null;
        }

        
        return fieldContainer;
    }

    
}