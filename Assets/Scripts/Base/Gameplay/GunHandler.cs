using UnityEngine;
using System.Collections;
using System;

public class GunHandler : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float FireSpeed;
    public float ReloadTime;
    public float MaxAngle;
    public float RotateSpeed;
    public Transform FirePoint;
    public Transform Center;

    private float mCurrentAngle;
    private PlayerControlInputHandler mInputHandler;
    private float mCurrentReload;

    void Start()
    {
        mInputHandler = GetComponent<PlayerControlInputHandler>();
        Move(Vector2.zero);
    }

    void Update()
    {
        mCurrentReload -= Time.deltaTime;

        if (mInputHandler.Axis != Vector2.zero)
        {
            Move(mInputHandler.Axis);
        }

        if (mInputHandler.Interact && mCurrentReload <= 0f)
        {
            Fire();
        }

        mInputHandler.ConsumeInput();

    }

    private void Move(Vector2 axis)
    {
        mCurrentAngle = Mathf.Clamp(mCurrentAngle + axis.x * RotateSpeed * Time.deltaTime, -MaxAngle, MaxAngle);
        Center.transform.localRotation = Quaternion.Euler(0f, 0f, mCurrentAngle);
    }

    public void TriggerFire()
    {
        Fire();
    }

    private void Fire()
    {
        mCurrentReload = ReloadTime;
        GameObject fireObj = GameObject.Instantiate(BulletPrefab, FirePoint.transform.position, BulletPrefab.transform.rotation);
        Vector3 direction = FirePoint.transform.position - Center.transform.position;
        fireObj.GetComponent<Rigidbody2D>().velocity = direction.normalized * FireSpeed;
        fireObj.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.value * 360f);
    }
}
