using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class BulletGraphEditor : EditorWindow
{
    private BulletGraphView graphView;
    private BulletInspectorView inspectorView;

    [MenuItem("Bullet/Editor")]
    public static void Open()
    {
        BulletGraphEditor wnd = GetWindow<BulletGraphEditor>();
        wnd.titleContent = new GUIContent("Bullet Graph");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/BulletGraphEditor.uxml");
        visualTree.CloneTree(root);

        // Instantiate UXML
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BulletGraphEditor.uss");
        root.styleSheets.Add(stylesheet);

        graphView = root.Q<BulletGraphView>();
        inspectorView = root.Q<BulletInspectorView>();
        graphView.onNodeSelected = OnNodeSelectionChange;
        OnSelectionChange();
    }
    

    private void OnSelectionChange()
    {
        BulletGraph graph = Selection.activeObject as BulletGraph;
        if (graph)
        {
            graphView.PopulateView(graph);
        }
    }
    
    [OnOpenAsset(1)]
    public static bool OpenGameStateWindow(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        
        if (!(obj is BulletGraph g)) { return false; }

        bool windowIsOpen = HasOpenInstances<BulletGraphEditor>();
        if (!windowIsOpen)
        {
            CreateWindow<BulletGraphEditor>();
        }
        else
        {
            FocusWindowIfItsOpen<BulletGraphEditor>();
        }
 
        // Window should now be open, proceed to next step to open file
        return false;
    }
 
    [OnOpenAsset(2)]
    public static bool OpenGameStateFlow(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        
        if (!(obj is BulletGraph g)) { return false; }
        
        var window = GetWindow<BulletGraphEditor>();

        window.OnSelectionChange();
 
        return true;
    }

    void OnNodeSelectionChange(NodeView node)
    {
        inspectorView.UpdateSelection(node);
    }
}
