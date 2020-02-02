﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Pixelplacement;
using System;

public class EpisodeManager : MonoBehaviour
{
    public static EpisodeManager Instance;

    public DescriptionUI DescriptionUI;

    private List<GameObject> mChoices = new List<GameObject>();
    private GameObject mCurrentBackground;

    private const float TRANSITION_DURATION = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void DisplayEnterEpisode(Episode episode)
    {
        DescriptionUI.PlayMessage(episode.GetEpisodeText());
    }

    public void DisplayResolveEpisode(Episode episode, EpisodeChoice choice)
    {
        DescriptionUI.PlayMessage(choice.Outcome);
    }

    public void DisplayPlayingEpisode(Episode episode)
    {
        DescriptionUI.PlayMessage("");
    }

    public IEnumerator TransitionScreen(Episode next)
    {
        if (next.BackgroundPrefab != null && (mCurrentBackground == null || mCurrentBackground.name != next.BackgroundPrefab.name))
        {
            bool firstTransition = mCurrentBackground == null;

            if (mCurrentBackground != null)
            {
                Tween.Position(mCurrentBackground.transform, new Vector2(-10f, 0f), TRANSITION_DURATION, 0f, Tween.EaseInOut);
            }

            mCurrentBackground = GameObject.Instantiate(next.BackgroundPrefab, Vector3.zero, Quaternion.identity);
            mCurrentBackground.name = next.BackgroundPrefab.name;

            mCurrentBackground.transform.position = new Vector2(10f, 0f);
            Tween.Position(mCurrentBackground.transform, Vector2.zero, firstTransition ? 0f : TRANSITION_DURATION, 0f, Tween.EaseInOut);

            yield return new WaitForSeconds(TRANSITION_DURATION);
        }
    }

    public void DisplayFailedToChoose(EpisodeChoice timeOutChoice)
    {
        DescriptionUI.PlayMessage(timeOutChoice.Text);
    }
}
