using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class FireNode: Node
{
   [Port(true, true)] public Bullet output;
   [Port(false)] public GameObject bullet;
   [Port(false)] public Vector3 position;
   [Port(false)] public Vector3 direction;
   [Editable] public string id;
   public override string Name => "Fire";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = state.Repeat();

      var i = 0;
      while (state.state.Repeat && i<10000)
      {
         i++;
         state = GetAllInputs(state.Reset());
         if (!state.state.Virtual)
         {
            var b = Instantiate(bullet, position, Quaternion.LookRotation(direction), state.attached.transform);
            output = new(b, position, direction);
            
            var next = GetConnection("output");
            if (next.other && !state.test)
            {
               
               var s = state;
               next.other.Eval(state);
            }
            //Debug.Log($"I will create a fish at {position} facing {direction}. Yep.");
         }
         state = AfterIter(state);
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