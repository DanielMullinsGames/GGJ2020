using UnityEngine;
using System.Collections;

public class StartGameController : MonoBehaviour
{
    public GameObject StartAvailable;

    // Update is called once per frame
    void Update()
    {
        StartAvailable.SetActive(PlayerManager.HasEnoughPlayers());
    }
}
