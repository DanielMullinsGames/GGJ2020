using UnityEngine;
using System.Collections.Generic;

public class SpeechBubbleGenerator : MonoBehaviour
{
    [SerializeField] private List<EpisodeChoiceSpawnPoint> m_PossibleSpawns;
    [SerializeField] private float m_ThoughtCooldown;
    [SerializeField] private float m_ThoughtDelay;
    [SerializeField] private bool m_SpawnDark;

    private float mCooldownLeft;
    private int mCurrentThoughtIndex;
   
    public void SpawnSpeechBubble()
    {
        if (!GameStateManager.Instance.Playing || GameStateManager.Instance.CurrentEpisode == null)
            return;

        foreach (var spawnPoint in m_PossibleSpawns)
        {
            EpisodeChoice choice;

            if (!m_SpawnDark)
                choice = GameStateManager.Instance.CurrentEpisode.GetOrderedChoice(mCurrentThoughtIndex, out mCurrentThoughtIndex);
            else
                choice = GameStateManager.Instance.CurrentEpisode.GetRandomDarkChoice();

            if (choice != null)
            {
                spawnPoint.SpawnChoice(choice, m_SpawnDark);
            }

        }

    }

    private void Update()
    {
        mCooldownLeft -= Time.deltaTime;

        if (GameStateManager.Instance.Playing && mCooldownLeft <= 0f)
        {
            SpawnSpeechBubble();
            mCooldownLeft = m_ThoughtCooldown;
        }
        else if (!GameStateManager.Instance.Playing)
        {
            mCooldownLeft = m_ThoughtDelay;
        }
    }
}
