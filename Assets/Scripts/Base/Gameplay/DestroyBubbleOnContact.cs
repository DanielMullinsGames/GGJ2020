using UnityEngine;
using System.Collections;

public class DestroyBubbleOnContact : MonoBehaviour
{

    // Use this for initialization
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<EpisodeChoiceBubble>() != null)
            Destroy(collision.collider.gameObject);
    }
}
