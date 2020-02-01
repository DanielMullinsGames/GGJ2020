using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class Fragment : MonoBehaviour
    {

        protected virtual void Awake()
        {
            FragmentSpawner.RegisterFragment(this);
        }

        // Update is called once per frame
        protected virtual void OnDestroy()
        {
            FragmentSpawner.UnregisterFragment(this);
        }
    }
}
