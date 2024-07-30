using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Node("Animate")]
public class AnimateNode: Node
{
   [PortDecorator(CustomPort.Style.square)] [Port(false)] public Bullet bullet;
   [PortDecorator(CustomPort.Style.cap)] [Port(true, true)] public Bullet output;
   [Port(true, true)] public Vector3 pos, dir;
   [Port(true, true)] public float index, localTime, gTime;
   [Editable] public int maxAnimationLength;
   [Editable] public int testTickRate;
   private List<Anim> animators;
   private bool c;
   private float globalTime;
   [Editable] public string id;

   public override void ClearState()
   {
      animators = new();
      c = false;
      globalTime = 0;
   }

   public override string Name => "Animate";
   protected override TreeStateData Evaluate(TreeStateData state)
   {
      state = GetAllInputs(state);
      if (bullet != null)
      {
         var newState = state.Clone();
         newState.activationId = Random.Range(0, 57890190);
         newState.iterationId = 0;
         
         animators.Add(new() {b = bullet, s = newState, Id = state.iterationId});
         if (!c)
         {
            c = true;
            Debug.Log("starting!!");
            if (state.test) TestAnimate();
            else if (state.attached.TryGetComponent(out GraphPlayer p))
            {
               p.Play(this);
            }
         }
         
         bullet = null;
      }
      
      return state;
   }

   private class Anim
   {
      public Bullet b;
      public TreeStateData s;
      public int Id;
      public float LocalTime;
   }
   
   public bool Animate(float timeStep)
   {
      List<CustomEdge> next = FindOutputPort("output").Edges;
      globalTime += timeStep;
      if (globalTime > maxAnimationLength || animators.Count == 0) return true;
      //Debug.Log($"Animator tick {globalTime} {animators.Count}");
      Tick(timeStep, globalTime, next);

      return false;
   }

   private Anim a;
   private void Tick(float t, float gt, List<CustomEdge> next)
   {
      for (int i = animators.Count - 1; i >= 0; i--)
      {
         a = animators[i];
         if (!a.b.main)
         {
            animators.RemoveAt(i);
            continue;
         }
         a.b.main.SetActive(true);
         a.LocalTime += t;
         pos = a.b.startPos;
         dir = a.b.startDir;
         index = a.Id;
         localTime = a.LocalTime;
         gTime = gt;
         output = a.b;
         
         if (next.Any()) a.s.iterationId++;
         next.ForEach(e => e.inputNode.Eval(a.s));
      }
      // foreach (var a in animators)
      // {
         // if (!a.b.main)
         // {
         //    remove.Add(a);
         //    continue;
         // }
         // pos = a.b.startPos;
         // dir = a.b.startDir;
         // index = a.id;
         // time = gt;
         // output = a.b;
            
         //next = FindOutputPort("output").Edges;
         //if (next.Any()) a.s.iterationId++;
         //next.ForEach(e => e.inputNode.Eval(a.s));
      // }
      //
      // foreach (var r in remove)
      // {
      //    animators.Remove(r);
      // }
      // remove.Clear();
   
   }
   
   async Awaitable TestAnimate()
   {
      float t = 0;
      float globalT = 0;

      while (globalT < maxAnimationLength && animators.Count > 0)
      {
         t += testTickRate/1000f; //Time.dt
         globalT += testTickRate/1000f;
         List<CustomEdge> next = FindOutputPort("output").Edges;
         try
         {
            Tick(t, globalT, next);
         }
         catch (Exception e)
         {
            Debug.Log(e);
         }

         //Debug.Log($"Animator tick {globalT} {animators.Count}");
         await Task.Delay(testTickRate);
      }
      
      Debug.Log($"finished animation");
   }
}