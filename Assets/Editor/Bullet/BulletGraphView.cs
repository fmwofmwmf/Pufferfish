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
      
      var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Bullet/BulletGraphEditor.uss");
      styleSheets.Add(stylesheet);
   }

   public void PopulateView(BulletGraph graph)
   {
      currentGraph = graph;

      graphViewChanged -= OnGraphViewChanged;
      DeleteElements(graphElements);
      graphViewChanged += OnGraphViewChanged;
      
      graph.nodes.ForEach(node => CreateNodeView(node));
      graph.edges.ForEach(edge =>
      {
         CustomPort inPort = edge.inputPort;
         CustomPort outPort = edge.outputPort;
         inPort.Connect(edge);
         outPort.Connect(edge);
         
         Port ip = FindNodeView(inPort.owner).Ports[inPort];
         Port op = FindNodeView(outPort.owner).Ports[outPort];
         
         Edge e = ip.ConnectTo(op);
         AddElement(e);
      });
   }

   NodeView FindNodeView(Node node)
   {
      return GetNodeByGuid(node.guid) as NodeView;
   }
   
   NodeView FindPort(Node node)
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
               n.DisconnectAll();
               currentGraph.DeleteNode(n.Node);
            } 
            else if (elem is Edge e)
            {
               NodeView n1 = e.input.node as NodeView;
               NodeView n2 = e.output.node as NodeView;
               
               VisualPort p1 = e.input as VisualPort;
               VisualPort p2 = e.output as VisualPort;
               
               p1.UnCollapse();
               currentGraph.Disconnect(n1.Node, n2.Node, p1.Field, p2.Field);
            }
         }
      }

      if (graphviewchange.edgesToCreate != null)
      {
         graphviewchange.edgesToCreate.ForEach(edge =>
         {
            NodeView n1 = edge.input.node as NodeView;
            NodeView n2 = edge.output.node as NodeView;
               
            VisualPort p1 = edge.input as VisualPort;
            VisualPort p2 = edge.output as VisualPort;
            
            p2.Collapse();
            currentGraph.Connect(n1.Node, n2.Node, p1.Field, p2.Field);
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
      NodeView nodeView = new(this, node);
      nodeView.OnNodeSelected = onNodeSelected;
      AddElement(nodeView);
   }
}
