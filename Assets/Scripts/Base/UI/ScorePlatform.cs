using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePlatform : MonoBehaviour
{
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
        winnerText.text = winnerShadow.text = "In the end, it was " + motivation.Name + " that truly drove him.";
    }
}
