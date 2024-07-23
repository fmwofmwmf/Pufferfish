using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletGraphView : GraphView
{
   public Action<NodeView> onNodeSelected;
   public new class UxmlFactory: UxmlFactory<BulletGraphView, UxmlTraits> {}

   private BulletGraph currentGraph;
   
   public BulletGraphView()
   {
      Insert(0, new GridBackground());
      
      this.AddManipulator(new ContentDragger());
      this.AddManipulator(new SelectionDragger());
      this.AddManipulator(new ContentZoomer());
      this.AddManipulator(new RectangleSelector());
      
      var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BulletGraphEditor.uss");
      styleSheets.Add(stylesheet);
   }

   public void PopulateView(BulletGraph graph)
   {
      currentGraph = graph;

      graphViewChanged -= OnGraphViewChanged;
      DeleteElements(graphElements);
      graphViewChanged += OnGraphViewChanged;
      
      graph.nodes.ForEach(n => CreateNodeView(n));
      graph.nodes.ForEach(n =>
      {
         var nv = FindNodeView(n);
         
         var fields = n.GetAllFields();
         foreach (var f in fields)
         {
            var port = nv.connections[f];
            var otherPort = n.GetConnection(f.Name);
            if (port.port.direction == Direction.Input && otherPort != null && otherPort.field != "")
            {
               var othernv = FindNodeView(otherPort.other);
               
               var otherField = othernv.node.GetAllFields().Find(f => f.Name == otherPort.field);
               
               var otherVPort = othernv.connections[otherField];
               port.Collapse(); 
               Edge e = otherVPort.port.ConnectTo(port.port);
               AddElement(e);
            }
         }
         
      });
   }

   NodeView FindNodeView(Node node)
   {
      return GetNodeByGuid(node.guid) as NodeView;
   }
   
   public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
   {
      return ports.ToList().Where(end => end.node != startPort.node && end.portType == startPort.portType && end.direction != startPort.direction).ToList();
   }

   private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
   {
      if (graphviewchange.elementsToRemove != null)
      {
         foreach (var elem in graphviewchange.elementsToRemove)
         {
            if (elem is NodeView n)
            {
               foreach (var p in n.connections2.Keys)
               {
                  foreach (var e in p.connections)
                  {
                     if (e.parent != null) e.parent.Remove(e);
                  }
               }

               currentGraph.DeleteNode(n.node);
            } else if (elem is Edge e)
            {
               NodeView n1 = e.output.node as NodeView;
               NodeView n2 = e.input.node as NodeView;
               var f1 = n1.connections2[e.output];
               var f2 = n2.connections2[e.input];
               var c1 = n1.connections[f1];
               var c2 = n2.connections[f2];
               c2.UnCollapse();
               currentGraph.Disconnect(n1.node, n2.node, f1, f2);
            }
         }
      }

      if (graphviewchange.edgesToCreate != null)
      {
         graphviewchange.edgesToCreate.ForEach(edge =>
         {
            NodeView n1 = edge.output.node as NodeView;
            NodeView n2 = edge.input.node as NodeView;
            var f1 = n1.connections2[edge.output];
            var f2 = n2.connections2[edge.input];
            var c1 = n1.connections[f1];
            var c2 = n2.connections[f2];
            c2.Collapse();
            currentGraph.Connect(n1.node, n2.node, f1, f2);
         });
      }
      return graphviewchange;
   }

   public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
   {
      {
         var types = TypeCache.GetTypesDerivedFrom<Node>();
         foreach (var t in types)
         {
            if (t.GetCustomAttribute(typeof(NodeAttribute)) is NodeAttribute s)
            {
               evt.menu.AppendAction($"{s.path}/{t.Name}", _ => CreateNode(t));
            }
            else
            {
               evt.menu.AppendAction($"{t.Name}", _ => CreateNode(t));
            }
            
         }
      }
   }

   void CreateNode(Type type)
   {
      Node node = currentGraph.CreateNode(type);
      CreateNodeView(node);
   }

   void CreateNodeView(Node node)
   {
      NodeView nodeView = new(node);
      nodeView.onNodeSelected = onNodeSelected;
      AddElement(nodeView);
   }
}
