using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class FireNode: Node
{
   [PortDecorator(CustomPort.Style.square)] [Port(false)] public PortLogic LogicIn;
   [PortDecorator(CustomPort.Style.square)] [Port(true, true)] public Bullet output;
   [Port(false)] public GameObject bullet;
   [Port(false)] public Vector3 position;
   [Port(false)] public Vector3 direction;
   [Editable] public string id;
   public override string Name => "Fire";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (!state.state.Virtual)
      {
         var b = Instantiate(bullet, position, Mathfm.RightQ(direction), state.attached.transform);
         output = new(b, position, direction.normalized);
         
         var next = FindOutputPort("output");
         if (next.Edges.Any())
         {
            b.SetActive(false);
            next.Edges[0].inputNode.Eval(state);
         }
      }

      return state;
   }
}

[Serializable]
public class Bullet
{
   public GameObject main;
   public Vector3 startPos, startDir;
   public int localT;
   
   public Bullet(GameObject m, Vector3 p, Vector3 d) {
      main = m;
      startPos = p;
      startDir = d;
   }
}