using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

public class Graphrunner : StateMachineBehaviour
{
    public BulletGraph g;
    public string id;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
        AnimatorControllerPlayable controller)
    {
        g.InitializeEdges();
        g.Reset();
        FireNode node = g.nodes.Find(
            n => n is FireNode f && f.id == id
        ) as FireNode;
        animator.GetComponent<MonoBehaviour>().StartCoroutine(RunNode(node, animator.gameObject));
    }
    
    private IEnumerator RunNode(FireNode n, GameObject go)
    {
        Node.TreeStateData state = new() { originator = n, attached = go, activationId = Random.Range(0, 57890190), test = false, state = new()};
        state = state.Repeat();
        var i = 0;
        float delay;
        while (state.state.Repeat && i<5000)
        {
            state = n.Eval(state.Reset());
            state.iterationId++;
            i++;
            delay = state.state.delay;
            if (delay != 0) yield return new WaitForSeconds(delay);
        }
        Debug.Log($"finished running {n}");
    }
}
