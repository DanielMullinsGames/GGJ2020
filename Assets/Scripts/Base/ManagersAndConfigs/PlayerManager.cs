using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public List<Transform> PlayerSpawnLocations;

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
            Motivation motivation = PlayerConfig.Instance.PlayableMotivations[i];
            GameObject player = GameObject.Instantiate(motivation.PlayableObject, PlayerSpawnLocations[i].position, motivation.PlayableObject.transform.rotation);
            player.AddComponent<MotivationInstance>().Motivation = motivation;
            player.GetComponent<CharacterControllerRewiredInput>().playerId = i;
        }
    }
}
