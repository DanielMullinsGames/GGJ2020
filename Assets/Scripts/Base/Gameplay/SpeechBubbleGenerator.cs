using UnityEngine;
using System.Collections.Generic;

public class SpeechBubbleGenerator : MonoBehaviour
{
    [SerializeField] private List<EpisodeChoiceSpawnPoint> m_PossibleSpawns;
   
    public void SpawnSpeechBubble()
    {
        if (!GameStateManager.Instance.Playing || GameStateManager.Instance.CurrentEpisode == null)
            return;

        EpisodeChoiceSpawnPoint spawnPoint = m_PossibleSpawns[UnityEngine.Random.Range(0, m_PossibleSpawns.Count)];
        EpisodeChoice choice = GameStateManager.Instance.CurrentEpisode.GetRandomChoice();
        spawnPoint.SpawnChoice(choice);
    }
}
