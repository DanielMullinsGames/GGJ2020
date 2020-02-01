using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct EpisodeCriteria
{
    public List<Plot> RequiredPlotPoints;
    public List<Plot> BlockingPlotPoints;

    public bool Valid()
    {
        foreach (var required in RequiredPlotPoints)
            if (!PlotManager.Instance.CheckPlot(required))
                return false;

        foreach (var blocking in BlockingPlotPoints)
            if (PlotManager.Instance.CheckPlot(blocking))
                return false;

        return true;
    }
}

[System.Serializable]
public struct EpisodeText
{
    public EpisodeCriteria Criteria;
    public string Text;
}


public class Episode : ScriptableObject
{
    public GameObject BackgroundPrefab;
    public List<EpisodeText> PotentialDescriptions;
    public List<EpisodeChoice> Choices;
    public List<EpisodeChoice> DarkChoices;

    public EpisodeChoice GetRandomChoice()
    {
        return EpisodeChoice.GetRandomChoice(Choices);
    }

    public EpisodeChoice GetRandomDarkChoice()
    {
        return EpisodeChoice.GetRandomChoice(DarkChoices);
    }

    public string GetEpisodeText()
    {
        foreach (var pot in PotentialDescriptions)
            if (pot.Criteria.Valid())
                return pot.Text;

        return string.Empty;
    }
}

[System.Serializable]
public class EpisodeChoice
{
    public EpisodeCriteria Criteria;
    public string Text;
    public List<MotivationScore> Scores;
    public Plot PlotToApply;
    public Episode NextScene;
    public string Outcome;
    public bool EndsGame;
    public int HealthChange;

    public static EpisodeChoice GetRandomChoice(List<EpisodeChoice> potential)
    {
        List<EpisodeChoice> scratchpad = new List<EpisodeChoice>();

        foreach (var pot in potential)
            if (pot.Criteria.Valid())
                scratchpad.Add(pot);

        if (scratchpad.Count == 0)
            return null;

        return scratchpad[UnityEngine.Random.Range(0, scratchpad.Count)];
    }
}