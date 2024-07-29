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

      graph.InitializeEdges();
          
      graph.nodes.ForEach(node => CreateNodeView(node));

      for (int i = graph.edges.Count - 1; i >= 0; i--)
      {
         var edge = graph.edges[i];

         Port ip = FindNodeView(edge.inputNode).Ports[edge.inName].port;
         Port op = FindNodeView(edge.outputNode).Ports[edge.outName].port;
         
         Edge e = ip.ConnectTo(op);
         AddElement(e);
      }

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
      return ports.ToList().Where(end =>
      {
         VisualPort e = end.parent as VisualPort;
         VisualPort s = startPort.parent as VisualPort;
         
         return e != null && s != null && end.node != startPort.node && end.portType == startPort.portType &&
                end.direction != startPort.direction && e.Shape == s.Shape;
      }).ToList();
   }

   private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
   {
      if (graphviewchange.elementsToRemove != null)
      {
         for (int i = graphviewchange.elementsToRemove.Count - 1; i >= 0; i--)
         {
            var elem = graphviewchange.elementsToRemove[i];
            if (elem is NodeView n)
            {
               n.DisconnectAll();
               currentGraph.DeleteNode(n.Node);
               RemoveElement(n);
            } 
            else if (elem is Edge e)
            {
               NodeView n1 = e.input.node as NodeView;
               NodeView n2 = e.output.node as NodeView;
            
               VisualPort p1 = e.input.parent as VisualPort;
               VisualPort p2 = e.output.parent as VisualPort;
            
               p1.UnCollapse();
               currentGraph.Disconnect(n1.Node, n2.Node, p1.Field, p2.Field);
               RemoveElement(e);
            }
         }
      }

      if (graphviewchange.edgesToCreate != null)
      {
         graphviewchange.edgesToCreate.ForEach(edge =>
         {
            NodeView n1 = edge.input.node as NodeView;
            NodeView n2 = edge.output.node as NodeView;
               
            VisualPort p1 = edge.input.parent as VisualPort;
            VisualPort p2 = edge.output.parent as VisualPort;
            
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

         Vector2 position = evt.mousePosition;
         
         position.x = (position.x - contentViewContainer.worldBound.x) / scale;
         position.y = (position.y - contentViewContainer.worldBound.y) / scale;
         
         foreach (var t in types)
         {
            if (t.GetCustomAttribute(typeof(NodeAttribute)) is NodeAttribute s)
            {
               evt.menu.AppendAction($"{s.path}/{t.Name}", _ => CreateNode(t, position));
            }
            else
            {
               evt.menu.AppendAction($"{t.Name}", _ => CreateNode(t, position));
            }
            
         }
      }
   }

   void CreateNode(Type type, Vector2 position)
   {
      Node node = currentGraph.CreateNode(type, position);
      CreateNodeView(node);
   }

   void CreateNodeView(Node node)
   {
      NodeView nodeView = new(this, node);
      nodeView.OnNodeSelected = onNodeSelected;
      AddElement(nodeView);
   }
}
