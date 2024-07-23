using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletInspectorView : VisualElement
{
   public new class UxmlFactory: UxmlFactory<BulletInspectorView, UxmlTraits> {}

   private NodeEditor editor;
   
   public BulletInspectorView()
   {
      
   }

   public void UpdateSelection(NodeView node)
   {
      Clear(); 
      Object.DestroyImmediate(editor);
      editor = (NodeEditor)Editor.CreateEditor(node.node, typeof(NodeEditor));
      editor.notAttached = true;
      IMGUIContainer container = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
      Add(container);
   }
}
