using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 35f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing;
    private bool canDash = true;
    private bool dashResetAvailable = true;

    [Header("Movimento")]
    public float moveSpeed = 8.75f;
    public float jumpForce = 30f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 0.4f;

    [Header("Wall Jump")]
    public float wallJumpingTime = 0.2f;
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(8.75f, 30f);

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Jump Custom")]
    public float jumpCutMultiplier = 0.25f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteCounter;

    [Header("Checagem de chão/parede")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.05f);
    public Vector2 wallCheckSize = new Vector2(0.05f, 0.2f);
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float horizontal;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;

    private string _ultimoLadoDebug = "";
    private bool isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        Application.targetFrameRate = 100;
    }

    void Update()
    {
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        bool isWalled = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Reset dash se tocou no chão ou parede
        if ((isGrounded || isWalled) && !dashResetAvailable)
        {
            dashResetAvailable = true;
            Debug.Log("Dash resetado por chão ou parede.");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (isWalled && !isGrounded && !isWallJumping)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0f, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));

            if (wallCheck.position.x < transform.position.x && isFacingRight)
            {
                isFacingRight = false;
                Flip();
            }
            else if (wallCheck.position.x > transform.position.x && !isFacingRight)
            {
                isFacingRight = true;
                Flip();
            }
        }
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            if (isWallSliding || (wallJumpingCounter > 0f && !isGrounded && isWalled))
            {
                isWallJumping = true;
                rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
                wallJumpingCounter = 0f;
                jumpBufferCounter = 0f;

                if (transform.localScale.x != wallJumpingDirection)
                {
                    isFacingRight = !isFacingRight;
                    Flip();
                }

                anim.ResetTrigger("Land");
                anim.SetTrigger("Jump");
                Invoke(nameof(StopWallJumping), wallJumpingDuration);
            }
            else if (coyoteCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.ResetTrigger("Land");
                anim.SetTrigger("Jump");
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (Input.GetMouseButtonDown(1) && !isDashing && !isWallSliding && canDash && dashResetAvailable)
        {
            StartCoroutine(DoDash());
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            bool canAttack = stateInfo.IsName("Idle") || stateInfo.IsName("Run") || stateInfo.IsName("Jump") || stateInfo.IsName("Fall");

            if (canAttack)
            {
                string triggerToUse = isGrounded ? "Slash" : "Sweep";
                anim.SetTrigger(triggerToUse);
                isAttacking = true;
                StartCoroutine(ResetAttack(0.4f));
            }
        }

        if (!isWallJumping && !isWallSliding && horizontal != 0f)
        {
            if (horizontal < 0 && isFacingRight || horizontal > 0 && !isFacingRight)
            {
                isFacingRight = !isFacingRight;
                Flip();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isFacingRight = !isFacingRight;
            Flip();
            Debug.Log("Flip manual executado. Agora virado para: " + (isFacingRight ? "Direita" : "Esquerda"));
        }

        if (!wasGrounded && isGrounded)
        {
            anim.SetTrigger("Land");
        }

        wasGrounded = isGrounded;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsWallSliding", isWallSliding);

        string ladoAtual = isFacingRight ? "Direita" : "Esquerda";
        if (_ultimoLadoDebug != ladoAtual)
        {
            Debug.Log("Player está virado para: " + ladoAtual);
            _ultimoLadoDebug = ladoAtual;
        }
    }

    private IEnumerator DoDash()
    {
        isDashing = true;
        isAttacking = true;
        canDash = false;
        dashResetAvailable = false;

        anim.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        isAttacking = false;

        StartCoroutine(ResetDashCooldown());
    }

    private IEnumerator ResetDashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (!isWallJumping && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private IEnumerator ResetAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        Debug.Log("Ataque destravado automaticamente.");
    }

    public void ResetDashByEnemyHit()
    {
        dashResetAvailable = true;
        Debug.Log("Dash resetado por acerto em inimigo.");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}