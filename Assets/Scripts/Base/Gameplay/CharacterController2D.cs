using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class CharacterController2D : MonoBehaviour
{
    // The Rewired player id of this character
    public int playerId = 0;

    [SerializeField] private float m_Speed;
    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_ShoveCheck;
    [SerializeField] private float ShoveForce;
    [SerializeField] private float mStunDuration;
    [SerializeField] private float mShoveStunDuration;

    [SerializeField]
    private float k_GroundedRadius = .01f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private float mStunLeft;

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;

    [SerializeField]
    private CharacterAnimationController animController;

    public static List<CharacterController2D> Characters = new List<CharacterController2D>();

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }


    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        Characters.Add(this);
    }

    private void OnDestroy()
    {
        Characters.Remove(this);
    }

    private void FixedUpdate()
    {
        if (mStunLeft > 0f && mStunLeft - Time.fixedDeltaTime < 0f)
        {
            animController.SetShocked(false);
        }
        mStunLeft -= Time.fixedDeltaTime;

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                {
                    OnLandEvent.Invoke();
                    animController.Land();
                }
            }
        }
    }

    public void Move(float move, bool jump, bool shove)
    {
        if (mStunLeft > 0f)
        {
            return;
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * m_Speed, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }

            animController.SetRunning(Mathf.Abs(move) > 0.01f);
        }
        // If the player should jump...
        if (m_Grounded && jump)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            animController.Jump();
        }

        if (shove)
        {
            animController.Shove();
            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_ShoveCheck.position, k_GroundedRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject && colliders[i].gameObject.GetComponent<CharacterController2D>())
                {
                    colliders[i].gameObject.GetComponent<Rigidbody2D>().AddForce((colliders[i].gameObject.transform.position - transform.position).normalized * ShoveForce, ForceMode2D.Impulse);
                    colliders[i].gameObject.GetComponentInChildren<CharacterAnimationController>().GetShoved();
                    colliders[i].gameObject.GetComponentInChildren<CharacterController2D>().ShoveStun();
                }
            }
        }
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Stun()
    {
        mStunLeft = Mathf.Max(mStunDuration, mStunLeft);
        animController.SetShocked(true);
    }

    public void ShoveStun()
    {
        mStunLeft = Mathf.Max(mShoveStunDuration, mStunLeft);
        mStunLeft = mShoveStunDuration;
        animController.SetShocked(true);
    }
}