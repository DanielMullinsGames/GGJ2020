using UnityEngine;
using System.Collections;
using System;

public class Bullet : MonoBehaviour
{
    public void TriggerHit()
    {
        GameObject.Destroy(gameObject);
    }

    private void Update()
    {
        if (!GameStateManager.Instance.Playing)
            Destroy(gameObject);
    }
}
