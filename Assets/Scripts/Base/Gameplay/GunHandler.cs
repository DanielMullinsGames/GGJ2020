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
    public ParticleSystem FireParticles;
    public AudioSource ButtonPressedSound;

    private float mCurrentAngle;
    private PlayerControlInputHandler mInputHandler;
    private float mCurrentReload;

    [SerializeField]
    private Animator cannonAnim;

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

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
#endif

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
        ButtonPressedSound.Play();

        mCurrentReload = ReloadTime;
        GameObject fireObj = GameObject.Instantiate(BulletPrefab, FirePoint.transform.position, BulletPrefab.transform.rotation);
        Vector3 direction = FirePoint.transform.position - Center.transform.position;
        fireObj.GetComponent<Rigidbody2D>().velocity = direction.normalized * FireSpeed;
        fireObj.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.value * 360f);

        cannonAnim.Play("shoot", 0, 0f);

        if (FireParticles != null)
        {
            var particles = Instantiate(FireParticles.gameObject);
            particles.gameObject.SetActive(true);
            particles.transform.position = FireParticles.transform.position;
            particles.transform.rotation = FireParticles.transform.rotation;
            Destroy(particles.gameObject, 1f);
        }
    }
}
