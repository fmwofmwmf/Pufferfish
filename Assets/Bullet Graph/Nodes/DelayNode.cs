
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Timing")]
public class DelayNode: Node
{
   
   [Port(false)] public float delay;
   [Port(false)] public float inF;
   [Port(true)] public float outF;
   [Port(false)] public Vector3 inV3;
   [Port(true)] public Vector3 outV3;
   private int skips;
   public override string Name => "Delay";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      if (skips > 0)
      {
         skips--;
         return state.Virt().Repeat();
      }
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      outF = inF;
      outV3 = inV3;
      skips = (int)(delay * 60);

      return state;
   }

}
