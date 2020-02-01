using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EpisodeManager : MonoBehaviour
{
    public static EpisodeManager Instance;

    public TextMeshPro EpisodeTextArea;

    private List<GameObject> mChoices = new List<GameObject>();
    private GameObject mCurrentBackground;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void DisplayEnterEpisode(Episode episode)
    {
        EpisodeTextArea.text = episode.GetEpisodeText();
    }

    public void DisplayResolveEpisode(Episode episode, EpisodeChoice choice)
    {
        EpisodeTextArea.text = choice.Outcome;
    }

    public void DisplayPlayingEpisode(Episode episode)
    {
        EpisodeTextArea.text = "";
    }

    public IEnumerator TransitionScreen(Episode next)
    {
        if (next.BackgroundPrefab != null && (mCurrentBackground == null || mCurrentBackground.name != next.BackgroundPrefab.name))
        {
            if (mCurrentBackground != null)
            {
                yield return new WaitForSeconds(1f);
                Destroy(mCurrentBackground);
            }

            mCurrentBackground = GameObject.Instantiate(next.BackgroundPrefab, Vector3.zero, Quaternion.identity);
            mCurrentBackground.name = next.BackgroundPrefab.name;
        }
    }
}
