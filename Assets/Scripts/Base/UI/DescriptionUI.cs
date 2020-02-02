using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class DescriptionUI : MonoBehaviour
{
    public bool DIsplayingMessage { get { return sequentialText.PlayingMessage || currentCoroutines > 0; } }

    [SerializeField]
    private SequentialText sequentialText;

    private int currentCoroutines;

    private void Start()
    {
        sequentialText.Clear();
        sequentialText.CharacterShown += OnCharacterShown;
    }

    public void PlayMessage(string message)
    {
        if (sequentialText.PlayingMessage)
        {
            sequentialText.SkipToEnd();
        }

        currentCoroutines++;
        CustomCoroutine.WaitOnConditionThenExecute(() => !sequentialText.PlayingMessage, () =>
        {
            currentCoroutines--;
            sequentialText.PlayMessage(message);
        });
    }

    private void OnCharacterShown()
    {
        Tween.LocalPosition(sequentialText.transform, Vector2.up * 0.01f, 0.025f, 0f, Tween.EaseIn);
        Tween.LocalPosition(sequentialText.transform, Vector2.zero, 0.075f, 0.025f, Tween.EaseOut);
    }
}
