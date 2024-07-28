using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Animate")]
public class AnimateSplitNode: Node
{
   [Port(false)] public List<Bullet> input;
   [Port(true, false, false)] public List<Bullet> o1, o2;
   [Editable] public string id;

   public override void Reset()
   {
      o1 = new();
      o2 = new();
   }

   public override string Name => "Split (Animator)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      // var b = input[0];
      // input.RemoveAt(0);
      //
      // var n1 = GetConnection("o1");
      // if (n1.other && !state.test)
      // {
      //    n1.other.Eval(state);
      // }
      // var n2 = GetConnection("o2");
      // if (n2.other && !state.test)
      // {
      //    n2.other.Eval(state);
      // }
      return state;
   }
}