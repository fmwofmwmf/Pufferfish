using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Animate")]
public class AnimateMoveNode: Node
{ 
    [PortDecorator(CustomPort.Style.cap)] [Port(false)] public Bullet input; 
    [Port(false)] public Vector3 position; 
    [Port(false)] public Vector3 direction;
    [Editable] public string id;
   public override void ClearState()
   {
       
   }

   public override string Name => "Move (Animator)";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
       GetAllInputs(state);
       //Debug.Log(state.iterationId);
       input.main.transform.position = position;

       return state;
   }
}