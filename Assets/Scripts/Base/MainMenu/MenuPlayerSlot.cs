using UnityEngine;
using System.Collections;
using Rewired;
using UnityEngine.SceneManagement;
using System;

public class MenuPlayerSlot : MonoBehaviour
{
    public GameObject OnObject;
    public GameObject OffObject;
    public Transform PlayerSpriteAnchor;

    public int m_PlayerNum;

    private Player player; // The Rewired Player
    private bool submitting;
    private bool cancelling;
    private bool starting;
 

    void Start()
    {
        player = ReInput.players.GetPlayer(m_PlayerNum);
        PlayerManager.SetActivePlayers = true;
        GameObject spawned = GameObject.Instantiate(MotivationManager.Instance.MotivationScores[m_PlayerNum].Type.MenuPrefab, PlayerSpriteAnchor);
        spawned.transform.localPosition = Vector3.zero;

        UpdateAppearance();
    }

    private void Update()
    {
        GetInput();
        ProcessInput();

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        OnObject.SetActive(PlayerManager.ActivePlayers[m_PlayerNum]);
        OffObject.SetActive(!PlayerManager.ActivePlayers[m_PlayerNum]);
    }

    private void GetInput()
    {
        submitting = player.GetButtonDown("Submit");
        starting = player.GetButtonDown("Start");
        cancelling = player.GetButtonDown("Cancel");
    }

    private void ProcessInput()
    {
        if (submitting)
            PlayerManager.ActivePlayers[m_PlayerNum] = true;

        if (cancelling)
            PlayerManager.ActivePlayers[m_PlayerNum] = false;

        if (starting && PlayerManager.ActivePlayers[m_PlayerNum] && PlayerManager.HasEnoughPlayers())
            SceneManager.LoadScene(1);
    }
}
