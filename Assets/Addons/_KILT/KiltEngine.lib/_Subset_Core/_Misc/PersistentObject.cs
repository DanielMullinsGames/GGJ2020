using UnityEngine;
using System.Collections;

namespace Kilt
{
    public class PersistentObject : MonoBehaviour
    {
        protected virtual void Awake()
        {
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }
    }
}
