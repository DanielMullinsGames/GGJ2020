using UnityEngine;
using System.Collections;

public class EpisodeChoiceSpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 m_VelocityToSet;

    public void SpawnChoice(EpisodeChoice choice)
    {
        GameObject bubble = GameObject.Instantiate(GameConfig.Instance.EpisodeBubblePrefab, transform.position, GameConfig.Instance.EpisodeBubblePrefab.transform.rotation);
        bubble.GetComponent<EpisodeChoiceBubble>().SetChoice(choice);
        bubble.GetComponent<EpisodeChoiceBubble>().SetTargetVelocity(m_VelocityToSet);
    }
}
