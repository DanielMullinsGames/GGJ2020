using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public List<Transform> PlayerSpawnLocations;
    public static bool[] ActivePlayers = new bool[4];
    public static bool SetActivePlayers = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SpawnPlayers()
    {
        for (int i = 0; i < PlayerConfig.Instance.PlayableMotivations.Count; i++)
        {
            bool active = !SetActivePlayers || ActivePlayers[i];

            if (!active)
                continue;

            Motivation motivation = PlayerConfig.Instance.PlayableMotivations[i];
            GameObject player = GameObject.Instantiate(motivation.PlayableObject, PlayerSpawnLocations[i].position, motivation.PlayableObject.transform.rotation);
            player.AddComponent<MotivationInstance>().Motivation = motivation;
            player.GetComponent<CharacterControllerRewiredInput>().playerId = i;
        }
    }

    public static bool HasEnoughPlayers()
    {
        int count = 0;

        for (int i = 0; i < ActivePlayers.Length; i++)
            if (ActivePlayers[i])
                count++;

        return count >= 2;
    }
}
