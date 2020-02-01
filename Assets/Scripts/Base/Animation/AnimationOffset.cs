using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOffset : MonoBehaviour
{
    private void Start()
    {
        var clipName = GetComponent<Animation>().clip.name;
        GetComponent<Animation>()[clipName].normalizedTime = Random.value;
    }
}
