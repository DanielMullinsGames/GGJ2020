using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePlatform : MonoBehaviour
{
    public int platformIndex;

    [SerializeField]
    private TMPro.TextMeshPro winnerText;

    [SerializeField]
    private TMPro.TextMeshPro winnerShadow;

    [SerializeField]
    private TMPro.TextMeshPro scoreText;

    [SerializeField]
    private TMPro.TextMeshPro scoreShadow;

    [SerializeField]
    private List<GameObject> motiveAnims = new List<GameObject>();

    [SerializeField]
    private bool winnerPlatform;

    void Start()
    {
        var scores = new List<MotivationScore>(MotivationManager.SavedScores);

        if (scores.Count < platformIndex + 1)
        {
            transform.position = Vector3.one * 10000f;
            return;
        }

        scores.Sort((MotivationScore a, MotivationScore b) => 
        {
            if (a.Score > b.Score)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        ShowScoreText((int)scores[platformIndex].Score);
        ShowMotivation(scores[platformIndex].Type);

        if (platformIndex == 0)
        {
            ShowWinnerText(scores[platformIndex].Type);
        }
    }

    private void ShowMotivation(Motivation motivation)
    {
        motiveAnims.Find(x => x.name.ToLower().Contains(motivation.Name.ToLower())).SetActive(true);
    }

    private void ShowScoreText(int score)
    {
        scoreText.text = scoreShadow.text = score.ToString();
    }

    private void ShowWinnerText(Motivation motivation)
    {
        winnerText.text = winnerShadow.text = ("In the end, it was " + motivation.Name + " that truly drove him.").ToUpper();
    }
}
