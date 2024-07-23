using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Graphrunner : StateMachineBehaviour
{
    public BulletGraph g;
    public string id;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
        AnimatorControllerPlayable controller)
    {
        g.Reset();
        var node = g.nodes.Find(
            n => n is FireNode f && f.id == id
        );
        Node.TreeStateData state = new()
            { originator = node, attached = animator.gameObject, activationId = Random.Range(0, 57890190), test = false };
        node.Eval(state);
    }
}
