using UnityEngine;
using System.Collections;

public class PlayerControlProxyArea : MonoBehaviour
{
    public PlayerControlInputHandler InputTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterControllerRewiredInput>() != null)
            collision.gameObject.GetComponent<CharacterControllerRewiredInput>().AddInputHandler(InputTarget);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterControllerRewiredInput>() != null)
            collision.gameObject.GetComponent<CharacterControllerRewiredInput>().RemoveInputHandler(InputTarget);
    }
}
