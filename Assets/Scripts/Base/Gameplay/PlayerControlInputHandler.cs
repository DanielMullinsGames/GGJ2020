using UnityEngine;
using System.Collections;
using System;

public class PlayerControlInputHandler : MonoBehaviour
{
    public Vector2 Axis;
    public bool Interact;

    public void SendInput(Vector2 axisInput, bool interactButtonDown)
    {
        Axis += axisInput;
        Interact |= interactButtonDown;
    }

    public void ConsumeInput()
    {
        Axis = Vector2.zero;
        Interact = false;
    }

}
