using UnityEngine;
using System.Collections.Generic;
using System;

public class Gremlin : MonoBehaviour
{
    public EpisodeChoice DarkChoice;
    public float Speed;

    private Transform mCurrentTarget;

    public void SetDarkChoice(EpisodeChoice darkChoice)
    {
        DarkChoice = darkChoice;
    }

    private static List<EpisodeChoiceBubble> sValidChoices = new List<EpisodeChoiceBubble>();

    private void Update()
    {
        if (mCurrentTarget == null)
        {
            foreach (var bubble in EpisodeChoiceBubble.CurrentBubbles)
                if (!bubble.IsDark)
                    sValidChoices.Add(bubble);

            if (sValidChoices.Count > 0)
                mCurrentTarget = sValidChoices[UnityEngine.Random.Range(0, sValidChoices.Count)].transform;

            sValidChoices.Clear();
        }

        if (mCurrentTarget != null)
        {
            GetComponent<Rigidbody2D>().velocity = (mCurrentTarget.position - transform.position).normalized * Speed;
        }
    }

    internal void TriggerHit()
    {
        Destroy(gameObject);
    }
}
