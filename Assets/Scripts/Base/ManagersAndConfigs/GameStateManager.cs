using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public bool Entering;
    public bool Playing;
    public bool Resolving;
    public Episode CurrentEpisode;
    public GameObject GameOverScreen;
    public GameObject LoseScreen;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void StartGame()
    {
        StartCoroutine(EnterEpisode(GameConfig.Instance.StartingEpisode));
        PlayerManager.Instance.SpawnPlayers();
    }

    public void SelectChoice(EpisodeChoice choice)
    {
        if (!Playing)
            return;

        Playing = false;
        StartCoroutine(ResolveEpisode(CurrentEpisode, choice));
    }

    private IEnumerator EnterEpisode(Episode episode)
    {
        Entering = true;
        CurrentEpisode = episode;

        yield return EpisodeManager.Instance.StartCoroutine(EpisodeManager.Instance.TransitionScreen(episode));
        EpisodeManager.Instance.DisplayEnterEpisode(episode);
        yield return new WaitForSeconds(2f);

        Entering = false;
        StartCoroutine(PlayEpisode(episode));
    }

    private IEnumerator PlayEpisode(Episode episode)
    {
        Playing = true;

        EpisodeManager.Instance.DisplayPlayingEpisode(episode);

        while (Playing)
            yield return null;
    }

    private IEnumerator ResolveEpisode(Episode episode, EpisodeChoice choice)
    {
        Resolving = true;

        EpisodeManager.Instance.DisplayResolveEpisode(episode, choice);
        MotivationManager.Instance.ResolveChoice(choice);
        HealthManager.Instance.ChangeHealth(choice.HealthChange);

        if (choice.PlotToApply != null)
            PlotManager.Instance.TriggerPlot(choice.PlotToApply);

        yield return new WaitForSeconds(2f);

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


