using UnityEngine;
using System.Collections;

public class Motivation : ScriptableObject
{
    public GameObject PlayableObject;
    public GameObject MenuPrefab;
    public Color Color;
    public string Name;
}

[System.Serializable]
public class MotivationScore
{
    public Motivation Type;
    public float Score;
}