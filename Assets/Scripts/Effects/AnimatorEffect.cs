using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorEffect : Effect
{
    public Animator animator;

    protected override float Lifetime
    {
        get
        {
            if(!animator)
            {
                animator = GetComponent<Animator>();
            }

            return animator.GetCurrentAnimatorStateInfo(0).length;
        }
    }
}
