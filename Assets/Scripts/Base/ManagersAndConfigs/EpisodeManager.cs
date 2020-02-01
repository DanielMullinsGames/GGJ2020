using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EpisodeManager : MonoBehaviour
{
    public static EpisodeManager Instance;

    public TextMeshPro EpisodeTextArea;
    public List<Transform> EpisodeChoiceLocations;

    private List<GameObject> mChoices = new List<GameObject>();

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
        EpisodeTextArea.text = episode.Description;
    }

    public void DisplayResolveEpisode(Episode episode, EpisodeChoice choice)
    {
        foreach (var existing in mChoices)
            Destroy(existing);

        mChoices.Clear();

        EpisodeTextArea.text = choice.Outcome;
    }

    public void DisplayPlayingEpisode(Episode episode)
    {
        //EpisodeTextArea.text = "";
        DisplayChoices(episode);
    }

    private void DisplayChoices(Episode episode)
    {
        for (int i = 0; i < episode.Choices.Count; i++)
        {
            GameObject choiceObj = Instantiate(GameConfig.Instance.EpisodeBubblePrefab, EpisodeChoiceLocations[i]);
            choiceObj.GetComponent<EpisodeChoiceBubble>().SetChoice(episode.Choices[i]);
            mChoices.Add(choiceObj);
        }
    }
}
