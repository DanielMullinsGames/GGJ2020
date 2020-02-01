using UnityEngine;
using System.Collections;
using System;

public class GremlinSpawner : MonoBehaviour
{
    public GameObject PrefabToSpawn;
    public float MinTime;
    public float MaxTime;

    private float mTimeLeft;

    private void Awake()
    {
        mTimeLeft = UnityEngine.Random.Range(MinTime, MaxTime);
    }

    void Update()
    {
        if (GameStateManager.Instance.Playing)
        {
            mTimeLeft -= Time.deltaTime;

            if (mTimeLeft <= 0)
            {
                AttemptSpawnGremlin();
                mTimeLeft = UnityEngine.Random.Range(MinTime, MaxTime);
            }
        }
    }

    private void AttemptSpawnGremlin()
    {
        if (EpisodeChoiceBubble.CurrentBubbles.Count == 0)
            return;

        if (GameStateManager.Instance.CurrentEpisode == null)
            return;

        EpisodeChoice darkChoice = GameStateManager.Instance.CurrentEpisode.GetRandomDarkChoice();

        if (darkChoice == null)
            return;

        GameObject spawned = Instantiate(PrefabToSpawn, transform.position, PrefabToSpawn.transform.rotation);
        spawned.GetComponent<Gremlin>().SetDarkChoice(darkChoice);
    }
}
