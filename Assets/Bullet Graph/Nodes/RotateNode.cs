using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class RotateNode: Node
{
   [Port(false)] public Vector3 center;
   [Port(false)] public float dist, angle;
   [Port(true)] public Vector3 output;
   public override string Name => "Rotate";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;

      output = (Vector3) new Rotation(angle).Dir * dist + center;
      
      return state;
   }
}
