using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pixelplacement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public bool Entering;
    public bool Playing;
    public bool Resolving;
    public Episode CurrentEpisode;
    public GameObject GameOverScreen;
    public GameObject LoseScreen;
    public float TimeUntilTimeOut;
    public ScoreUI ScoreUI;

    [SerializeField] private EpisodeChoice episodeToTrigger;
    [SerializeField] private bool TriggerEpisodeToTrigger;
    [SerializeField] private Plot plotToTrigger;
    [SerializeField] private bool TriggerPlotToTrigger;

    private TimeOutController toController;

    private void Awake()
    {
        Instance = this;
        toController = GetComponent<TimeOutController>();
    }

    private void Start()
    {
        StartGame();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {
        if (TriggerPlotToTrigger)
            PlotManager.Instance.TriggerPlot(plotToTrigger);

        TriggerPlotToTrigger = false;

        if (TriggerEpisodeToTrigger && Playing)
        {
            SelectChoice(episodeToTrigger);
            TriggerEpisodeToTrigger = false;
        }
    }

    public void StartGame()
    {
        StartCoroutine(EnterEpisode(GameConfig.Instance.StartingEpisode));
        PlayerManager.Instance.SpawnPlayers();
    }

    public void SelectChoice(EpisodeChoiceBubble bubble)
    {
        if (!Playing)
            return;

        toController.CancelTimeOut();
        Playing = false;
        StartCoroutine(ResolveChoiceBubble(bubble));
    }

    public void SelectChoice(EpisodeChoice choice)
    {
        if (!Playing)
            return;

        toController.CancelTimeOut();
        Playing = false;
        StartCoroutine(ResolveEpisode(CurrentEpisode, choice));
    }

    private IEnumerator EnterEpisode(Episode episode)
    {
        Entering = true;
        CurrentEpisode = episode;

        yield return EpisodeManager.Instance.StartCoroutine(EpisodeManager.Instance.TransitionScreen(episode));
        EpisodeManager.Instance.DisplayEnterEpisode(episode);

        while (EpisodeManager.Instance.ShowingMessage)
            yield return null;

        yield return new WaitForSeconds(1f);

        Entering = false;
        StartCoroutine(PlayEpisode(episode));
    }

    private IEnumerator PlayEpisode(Episode episode)
    {
        Playing = true;
        EpisodeChoice timeOutChoice = episode.GetRandomDarkChoice();
        float timeLeft = TimeUntilTimeOut;

        EpisodeManager.Instance.DisplayPlayingEpisode(episode);

        while (Playing)
        {
            /*
            if (timeOutChoice != null)
            {
                timeLeft -= Time.deltaTime;
                toController.SetCurrentTimeOut(timeOutChoice, timeLeft);

                if (timeLeft <= 0f)
                    yield return StartCoroutine(ShowFailedToMakeChoice(episode, timeOutChoice));
            }
            */

            yield return null;
        }
    }

    private IEnumerator ShowFailedToMakeChoice(Episode episode, EpisodeChoice timeOutChoice)
    {
        Playing = false;
        EpisodeManager.Instance.DisplayFailedToChoose(timeOutChoice);

        while (EpisodeManager.Instance.ShowingMessage)
            yield return null;

        yield return new WaitForSeconds(1f);
        toController.CancelTimeOut();
        StartCoroutine(ResolveEpisode(CurrentEpisode, timeOutChoice));
    }

    private IEnumerator ResolveChoiceBubble(EpisodeChoiceBubble bubble)
    {
        bubble.SetBrainy();
        yield return new WaitForSeconds(1f);

        Tween.Position(bubble.transform, new Vector2(0f, 1.3f), 0.5f, 0f, Tween.EaseInOut);
        yield return new WaitForSeconds(1f);

        MotivationManager.Instance.ResolveChoice(bubble.Choice);
        if (bubble.Choice.Scores.Count > 0)
        {
            foreach (var motivationScore in bubble.Choice.Scores)
                ScoreUI.ShowScoreForMotivation(motivationScore.Type, MotivationManager.Instance.GetScore(motivationScore.Type));

            yield return new WaitForSeconds(2.5f);
        }

        yield return new WaitForSeconds(1f);
        bubble.CleanUp();

        StartCoroutine(ResolveEpisode(CurrentEpisode, bubble.Choice));
    }

    private IEnumerator ResolveEpisode(Episode episode, EpisodeChoice choice)
    {
        Resolving = true;
        yield return new WaitForSeconds(1f);

        EpisodeManager.Instance.DisplayResolveEpisode(episode, choice);

        while (EpisodeManager.Instance.ShowingMessage)
            yield return null;

        if (choice.PlotToApply != null)
            PlotManager.Instance.TriggerPlot(choice.PlotToApply);


        while (EpisodeManager.Instance.ShowingMessage)
            yield return null;

        HealthManager.Instance.ChangeHealth(choice.HealthChange);

        yield return new WaitForSeconds(1f);

        if (HealthManager.Instance.Health <= 0)
            StartCoroutine(LoseGame());
        else if (choice.EndsGame)
            StartCoroutine(FinishGame());
        else
            StartCoroutine(EnterEpisode(choice.NextScene));
    }

    private IEnumerator FinishGame()
    {
        GameOverScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }

    private IEnumerator LoseGame()
    {
        LoseScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }
}


