// PlayerController2D.cs (v1.5 – Movimento, Ataque e Dano)
// -----------------------------------------------------------------------------
// Controlador 2D completo: movimentação, pulo duplo, wall-slide, dash,
// dois ataques (Slice & Slash), sistema de vida/dano e dano em inimigos.
// -----------------------------------------------------------------------------
// Parametros esperados no Animator Controller (case-sensitive):
// • float   Speed
// • bool    IsGrounded
// • bool    IsFalling
// • bool    IsWallSliding
// • trigger Jump
// • trigger Dash
// • trigger Slice
// • trigger Slash
// • trigger Damage
// • trigger Die
// -----------------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    #region === Configurações no Inspector ===
    [Header("Movimentação")]
    [SerializeField] private float runMaxSpeed = 8f;
    [SerializeField] private float runAcceleration = 60f;
    [SerializeField] private float runDeceleration = 48f;
    [SerializeField, Range(0f, 1f)] private float accelInAir = .45f;
    [SerializeField, Range(0f, 1f)] private float decelInAir = .45f;

    [Header("Pulo")]
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

    [Header("Pulo Duplo")]
    [SerializeField] private int extraJumps = 1;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;

    [Header("Wall Slide & Jump")]
    [SerializeField] private float wallSlideSpeed = 3f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(14f, 16f);
    [SerializeField] private float wallJumpTime = 0.2f;

    [Header("Checagens (Empty Children)")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.2f, 0.05f);
    [Space(4)]
    [SerializeField] private Transform wallCheck = null;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 0.2f);
    [Space(4)]
    [SerializeField] private LayerMask groundLayer;

    // === ATAQUES ============================================================
    [Header("Ataque")]
    [SerializeField] private float sliceCooldown = 0.9f;           // Cooldown Slice
    [SerializeField] private float sliceFreezeTime = 0.6f;         // Congelamento Slice
    [SerializeField] private float slashCooldown = 0.35f;          // Cooldown Slash
    [SerializeField] private Transform attackPoint = null;         // Pivô da hitbox
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1f, 0.5f);
    [SerializeField] private LayerMask enemyLayer;

    // === VIDA / DANO ========================================================
    [Header("Vida")]
    public int maxHealth = 4;
    public int currentHealth;
    private bool isDead = false;
    private bool isTakingDamage = false;

    [Header("Reação a Dano")]
    [SerializeField] private float damagePushForceX = 5f;
    [SerializeField] private float damagePushForceY = 2f;
    #endregion

    #region === Variáveis internas ===
    private Rigidbody2D rb;
    private Animator animator;
    private bool isFacingRight = true;

    private Vector2 moveInput;
    private float lastOnGroundTime;
    private float jumpBufferCounter;

    // Estados
    private bool isGrounded;                 // usado pelo Animator
    private bool isJumping;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpCounter;
    private bool canDash = true;
    private bool isDashing;

    private int jumpsLeft;                   // pulo duplo

    // Ataques
    private float lastSliceTime;
    private float lastSlashTime;
    private bool  isSliceFrozen;
    #endregion

    #region === Inicialização ===
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        jumpsLeft = extraJumps;

        // Vida
        currentHealth = maxHealth;
    }
    #endregion

    #region === Loop principal ===
    private void Update()
    {
        if (isDead) return;          // Personagem morto - nenhuma lógica roda
        HandleTimers();

        if (!isTakingDamage)        // Durante dano, ignora entrada do jogador
            ReadInput();
        else
            moveInput.x = 0f;

        CheckCollisions();
        HandleJump();
        HandleFlip();
        UpdateAnimator();
        HandleAttacks();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        ApplyHorizontalMovement();
        ApplyWallSlide();
    }
    #endregion

    #region === Métodos privados ===
    // ---------------- Timers ----------------
    private void HandleTimers()
    {
        lastOnGroundTime -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
        if (isWallJumping) wallJumpCounter -= Time.deltaTime;
    }

    // ---------------- Entrada ----------------
    private void ReadInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f && !isWallJumping)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // Dash só é permitido se não estiver em wall-slide
        if (Input.GetMouseButtonDown(1) && canDash && !isWallSliding)
            StartCoroutine(Dash());
    }

    // ----------- Checagens de colisão -----------
    private void CheckCollisions()
    {
        bool groundedNow = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        bool onWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);

        if (groundedNow)
        {
            lastOnGroundTime = coyoteTime;
            jumpsLeft = extraJumps;
            canDash = true;
        }

        isGrounded = groundedNow;
        isWallSliding = onWall && !groundedNow && rb.linearVelocity.y < 0f && !isWallJumping;
    }

    // ---------------- Movimentação ----------------
    private void ApplyHorizontalMovement()
    {
        if (isSliceFrozen || isTakingDamage) return;  // Slice ou Dano bloqueiam movimento
        if (isWallJumping) return;

        bool onWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
        if (onWall && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float targetSpeed = moveInput.x * runMaxSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAcceleration : runDeceleration;
        if (lastOnGroundTime <= 0f)
            accelRate *= (Mathf.Abs(targetSpeed) > 0.01f) ? accelInAir : decelInAir;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        rb.AddForce(speedDiff * accelRate * Vector2.right, ForceMode2D.Force);
    }

    private void ApplyWallSlide()
    {
        if (isWallSliding)
        {
            float yVel = Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, yVel);
        }
    }

    // ---------------- Pulo & Wall Jump ----------------
    private void HandleJump()
    {
        if (isDashing || isSliceFrozen || isTakingDamage) return; // Bloqueios

        if (jumpBufferCounter > 0f)
        {
            if (lastOnGroundTime > 0f)
            {
                PerformGroundJump();
            }
            else if (isWallSliding)
            {
                PerformWallJump();
            }
            else if (jumpsLeft > 0)
            {
                PerformDoubleJump();
            }
        }

        if (isWallJumping && wallJumpCounter <= 0f)
            isWallJumping = false;
    }

    private void PerformGroundJump()
    {
        isJumping = true;
        jumpBufferCounter = 0f;
        lastOnGroundTime = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("Jump");
    }

    private void PerformWallJump()
    {
        isJumping = true;
        isWallJumping = true;
        wallJumpCounter = wallJumpTime;
        jumpBufferCounter = 0f;

        int dir = isFacingRight ? -1 : 1;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(wallJumpForce.x * dir, wallJumpForce.y), ForceMode2D.Impulse);

        if ((dir > 0 && !isFacingRight) || (dir < 0 && isFacingRight)) Flip();
        animator.SetTrigger("Jump");
    }

    private void PerformDoubleJump()
    {
        isJumping = true;
        jumpBufferCounter = 0f;
        jumpsLeft--;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("Jump");
    }

    // ---------------- Dash ----------------
    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.SetTrigger("Dash");

        float dashDir = isFacingRight ? 1f : -1f;
        float startTime = Time.time;
        float storedGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        while (Time.time - startTime < dashDuration)
            yield return null;

        rb.gravityScale = storedGravity;
        isDashing = false;
    }

    // ---------------- Animator ----------------
    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsFalling", rb.linearVelocity.y < -0.1f && !isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
    }

    // -------------- Ataques (Slice / Slash) --------------
    private void HandleAttacks()
    {
        // Restrição: não pode atacar se estiver em wall-slide ou sofrendo dano
        if (Input.GetMouseButtonDown(0) && !isWallSliding && !isTakingDamage)
        {
            if (isGrounded)
            {
                // --- SLICE --------------------------------------------------------------
                if (Time.time >= lastSliceTime + sliceCooldown)
                {
                    animator.SetTrigger("Slice");
                    lastSliceTime = Time.time;
                    PerformAttack();
                    StartCoroutine(FreezeDuringSlice());
                }
            }
            else
            {
                // --- SLASH --------------------------------------------------------------
                if (Time.time >= lastSlashTime + slashCooldown)
                {
                    animator.SetTrigger("Slash");
                    lastSlashTime = Time.time;
                    PerformAttack();
                }
            }
        }
    }

    // Área de dano do ataque
    private void PerformAttack()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackBoxSize,
            0f,
            enemyLayer);

        foreach (Collider2D hit in hits)
            hit.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
    }

    // Congela movimentação horizontal durante parte do Slice
    private System.Collections.IEnumerator FreezeDuringSlice()
    {
        isSliceFrozen = true;

        // Zera velocidade horizontal
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        moveInput.x = 0f;

        yield return new WaitForSeconds(sliceFreezeTime);
        isSliceFrozen = false;
    }

    // ---------------- Vida / Dano ----------------
    public void TakeDamage()
    {
        if (isDead || isTakingDamage) return;

        currentHealth--;
        animator.SetTrigger("Damage");
        isTakingDamage = true;

        // Knockback (empurra na direção oposta ao olhar)
        int dir = isFacingRight ? -1 : 1;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir * damagePushForceX, damagePushForceY), ForceMode2D.Impulse);

        // Atualiza UI
        var ui = FindAnyObjectByType<VidaUIController>();
        if (ui != null) ui.UpdateVida();

        if (currentHealth <= 0)
            StartCoroutine(Die());
        else
            StartCoroutine(RecoverFromDamage(0.5f));
    }

    private System.Collections.IEnumerator RecoverFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTakingDamage = false;
    }

    private System.Collections.IEnumerator Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        gameObject.layer = LayerMask.NameToLayer("Dead");

        // Pequeno atraso para garantir execução da animação
        yield return new WaitForSeconds(0.2f);
        enabled = false; // Desativa este script
    }

    // ---------------- Colisão com Inimigos ----------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("EnemyAttack"))
            TakeDamage();
    }

    // ---------------- Utilidades ----------------
    private void HandleFlip()
    {
        if (isDashing || isWallJumping || isSliceFrozen || isTakingDamage) return;
        if (moveInput.x > 0 && !isFacingRight) Flip();
        else if (moveInput.x < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(
            transform.localScale.x * -1f,
            transform.localScale.y,
            transform.localScale.z);
    }
    #endregion

    #region === Gizmos ===
    private void OnDrawGizmosSelected()
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
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
        }
    }
    #endregion
}