using UnityEngine;
using System.Collections;

public class EpisodeChoiceSpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector3 m_VelocityToSet;

    public void SpawnChoice(EpisodeChoice choice)
    {
        GameObject bubble = GameObject.Instantiate(GameConfig.Instance.EpisodeBubblePrefab, transform.position, GameConfig.Instance.EpisodeBubblePrefab.transform.rotation);
        bubble.GetComponent<EpisodeChoiceBubble>().SetChoice(choice);
        bubble.GetComponent<Rigidbody2D>().velocity = m_VelocityToSet;
    }
}
