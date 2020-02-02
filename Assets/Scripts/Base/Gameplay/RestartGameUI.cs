using UnityEngine;
using System.Collections.Generic;
using Rewired;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class RestartGameUI : MonoBehaviour
{
    public GameObject CountingDownUI;
    public float TimeToRestart;
    
    private float PercentageDone { get { return mCurrentTime / TimeToRestart; } }
    private float mCurrentTime;

    private List<Player> mRewiredPlayers = new List<Player>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < PlayerConfig.Instance.PlayableMotivations.Count; i++)
            if (PlayerManager.ActivePlayers[i] || !PlayerManager.SetActivePlayers)
                mRewiredPlayers.Add(ReInput.players.GetPlayer(i));
    }

    // Update is called once per frame
    void Update()
    {
        bool restarting = false;

        foreach (var player in mRewiredPlayers)
            if (player.GetButton("Start"))
                restarting = true;

        if (restarting)
        {
            mCurrentTime += Time.deltaTime;

            if (mCurrentTime > TimeToRestart)
                SceneManager.LoadScene(0);
        }
        else if (mCurrentTime > 0f)
        {
            mCurrentTime = Mathf.Max(mCurrentTime - Time.deltaTime, 0f);
        }

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        CountingDownUI.SetActive(mCurrentTime > 0);
    }
}
