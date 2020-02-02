using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBarUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshPro text;

    [SerializeField]
    private TMPro.TextMeshPro shadow;

    private int score;

    public void UpdateScore(int score)
    {
        this.score = score;
        gameObject.SetActive(true);
        GetComponent<Animator>().Play("show_score", 0, 0f);
    }

    private void UpdateTextKeyframe()
    {
        text.text = shadow.text = score.ToString();
    }
}
