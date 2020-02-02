using UnityEngine;
using System.Collections;
using Rewired;
using UnityEngine.SceneManagement;
using System;
using Pixelplacement;

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

    public GameObject fadeOut;

    void Start()
    {
        player = ReInput.players.GetPlayer(m_PlayerNum);
        PlayerManager.SetActivePlayers = true;
        //GameObject spawned = GameObject.Instantiate(MotivationManager.Instance.MotivationScores[m_PlayerNum].Type.MenuPrefab, PlayerSpriteAnchor);
        //spawned.transform.localPosition = new Vector2(1.5f, 0f);

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

        if (submitting)
        {
            Bounce();
        }
    }

    private void Bounce()
    {
        OnObject.transform.localPosition = Vector3.zero;
        Tween.LocalPosition(OnObject.transform, new Vector2(0f, 0.1f), 0.05f, 0f, Tween.EaseInOut);
        Tween.LocalPosition(OnObject.transform, new Vector2(0f, 0f), 0.2f, 0.1f, Tween.EaseSpring);
    }

    private void ProcessInput()
    {
        if (submitting)
            PlayerManager.ActivePlayers[m_PlayerNum] = true;

        if (cancelling)
            PlayerManager.ActivePlayers[m_PlayerNum] = false;

        if (starting && PlayerManager.ActivePlayers[m_PlayerNum] && PlayerManager.HasEnoughPlayers())
        {
            CustomCoroutine.WaitThenExecute(0.25f, () => SceneManager.LoadScene(2));
            fadeOut.SetActive(true);
        }
    }
}
