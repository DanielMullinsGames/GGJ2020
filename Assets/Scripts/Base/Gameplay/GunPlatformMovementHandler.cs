﻿using UnityEngine;
using System.Collections;
using System;

public class GunPlatformMovementHandler : MonoBehaviour
{
    public float Speed;
    public Transform CenterReference;
    public float MaxAngle;
    public Vector3 StartingLine;
    public LayerMask EdgeLineMask;
    public float StartingAngle;
    public AudioSource ButtonPressedSound;
    public AudioSource MovedSound;

    private bool mMovingRight;
    private bool mMovingLeft;
    private float mCurrentAngle;
    private PlayerControlInputHandler mInputHandler;

    void Start()
    {
        mInputHandler = GetComponent<PlayerControlInputHandler>();
        Move(Vector2.zero);
    }

    void Update()
    {
        if (mInputHandler.Axis != Vector2.zero)
        {
            Move(mInputHandler.Axis);
            mInputHandler.ConsumeInput();
        }

        if (mMovingLeft || mMovingRight)
        {
            ; float direction = 0;
            if (mMovingRight) direction += 1;
            if (mMovingLeft) direction -= 1;
            Move(new Vector2(direction, 0f));
        }

    }

    private void Move(Vector2 axis)
    {
        mCurrentAngle = Mathf.Clamp(mCurrentAngle + axis.x * Speed * Time.deltaTime, -MaxAngle, MaxAngle);
        Vector3 direction = Quaternion.Euler(0f, 0f, mCurrentAngle) * StartingLine;

        RaycastHit2D hit = Physics2D.Raycast(CenterReference.transform.position, direction, Mathf.Infinity, EdgeLineMask);

        if (hit.collider != null)
        {
            transform.position = hit.point;
            transform.rotation = Quaternion.Euler(0f, 0f, mCurrentAngle + StartingAngle);
        }
    }

    public void SetMovingRight()
    {
        ButtonPressedSound.Play();
        MovedSound.Play();
        mMovingRight = true;
    }

    public void SetMovingLeft()
    {
        ButtonPressedSound.Play();
        MovedSound.Play();
        mMovingLeft = true;
    }

    public void UnsetMovingRight()
    {
        mMovingRight = false;
    }

    public void UnsetMovingLeft()
    {
        mMovingLeft = false;
    }
}
