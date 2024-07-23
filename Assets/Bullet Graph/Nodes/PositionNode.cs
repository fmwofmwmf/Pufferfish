using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Input")]
public class PositionNode: Node
{
   [Port(true)] public Vector3 position;
   [Port(true)] public Vector3 facing;
   [Editable] public PositionSelector target;
   [Editable] public ModeSelector mode;
   public enum PositionSelector
   {
      attached,
      player
   }
   public enum ModeSelector
   {
      normal,
      local
   }
   public override string Name => "Position";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      GameObject t = null;
      switch (target)
      {
         case PositionSelector.attached:
            t = state.attached;
            break;
         case PositionSelector.player:
            t = Player.main.GameObject();
            break;
      }

      if (t == null) return state.Error();
      switch (mode)
      {
         case ModeSelector.normal:
            position = t.transform.position;
            facing = t.transform.right;
            break;
         case ModeSelector.local:
            position = t.transform.localPosition;
            facing = t.transform.localRotation * Vector3.right;
            break;
      }
      
      return state;
   }
}
