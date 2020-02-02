using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField]
    private List<ScoreBarUI> scoreBars;

    [Header("Testing")]
    [SerializeField]
    private List<Motivation> testMotivations = new List<Motivation>();

    public void ShowScoreForMotivation(Motivation motivation, int score)
    {
        var bar = scoreBars.Find(x => x.gameObject.name.ToLower().Contains(motivation.Name.ToLower()));

        bar.UpdateScore(score);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S))
        {
            foreach (Motivation motivation in testMotivations)
            {
                ShowScoreForMotivation(motivation, 1);
            }
        }
    }
#endif
}
