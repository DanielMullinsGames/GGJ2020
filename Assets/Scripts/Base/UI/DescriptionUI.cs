using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField]
    private SequentialText sequentialText;

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

        CustomCoroutine.WaitOnConditionThenExecute(() => !sequentialText.PlayingMessage, () =>
        {
            sequentialText.PlayMessage(message);
        });
    }

    private void OnCharacterShown()
    {
        Tween.LocalPosition(sequentialText.transform, Vector2.up * 0.01f, 0.025f, 0f, Tween.EaseIn);
        Tween.LocalPosition(sequentialText.transform, Vector2.zero, 0.075f, 0.025f, Tween.EaseOut);
    }
}
