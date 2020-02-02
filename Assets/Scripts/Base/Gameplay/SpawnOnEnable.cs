using UnityEngine;
using System.Collections;

public class SpawnOnEnable : MonoBehaviour
{
    public GameObject ObjToSpawn;

    private void OnEnable()
    {
        if (ObjToSpawn != null)
            GameObject.Instantiate(ObjToSpawn, transform.position, ObjToSpawn.transform.rotation);
    }
}
