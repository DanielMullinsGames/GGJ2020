﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public void SetRunning(bool running)
    {
        animator.SetBool("running", running);
    }

    public void Jump()
    {
        Tween.LocalScale(transform, new Vector2(1f, 0.7f), 0.05f, 0f, Tween.EaseOut);
        Tween.LocalScale(transform, Vector2.one, 0.1f, 0.05f, Tween.EaseSpring);

        animator.SetTrigger("jump");
    }

    public void Land()
    {
        animator.SetTrigger("land");
        Tween.LocalScale(transform, new Vector2(1f, 0.7f), 0.05f, 0f, Tween.EaseOut);
        Tween.LocalScale(transform, Vector2.one, 0.3f, 0.05f, Tween.EaseSpring);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Jump();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Land();
            }
        }
    }
#endif
}
