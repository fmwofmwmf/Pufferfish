
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class MathV3Node2: Node
{
   [Port(false)] public Vector3 vec1;
   [Port(false)] public Vector3 vec2;
   [Port(false)] public float amount;
   [Port(true)] public Vector3 output;
   [Editable][SerializeField] private Operation mode;
   public override string Name => "Math (Vector3 F)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      
      switch (mode)
      {
         case Operation.lerp:
            output = Vector3.LerpUnclamped(vec1, vec2, amount);
            break;
         case Operation.rotateAbout:
            output = Quaternion.AngleAxis(amount, vec2) * vec1;
            break;
      }
      
      return state;
   }
   
   [Serializable]
   private enum Operation
   {
      lerp,
      rotateAbout,
      
   }
}
