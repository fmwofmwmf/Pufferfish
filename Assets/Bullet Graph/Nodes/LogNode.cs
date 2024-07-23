
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class LogNode: Node
{
   
   [Editable] public string message;
   [Port(false)] public float count;

   public override string Name => "Log";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = state.Repeat();
      var i = 0;
      while (state.state.Repeat && i<1000)
      {
         i++;
         state = GetAllInputs(state.Reset());
         if (!state.state.Virtual) Debug.Log($"{message} ({count}) ({state.state.Repeat})");
         else Debug.Log($"{message} ({count}) ({state.state.Repeat}) Virt");
         state = AfterIter(state);
      } 
      return state;
   }
}
