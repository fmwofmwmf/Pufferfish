
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Float")]
public class MathFNode: Node
{
   
   [Port(false)] public float float1;
   [Port(false)] public float float2;
   [Port(true)] public float output;
   [Editable][SerializeField] private Operation mode;
   public override string Name => "Math (Float)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state.Error();
      
      switch (mode)
      {
         case Operation.add:
            output = float1 + float2;
            break;
         case Operation.subtract:
            output = float1 - float2;
            break;
         case Operation.multiply:
            output = float1 * float2;
            break;
         case Operation.divide:
            if (float2 != 0) output = float1 / float2;
            else output = 0;
            break;
      }
      return state;
   }
   
   [Serializable]
   private enum Operation
   {
      add,
      subtract,
      multiply,
      divide
   }
}
