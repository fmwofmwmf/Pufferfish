
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Vector")]
public class JoinVNode: Node
{
   
   [Port(false)] public float x;
   [Port(false)] public float y;
   [Port(false)] public float z;
   [Port(true)] public Vector3 vector3;
   [Port(true)] public Vector2 vector2;


   public override string Name => "Vector";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      
      vector2 = new(x, y);
      vector3 = new(x, y, z);
      return state;
   }
}
