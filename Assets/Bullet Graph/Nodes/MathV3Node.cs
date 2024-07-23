
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class MathV3Node: Node
{
   
   [Port(false)] public Vector3 vec1;
   [Port(false)] public Vector3 vec2;
   [Port(true)] public Vector3 output;
   [Editable][SerializeField] private Operation mode;
   public override string Name => "Math (Vector3)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      
      switch (mode)
      {
         case Operation.add:
            output = vec1 + vec2;
            break;
         case Operation.subtract:
            output = vec1 - vec2;
            break;
         case Operation.multiply:
            output = Vector3.Scale(vec1, vec2);
            break;
         case Operation.reflect:
            output = Vector3.Reflect(vec1, vec2);
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
      reflect
   }
}
