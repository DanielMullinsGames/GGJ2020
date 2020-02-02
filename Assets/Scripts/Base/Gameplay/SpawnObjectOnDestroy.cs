using UnityEngine;
using System.Collections;

public class SpawnObjectOnDestroy : MonoBehaviour
{
    public GameObject ObjToSpawn;

    private void OnDestroy()
    {
        if (ObjToSpawn != null)
            GameObject.Instantiate(ObjToSpawn, transform.position, ObjToSpawn.transform.rotation);
    }
}
