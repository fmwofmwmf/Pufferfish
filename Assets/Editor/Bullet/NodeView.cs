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
    public Action<NodeView> onNodeSelected;
    public Node node;

    public readonly Dictionary<FieldInfo, CustomPort> connections = new Dictionary<FieldInfo, CustomPort>();
    public readonly Dictionary<Port, FieldInfo> connections2 = new Dictionary<Port, FieldInfo>();
    public NodeView(Node node)
    {
        this.node = node;
        title = node.Name;
        viewDataKey = node.guid;
        style.overflow = Overflow.Visible;
        mainContainer.style.overflow = Overflow.Visible;
        style.left = node.graphPos.x;
        style.top = node.graphPos.y;
        CreateButtons();
        CreateInputPorts();
        CreateOutputPorts();
        
    }

    void CreateInputPorts()
    {
        var fields = node.GetAllFields()
            .Where(f => f.GetCustomAttribute<PortAttribute>().output == false);

        foreach (var field in fields)
        {
            var b = field.GetCustomAttribute<PortAttribute>().right;
            var p = CustomPort.Create(Direction.Input, b? Port.Capacity.Multi : Port.Capacity.Single, field, node);
            connections[field] = p;
            connections2[p.port] = field;
            if (p.port.connections.Any()) p.Collapse();
            p.port.portName = TypeName(field);
            
            inputContainer.Add(p);
        }
    }
    
    void CreateOutputPorts()
    {
        var fields = node.GetAllFields()
            .Where(f => f.GetCustomAttribute<PortAttribute>().output == true);
        
        foreach (var field in fields)
        {
            var b = field.GetCustomAttribute<PortAttribute>().right;
            var p = CustomPort.Create(Direction.Output, b? Port.Capacity.Multi : Port.Capacity.Single, field, node);
            connections[field] = p;
            connections2[p.port] = field;
            p.port.portName = TypeName(field);
            p.Collapse();
            outputContainer.Add(p);
        }
    }

    void CreateButtons()
    {
        var editor = Editor.CreateEditor(node, typeof(NodeEditor));
        IMGUIContainer container = new IMGUIContainer(() =>
        {
            editor.OnInspectorGUI();
        });
        container.style.backgroundColor = new StyleColor(new Color(56/255f, 56/255f, 56/255f, .7f));
        Add(container);
    }

    string TypeName(FieldInfo f)
    {
        string n;
        switch (f.FieldType)
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
                n = f.FieldType.Name;
                break;
        }
        
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return $"{textInfo.ToTitleCase(f.Name)}({n})";
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        node.graphPos = newPos.min;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (onNodeSelected != null)
        {
            onNodeSelected.Invoke(this);
        }
    }
}


public class CustomPort : VisualElement
{
    public Port port { get; private set; }
    public VisualElement textField { get; private set; }

    public CustomPort(Direction portDirection, Port.Capacity portCapacity, FieldInfo type, Node n)
    {
        port = Port.Create<Edge>(Orientation.Horizontal, portDirection, portCapacity, type.FieldType);
        style.overflow = Overflow.Visible;
        if (portDirection == Direction.Input)
        {
            textField = CreateField("Input", type, n);
            if (textField != null) Add(textField);
        }

        Add(port);
        
    }

    public void Collapse()
    {
        if (textField != null) textField.visible = false;
    }
    
    public void UnCollapse()
    {
        if (textField != null) textField.visible = true;
    }

    private VisualElement CreateField(string portName, FieldInfo f, Node n)
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

        var fieldType = f.FieldType;
        var p = n.GetConnection(f.Name);
        if (fieldType == typeof(int))
        {
            IntegerField inputField = new IntegerField();
            
            
            inputField.value = (int)p.defaultValue.ExtractValue(typeof(int));
            fieldContainer.Add(new Label("Int"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else if (fieldType == typeof(float))
        {
            FloatField inputField = new FloatField();

            inputField.value = (float)p.defaultValue.ExtractValue(typeof(float));
            fieldContainer.Add(new Label("Float"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else if (fieldType == typeof(string))
        {
            TextField inputField = new TextField();
            
            inputField.value = (string)p.defaultValue.ExtractValue(typeof(string));
            fieldContainer.Add(new Label("String"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
        }
        else if (fieldType == typeof(bool))
        {
            Toggle inputField = new Toggle();
            
            inputField.value = (bool)p.defaultValue.ExtractValue(typeof(bool));
            fieldContainer.Add(new Label("Boolean"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
        }
        else if (fieldType == typeof(Vector2))
        {
            Vector2Field inputField = new Vector2Field();
            
            inputField.value = (Vector2)p.defaultValue.ExtractValue(typeof(Vector2));
            fieldContainer.Add(new Label("(2)"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -120; // Adjust as needed
        }
        else if (fieldType == typeof(Vector3))
        {
            Vector3Field inputField = new Vector3Field();

            inputField.value = (Vector3)p.defaultValue.ExtractValue(typeof(Vector3));
            fieldContainer.Add(new Label("(3)"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -160; // Adjust as needed
        }
        else if (fieldType == typeof(Color))
        {
            ColorField inputField = new ColorField();

            inputField.value = (Color)p.defaultValue.ExtractValue(typeof(Color));
            fieldContainer.Add(new Label("Color"));
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
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
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
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
            inputField.RegisterValueChangedCallback(evt => n.UpdateDefault(f, evt.newValue));
            fieldContainer.Add(inputField);
            fieldContainer.style.left = -60; // Adjust as needed
        }
        else
        {
            return null;
        }

        
        return fieldContainer;
    }

    public static CustomPort Create(Direction portDirection, Port.Capacity portCapacity, FieldInfo type, Node n)
    {
        return new CustomPort(portDirection, portCapacity, type, n);
    }
}