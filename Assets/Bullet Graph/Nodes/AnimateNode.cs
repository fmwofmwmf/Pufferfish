using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Node("Animate")]
public class AnimateNode: Node
{
   [Port(false)] public Bullet bullet;
   [Port(true, true)] public Bullet output;
   [Port(true, true)] public Vector3 pos, dir;
   [Port(true, true)] public float index, time;
   private List<Anim> animators;
   private Coroutine c;
   [Editable] public string id;

   public override void Reset()
   {
      animators = new();
      c = null;
   }

   public override string Name => "Animate";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (bullet != null)
      {
         Debug.Log($"Start!{state.iterationId}");
         var newState = state.Clone();
         newState.activationId = Random.Range(0, 57890190);
         newState.iterationId = 0;
         
         animators.Add(new() {b = bullet, s = newState, id = state.iterationId});
         c = state.attached.GetComponent<MonoBehaviour>().StartCoroutine(Animate());
         
         bullet = null;
      }
      
      return state;
   }

   private class Anim
   {
      public Bullet b;
      public TreeStateData s;
      public int id;
   }
   
   IEnumerator Animate()
   {
      float t = 0;
      float globalT = 0;
      while (globalT < 10)
      {
         t += Time.deltaTime;
         globalT += Time.deltaTime;

         while (t > 1f / 60)
         {
            t -= 1f / 60;
            foreach (var a in animators)
            {
               var next = GetConnection("output");
               if (next.other)
               {
                  pos = a.b.startPos;
                  dir = a.b.startDir;
                  index = a.id;
                  time = a.s.iterationId/60f;
                  output = a.b;
               
                  next.other.Eval(a.s);
                  a.s.iterationId++;
               }
            }
         }
         
         yield return new WaitForSeconds(0);
      }
      
      yield break;
   }
}