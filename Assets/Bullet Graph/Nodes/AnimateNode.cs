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
   [Editable] public int maxAnimationLength;
   private List<Anim> animators;
   private bool c;
   [Editable] public string id;

   public override void ClearState()
   {
      animators = new();
      c = false;
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
         if (!c)
         {
            c = true;
            
            if (state.test) TestAnimate();
            else state.attached.GetComponent<MonoBehaviour>().StartCoroutine(Animate());
         }
         
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
      // float t = 0;
      // float globalT = 0;
      // List<Anim> remove = new();
      //
      // while (globalT < maxAnimationLength && animators.Count > 0)
      // {
      //    t += Time.deltaTime;
      //    globalT += Time.deltaTime;
      //
      //    while (t > 1f / 60)
      //    {
      //       t -= 1f / 60;
      //       foreach (var a in animators)
      //       {
      //          if (!a.b.main)
      //          {
      //             remove.Add(a);
      //             continue;
      //          }
      //          
      //          var next = GetConnection("output");
      //          if (next.other)
      //          {
      //             pos = a.b.startPos;
      //             dir = a.b.startDir;
      //             index = a.id;
      //             time = a.s.iterationId/60f;
      //             output = a.b;
      //          
      //             next.other.Eval(a.s);
      //             a.s.iterationId++;
      //          }
      //          
      //          foreach (var r in remove)
      //          {
      //             animators.Remove(r);
      //          }
      //          remove.Clear();
      //       }
      //    }
      //    yield return new WaitForSeconds(1f / 60);
      // }
      //
      // Debug.Log($"finished animation");
      yield break;
   }
   
   async Awaitable TestAnimate()
   {
      // float t = 0;
      // float globalT = 0;
      // List<Anim> remove = new();
      //
      // while (globalT < maxAnimationLength && animators.Count > 0)
      // {
      //    t += 1/60f; //Time.dt
      //    globalT += 1/60f;
      //
      //    while (t > 1f / 60)
      //    {
      //       t -= 1f / 60;
      //       foreach (var a in animators)
      //       {
      //          if (!a.b.main)
      //          {
      //             remove.Add(a);
      //             continue;
      //          }
      //          var next = GetConnection("output");
      //          if (next.other)
      //          {
      //             pos = a.b.startPos;
      //             dir = a.b.startDir;
      //             index = a.id;
      //             time = a.s.iterationId/60f;
      //             output = a.b;
      //          
      //             next.other.Eval(a.s);
      //             a.s.iterationId++;
      //          }
      //       }
      //
      //       foreach (var r in remove)
      //       {
      //          animators.Remove(r);
      //       }
      //       remove.Clear();
      //    }
      //    await Task.Delay(16);
      // }
      //
      // Debug.Log($"finished animation");
   }
}