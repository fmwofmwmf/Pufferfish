
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Float")]
public class MathFNode: Node
{
   
   [PortName("")] [Port(false)] public float float1;
   [PortName("")] [Port(false)] public float float2;
   [PortName("")] [Port(true)] public float output;
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
         case Operation.mod:
            output = float1 % float2;
            break;
         case Operation.sinCos:
            output = Mathf.Sin(Mathf.Deg2Rad * float1) + Mathf.Cos(Mathf.Deg2Rad * float2);
            break;
         case Operation.divide:
            if (float2 != 0) output = float1 / float2;
            else output = 0;
            break;
         case Operation.floorDiv:
            if (float2 != 0) output = Mathf.Floor(float1 / float2);
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
      mod,
      sinCos,
      divide,
      floorDiv
   }
}
