using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 35f;
    public float dashDuration = 0.2f;
    private bool isDashing;
    private bool canDash = true;

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
    }

    void Update()
    {
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        // Checa estados com OverlapBox
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        bool isWalled = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        // Coyote Time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Reset dash quando encostar no chão
        if (isGrounded || isWalled)
        {
            canDash = true;
        }

        // Jump Buffer input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Wall Slide
        if (isWalled && !isGrounded && !isWallJumping)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0f, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));

            // Flip correto baseado na parede
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

        // Wall Jump setup
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

        // Pulo (Wall jump ou chão com buffer + coyote time)
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

        // Corta o pulo se soltar o botão enquanto ainda estiver subindo
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // Dash (permitido no ar ou em wall jump, apenas se puder)
        if (Input.GetMouseButtonDown(1) && !isDashing && !isWallSliding && canDash)
        {
            StartCoroutine(DoDash());
            return;
        }

        // Ataque (Slash no chão, Sweep no ar, Slice no ar com S pressionado)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            bool canAttack = stateInfo.IsName("Idle") || stateInfo.IsName("Run") || stateInfo.IsName("Jump") || stateInfo.IsName("Fall");

            if (canAttack)
            {
                string triggerToUse = "";

                if (isGrounded)
                {
                    triggerToUse = "Slash";
                }
                else
                {
                    triggerToUse = "Sweep"; // Ataque aéreo padrão
                }

                anim.SetTrigger(triggerToUse);
                isAttacking = true;
                StartCoroutine(ResetAttack(0.4f)); // Mesma duração
            }
        }


        // Flip durante movimentação normal
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

        // Aterrissagem
        if (!wasGrounded && isGrounded)
        {
            anim.SetTrigger("Land");
        }

        wasGrounded = isGrounded;

        // Animações base
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsWallSliding", isWallSliding);

        // DEBUG: Mostrar lado que o player está virado
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
        anim.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        isAttacking = false;
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