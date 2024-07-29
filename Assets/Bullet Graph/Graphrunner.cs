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
        g.Reset();
        var node = g.nodes.Find(
            n => n is FireNode f && f.id == id
        );
        animator.GetComponent<MonoBehaviour>().StartCoroutine(RunNode(node, animator.gameObject));
    }
    
    private IEnumerator RunNode(Node n, GameObject go)
    {
        Node.TreeStateData state = new() { originator = n, attached = go, activationId = Random.Range(0, 57890190), test = false };
        state = state.Repeat();
        var i = 0;
        while (state.state.Repeat && i<300)
        {
            state = n.Eval(state.Reset());
            state.iterationId++;
            i++;
            yield return new WaitForSeconds(1f / 60);
        }
        Debug.Log($"finished testing {n}");
    }
}
