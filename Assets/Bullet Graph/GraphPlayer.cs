using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

public class GraphPlayer : MonoBehaviour
{
    public List<AnimateNode> currentlyPlaying;

    public void Play(AnimateNode node)
    {
        if (!currentlyPlaying.Contains(node)) currentlyPlaying.Add(node);
    }

    private void Update()
    {
        for (int i = currentlyPlaying.Count - 1; i >= 0; i--)
        {
            var c = currentlyPlaying[i];
            if (c.Animate(Time.deltaTime))
            {
                currentlyPlaying.RemoveAt(i);
            }
        }
    }
}
