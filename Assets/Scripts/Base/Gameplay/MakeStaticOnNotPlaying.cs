using UnityEngine;
using System.Collections;

public class MakeStaticOnNotPlaying : MonoBehaviour
{

    
    void Update()
    {
        if (!GameStateManager.Instance.Playing)
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        else
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }
}
