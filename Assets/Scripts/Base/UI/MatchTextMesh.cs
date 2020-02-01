using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchTextMesh : MonoBehaviour
{
    public string ColorString { get; set; }

    [SerializeField]
    private TMPro.TextMeshPro toMatch;

    [SerializeField]
    private Color shadowColor;

    private void Update()
    {
        string text = toMatch.text;

        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(ColorString))
        {
            text.Replace(ColorString, "<color=#" + ColorUtility.ToHtmlStringRGBA(shadowColor) + ">");

            GetComponent<TMPro.TextMeshPro>().text = text;
        }
    }
}
