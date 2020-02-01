using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Episode : ScriptableObject
{

    public string Description;
    public List<EpisodeChoice> Choices;
    public Sprite Background;
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