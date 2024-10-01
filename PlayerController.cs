using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    // movement
    [Header("Movement")]
    private float horizontal;
    private bool isFacingRight = true;
    private bool doubleJump;
    public float walkSpeed = 7f;
    public float runSpeed = 10f;
    public float jumpingPower = 16f;

    // dashing
    [Header("dashing")]
    private bool canDash = true;
    private bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    // sliding
    [Header("sliding")]
    private bool isWallSliding;
    public float wallSlidingSpeed = 2f;

    // wall jumping
    [Header("wall jumping")]
    private bool isWallJumping;
    private float wallJumpingDirection;
    public float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(8f, 16f);


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private TrailRenderer tr;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
            return;

        //move control
        horizontal = Input.GetAxisRaw("Horizontal");

        //jump
        if (Input.GetButtonDown("Jump") && IsGrounded())
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

        //double jump
        if (Input.GetButtonDown("Jump") && !IsGrounded() && doubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            doubleJump = false;
        }
        

        //jump power
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

        //can double jump
        if (IsGrounded())
            doubleJump = true;

        if (Input.GetButtonDown("Dash") && canDash)
            StartCoroutine(Dash());

        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            Flip();
        }
        

    }
    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            if (isDashing)
                return;

            if (Input.GetButton("Run") && IsGrounded())
                rb.velocity = new Vector2(horizontal * runSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(horizontal * walkSpeed, rb.velocity.y);
        }
    }
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
