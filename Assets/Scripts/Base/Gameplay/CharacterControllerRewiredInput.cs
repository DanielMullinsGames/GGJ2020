using UnityEngine;
using System.Collections.Generic;
using Rewired;
using System;

public class CharacterControllerRewiredInput : MonoBehaviour
{
    // The Rewired player id of this character
    public int playerId = 0;

    private Player player; // The Rewired Player
    private Vector3 moveVector;
    private Vector3 interactVector;
    private CharacterController2D controller;
    private bool jumping;
    private bool interacting;
    private bool shoving;
    private List<PlayerControlInputHandler> InputHandlers = new List<PlayerControlInputHandler>(); 

    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
        player = ReInput.players.GetPlayer(playerId);
    }

    private void Update()
    {
        GetInput();
        ProcessInput();
    }

    private void GetInput()
    {
        // Get the input from the Rewired Player. All controllers that the Player owns will contribute, so it doesn't matter
        // whether the input is coming from a joystick, the keyboard, mouse, or a custom controller.

        moveVector.x = player.GetAxis("Horizontal"); // get input by name or action id
        moveVector.y = player.GetAxis("Vertical");
        interactVector.x = player.GetAxis("Interact_Horizontal"); // get input by name or action id
        interactVector.y = player.GetAxis("Interact_Vertical");
        jumping = player.GetButtonDown("Submit");
        interacting = player.GetButtonDown("Interact");
        shoving = player.GetButtonDown("Cancel");
    }

    private void ProcessInput()
    {
        // Process movement
        controller.Move(moveVector.x, jumping, shoving || interacting);

        foreach (var inputHandler in InputHandlers)
            inputHandler.SendInput(interactVector, interacting);
    }

    public void AddInputHandler(PlayerControlInputHandler inputTarget)
    {
        InputHandlers.Add(inputTarget);
    }

    public void RemoveInputHandler(PlayerControlInputHandler inputTarget)
    {
        InputHandlers.Remove(inputTarget);
    }
}
