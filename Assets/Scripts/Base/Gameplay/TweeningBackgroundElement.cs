using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class TweeningBackgroundElement : MonoBehaviour
{
    [SerializeField]
    private Transform tweeningChild;

    [SerializeField]
    private bool fromLeft;

    void Start()
    {
        TweenIn();
    }

    public void TweenOut()
    {
        Tween.LocalPosition(tweeningChild, GetOffscreenPos(), 1f, 0f, Tween.EaseIn);
    }

    private void TweenIn()
    {
        tweeningChild.transform.localPosition = GetOffscreenPos();
        Tween.LocalPosition(tweeningChild, Vector2.zero, 1f, 0f, Tween.EaseOut);
    }

    private Vector2 GetOffscreenPos()
    {
        return new Vector2(fromLeft ? -10f : 10f, 0f);
    }
}
