using UnityEngine;
using System.Collections;

public class ShockInput : MonoBehaviour
{
    public BoxCollider2D SafeArea;
    public SpringJoint2D Spring;
    public float Cooldown;

    private bool mSpent;
    private float mCooldownLeft;

    public void TriggerShock()
    {
        if (mSpent)
            return;

        mSpent = true;

        foreach (var character in CharacterController2D.Characters)
        {
            if (character.GetComponent<Collider2D>().bounds.Intersects(SafeArea.bounds))
                continue;

            character.Stun();
        }

        mCooldownLeft = Cooldown;
        Spring.enabled = false;
    }

    public void Reset()
    {
        mSpent = false;
    }

    private void Update()
    {
        if (!GameStateManager.Instance.Playing && mCooldownLeft <= 0f)
        {
            mCooldownLeft = 0.1f;
            Spring.enabled = false;
        }
        else if (GameStateManager.Instance.Playing && mCooldownLeft > 0f)
        {
            mCooldownLeft -= Time.deltaTime;

            if (mCooldownLeft <= 0f)
            {
                Spring.enabled = true;
            }
        }
    }
}
