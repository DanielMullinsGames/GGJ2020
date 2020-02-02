using UnityEngine;
using System.Collections;

public class SpawnOnDisable : MonoBehaviour
{
    public GameObject ObjToSpawn;

    private void OnDisable()
    {
        if (ObjToSpawn != null)
            GameObject.Instantiate(ObjToSpawn, transform.position, ObjToSpawn.transform.rotation);
    }
}
