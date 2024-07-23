
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class SplitV3Node: Node
{
   
   [Port(false)] public Vector3 input;
   [Port(true)] public float x;
   [Port(true)] public float y;
   [Port(true)] public float z;
   
   public override string Name => "Split Vector";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      x = input.x;
      y = input.y;
      z = input.z;
      return state;
   }
}
