using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Timing")]
public class RepeatNode: Node
{ 
   [PortDecorator(CustomPort.Style.square)] [Port(false)] public PortLogic LogicIn;
   [PortDecorator(CustomPort.Style.square)] [Port(true)] public PortLogic LogicOut;
   [Port(false)] public float max, min, increment;
   [Port(true)] public float f;
   [Port(true, true)] public float readF;
   
   private float c;
   private bool stopProp;
   private TreeState lastUpstream;
   private int iterId, activeId;
   public override string Name => "Repeat";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      if (!stopProp)
      {
         state = GetAllInputs(state);
         if (!state.state.Repeat && activeId == state.activationId && iterId == state.iterationId) return state;
         activeId = state.activationId;
         iterId = state.iterationId;
         stopProp = true;
         lastUpstream = state.state.Clone();
         if (state.state.Error) return state;
      }

      if (increment == 0 || (max - min) / increment < 0)
      {
         Debug.Log($"Invalid loop conditions: {max}, {min}, {increment}");
         return state.Error();
      }
      
      if (c < max-increment)
      {
         f = c;
         readF = c;
         c += increment;
         return state.Repeat();
      }

      bool b = c > max;
      f = c;
      readF = c;
      c = min;
      stopProp = false;
      return b ? state.Set(lastUpstream).Virt() : state.Set(lastUpstream);
   }
   

   public override void ClearState()
   {
      c = 0;
      stopProp = false;
   }
}
