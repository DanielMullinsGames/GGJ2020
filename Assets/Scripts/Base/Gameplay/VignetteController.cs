using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class VignetteController : MonoBehaviour
{
    private VignetteAndChromaticAberration vignetteAndChromaticAberration;

    private void Awake()
    {
        vignetteAndChromaticAberration = GetComponent<VignetteAndChromaticAberration>();
    }

    public void SetIntensity(float value)
    {
        vignetteAndChromaticAberration.intensity = value;
    }
}
