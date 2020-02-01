using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MotivationManager : MonoBehaviour
{
    public static MotivationManager Instance;

    public List<MotivationScore> MotivationScores;

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

    private void AddScore(MotivationScore score)
    {
        foreach (var entry in MotivationScores)
            if (entry.Type == score.Type)
                entry.Score += score.Score;
    }
}
