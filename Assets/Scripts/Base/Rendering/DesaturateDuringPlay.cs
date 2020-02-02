using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class DesaturateDuringPlay : MonoBehaviour
{
    [SerializeField]
    private float fadeSpeed = 1f;

    private void Update()
    {
        bool desaturate = GameStateManager.Instance.Playing;

        var curves = GetComponent<ColorCorrectionCurves>();
        curves.saturation = Mathf.Lerp(curves.saturation, desaturate ? 0f : 1f, Time.deltaTime * fadeSpeed);
    }
}
