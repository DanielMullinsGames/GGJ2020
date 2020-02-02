using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MotivationManager : MonoBehaviour
{
    public static MotivationManager Instance;

    public List<MotivationScore> MotivationScores;
    public static List<MotivationScore> SavedScores = new List<MotivationScore>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;    
    }

    public void ResolveChoice(EpisodeChoice choice)
    {
        foreach (var score in choice.Scores)
            AddScore(score);
    }

    public int GetScore(Motivation motivation)
    {
        foreach (var entry in MotivationScores)
            if (entry.Type == motivation)
                return (int)entry.Score;

        return 0;
    }

    private void AddScore(MotivationScore score)
    {
        foreach (var entry in MotivationScores)
            if (entry.Type == score.Type)
                entry.Score += score.Score;
    }

    internal void SaveScore()
    {
        SavedScores.Clear();
        SavedScores.AddRange(MotivationScores);
    }
}
