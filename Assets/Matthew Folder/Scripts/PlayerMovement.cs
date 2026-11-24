using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public struct Cooldown
{
    [SerializeField] private float cooldown;
    private float lastTriggerTime;

    // public constructor
    public Cooldown(float lastTriggerTime)
    {
        cooldown = 0;
        this.lastTriggerTime = lastTriggerTime;
    }

    public void Trigger()
    {
        lastTriggerTime = Time.fixedTime;
    }

    public void Drain()
    {
        lastTriggerTime = Time.fixedTime - cooldown;
    }

    public bool IsReady()
    {
        return Time.fixedTime - lastTriggerTime > cooldown;
    }

    public float Remaining()
    {
        return Mathf.Max((cooldown + lastTriggerTime - Time.fixedTime) / cooldown, 0);
    }
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float baseJumpForce = 10;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 5f;
    [SerializeField] private float speedOfJumpIncrease = 1;
    [SerializeField] private bool isPrincess;

    [Header("Control QOL")]
    [Tooltip("In seconds. Time after leaving the jump window you are still able to jump.")][SerializeField] private Cooldown jumpCoyoteTime = new(-100);
    [Tooltip("In seconds. Time after leaving the jump window you are still able to jump.")][SerializeField] private Cooldown wallCoyoteTime = new(-100);
    [Tooltip("In seconds. Time before enter the jump window you are still able to jump.")][SerializeField] private Cooldown bufferTime = new(-100);
    [SerializeField] private float runFriction = 0f;
    [SerializeField] private float resistFriction = 2f;
    [SerializeField] private LayerMask groundLayer;
    private float XLinearDampening = 0f;

    [Header("Stat Buffs")]
    [SerializeField] private float airAccelerationMultiplier = 0.8f;
    [SerializeField] private float jumpWalkSlowness = .5f;
    [SerializeField] private float speedMultiplier = 1f;

    [Header("other stats idk")]
    private bool isGrounded = true;
    private bool isOnWall = false;
    private bool isResisting = false;
    private bool isCrouching = false;
    private bool jumpCTXheld = false;
    private Vector2 xForce;
    private float storedVelocity;

    [Header("Components")]
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    public Collider2D playerCollider;
    private Animator animator;
    public Slider jumpSlider;

    [Header("Animator Hashing")]
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int xSpeedHash = Animator.StringToHash("xSpeed");
    private static readonly int xResistingHash = Animator.StringToHash("xResisting");
    private static readonly int jumpHash = Animator.StringToHash("jump");
    private static readonly int yVelHash = Animator.StringToHash("yVel");
    private static readonly int isCrouchingHash = Animator.StringToHash("isCrouching");

    // after everything is done loading
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rb.linearVelocityX);

        jumpForce = Mathf.Clamp(jumpForce, minJumpForce, maxJumpForce);
        if (jumpCTXheld == true)
        {
            jumpForce += Time.deltaTime * speedOfJumpIncrease;
        }

        if (jumpSlider != null)
        {
            float sliderVal = Mathf.Lerp(0, maxJumpForce - minJumpForce, (jumpForce - minJumpForce) / (maxJumpForce - minJumpForce));
            jumpSlider.value = sliderVal;
        }
    }

    private void LateUpdate()
    {
        // if it's already resisting...
        if (isResisting)
        {
            // ...continue resisting until acceleration stops
            isResisting = Mathf.Abs(rb.linearVelocityX - xForce.x) > Mathf.Abs(rb.linearVelocityX);
            XLinearDampening = resistFriction;
        }
        else
        {
            // ...else consider whether the player was already in motion
            isResisting = Mathf.Abs(rb.linearVelocityX - xForce.x) > Mathf.Abs(rb.linearVelocityX) && Mathf.Abs(rb.linearVelocityX) > maxSpeed/2f;

            // friction
            if (xForce.x == 0)
            {
                XLinearDampening = resistFriction;
            }
            else
            {
                XLinearDampening = runFriction;
            }
        }

        rb.linearVelocityX *= (1 - XLinearDampening * Time.deltaTime); // dampen x velocity but maintain y velocity

        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(xSpeedHash, Mathf.Abs(rb.linearVelocityX));
        animator.SetBool(xResistingHash, isResisting);
        animator.SetFloat(yVelHash, rb.linearVelocityY);
        animator.SetBool(isCrouchingHash, isCrouching);
    }

    private void FixedUpdate()
    {
        isGrounded = GroundCheck();
        isOnWall = WallCheck();

        // --------------------------------------------- //
        // give player coyote time for jumping
        if (isGrounded)
        {
            jumpCoyoteTime.Trigger();
        }
        if (isOnWall)
        {
            wallCoyoteTime.Trigger();
        }

        // acceleration modifiers
        float appliedAcceleration = acceleration * speedMultiplier * Time.fixedDeltaTime * 100;
        if (!isGrounded)
        {
            appliedAcceleration *= airAccelerationMultiplier;
        }

        rb.AddForce(appliedAcceleration * xForce); // movement acceleration
        rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, -maxSpeed, maxSpeed) * (jumpCTXheld && isGrounded && jumpForce > 11 ? jumpWalkSlowness : 1); // clamp speed  

        if (jumpSlider != null)
        {
            jumpSlider.minValue = 0;
            jumpSlider.maxValue = maxJumpForce - minJumpForce;
            if (jumpForce > 11)
            {
                jumpSlider.gameObject.SetActive(true);
            }
        }

        // --------------------------------------------- //
        // if unable to jump or not on the wall, store velocities
        if (!isOnWall && wallCoyoteTime.IsReady())
        {
            storedVelocity = rb.linearVelocityX;
        }

        // if within the buffer window
        if (!bufferTime.IsReady())
        {
            // only when touching ground or wall
            if (!wallCoyoteTime.IsReady() || !jumpCoyoteTime.IsReady())
            {
                rb.linearVelocityY = jumpForce;
                jumpForce = baseJumpForce;
                jumpSlider.gameObject.SetActive(false);

                //Debug.Log("coyote time remaining: " + wallCoyoteTime.Remaining() + " | " + "on the ground: " + isGrounded + " | " + "buffer remaining: " + bufferTime.Remaining());

                // boost off wall
                if (!wallCoyoteTime.IsReady() && !isGrounded)
                {
                    //Debug.Log("boost");

                    float minBoost = maxSpeed / 2f;
                    float boost = -storedVelocity - Mathf.Clamp(-storedVelocity, -minBoost, minBoost) + minBoost * Mathf.Sign(-storedVelocity);
                    rb.linearVelocityX = boost;

                    wallCoyoteTime.Drain();
                }

                jumpCoyoteTime.Drain();
                bufferTime.Drain();

                animator.SetTrigger(jumpHash);
            }
        }
    }

    private bool GroundCheck()
    {
        // cast thin box at the player's feet and verify the hit normal points upwards
        Vector2 origin = new(playerCollider.bounds.center.x, playerCollider.bounds.min.y);
        Vector2 boxSize = new(playerCollider.bounds.size.x * 0.9f, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, 0, groundLayer);
        return hit.collider != null && hit.normal.y > 0.65f;
    }

    private bool WallCheck()
    {
        // cast box slightly from player's feet
        Vector2 origin = (Vector2)transform.position + playerCollider.offset + playerCollider.bounds.size.x / 2f * Vector2.left;
        Vector2 boxSize = new(0.05f, 0.9f * playerCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.right, playerCollider.bounds.size.x, groundLayer);
        return hit.collider != null && Mathf.Abs(hit.normal.x) > 0.65f;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // short circuit
        //if (!ctx.performed) return;

        float xInput = ctx.ReadValue<Vector2>().x;

        xForce = new Vector2(xInput, 0);

        if (xForce.x > 0)
        {
            sr.flipX = false;
        }
        else if (xForce.x < 0)
        {
            sr.flipX = true;
        }
        if (isPrincess == false)
        {

            if (xForce.x < 0)
            {
                sr.flipX = false;
            }
            else if (xForce.x > 0)
            {
                sr.flipX = true;
            }
        }

        // crouching (below a certain threshold so controllers crouch on slight imperfections)
        if (Vector2.Dot(Vector2.up, ctx.ReadValue<Vector2>().normalized) < -0.5f)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        //Debug.Log("please jump please");
        if(isPrincess == true)
        {
            bufferTime.Trigger();
        }
        if (ctx.started && isPrincess == false)
        {
            jumpCTXheld = true;

        }
        else if (ctx.canceled && isPrincess == false)
        {
            jumpCTXheld = false;
            bufferTime.Trigger();
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 origin = new(playerCollider.bounds.center.x, playerCollider.bounds.min.y);
        Vector2 boxSize = new(playerCollider.bounds.size.x * 0.9f, 0.05f);

        Gizmos.DrawWireCube(origin, boxSize);
    }
}
