
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class SwizzleNode: Node
{
   [Port(false)] public Vector3 input;
   [Port(true)] public Vector3 output;
   [Port(true)] public Vector2 output2;
   [Editable][SerializeField] private Operation x, y, z;
   public override string Name => "Swizzle";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;

      output.x = Picker(input, x);
      output.y = Picker(input, y);
      output.z = Picker(input, z);
      output2 = output;
      
      return state;
   }

   private float Picker(Vector3 i, Operation o)
   {
      switch (o)
      {
         case Operation.x:
            return i.x;
         case Operation.y:
            return i.y;
         case Operation.z:
            return i.z;
         case Operation.zero:
            return 0;
      }

      return 0;
   }
   
   [Serializable]
   private enum Operation
   {
      x,
      y,
      z,
      zero
   }
}
