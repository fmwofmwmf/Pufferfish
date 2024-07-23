using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Animate")]
public class AnimateDelayNode: Node
{
   [Port(false)] public List<Bullet> bullet;
   [Port(false)] public List<Bullet> output;
   [Editable] public string id;
   public override void Reset()
   {
      output = new();
   }
   public override string Name => "Delay (Animator)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      return state;
   }
}