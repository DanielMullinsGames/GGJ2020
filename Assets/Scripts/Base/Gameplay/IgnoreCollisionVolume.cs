using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisionVolume : MonoBehaviour
{
    [SerializeField]
    private Collider2D colliderToIgnore;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<CharacterController2D>())
        {
            Physics2D.IgnoreCollision(colliderToIgnore, other, ignore: true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<CharacterController2D>())
        {
            Physics2D.IgnoreCollision(colliderToIgnore, other, ignore: false);
        }
    }
}
