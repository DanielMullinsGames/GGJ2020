using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Episode : ScriptableObject
{

    public string Description;
    public List<EpisodeChoice> Choices;
    public Sprite Background;

    public EpisodeChoice GetRandomChoice()
    {
        return Choices[UnityEngine.Random.Range(0, Choices.Count)];
    }
}

[System.Serializable]
public class EpisodeChoice
{
    public string Text;
    public List<MotivationScore> Scores;
    public Episode NextScene;
    public string Outcome;
    public bool EndsGame;
}