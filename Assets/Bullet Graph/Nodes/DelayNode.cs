
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[Node("Timing")]
public class DelayNode: Node
{
   [PortDecorator(CustomPort.Style.square)] [Port(false)] public PortLogic LogicIn;
   [PortDecorator(CustomPort.Style.square)] [Port(true)] public PortLogic LogicOut;
   [Port(false)] public float delay;
   private int skips;
   public override string Name => "Delay";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (state.state.Error) return state;
      state.state.delay = delay;
      return state;
   }

}
