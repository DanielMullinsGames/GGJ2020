using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockChargeBar : MonoBehaviour
{
    [SerializeField]
    private Transform bar;

    [SerializeField]
    private AnimatingSprite iconAnim;

    [SerializeField]
    private Rigidbody2D buttonRigidbody;

    [SerializeField]
    private SpriteRenderer buttonRenderer;

    [SerializeField]
    private Sprite buttonDynamicSprite;

    [SerializeField]
    private Sprite buttonStaticSprite;

    [SerializeField]
    private float chargeDuration = 10f;

    public AudioSource ShockSound;

    private float timer;
    private float mMaxBarScale;

    private bool mSpent;

    private void Awake()
    {
        mMaxBarScale = bar.localScale.x;
    }

    public void OnShockButtonPressed()
    {
        if (mSpent)
            return;

        ShockSound.Play();
        Shock();
        timer = 0f;
    }

    public void Reset()
    {
        mSpent = false;
    }

    private void Shock()
    {
        mSpent = true;
    }

    private void Update()
    {
        if (GameStateManager.Instance.Playing)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
        }

        bar.localScale = new Vector2(Mathf.Min(mMaxBarScale, (timer / chargeDuration) * mMaxBarScale), bar.localScale.y);

        if (timer > chargeDuration)
        {
            iconAnim.enabled = true;
            buttonRigidbody.bodyType = RigidbodyType2D.Dynamic;
            buttonRenderer.sprite = buttonDynamicSprite;
        }
        else
        {
            iconAnim.StopImmediate();
            buttonRigidbody.bodyType = RigidbodyType2D.Static;
            buttonRenderer.sprite = buttonStaticSprite;
        }
    }
}
