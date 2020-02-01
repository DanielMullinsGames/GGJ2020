using UnityEngine;
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
}
