using UnityEngine;
using System.Collections;

public class DestroyAfterDelay : MonoBehaviour
{
    public float Lifetime;

    private void Update()
    {
        Lifetime -= Time.deltaTime;

        if (Lifetime <= 0f)
            Destroy(gameObject);
    }
}
