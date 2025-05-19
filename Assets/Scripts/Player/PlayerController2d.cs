// PlayerController2D.cs ─ v2.4
// -----------------------------------------------------------------------------
// - Dash infinito NO CHÃO, mas somente 1 dash por salto aéreo
// - Cooldown (dashCooldown) respeitado em qualquer situação
// - Demais sistemas (combo-dash, ataques, wall-slide, etc.) inalterados & seguros
// -----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    #region === Configurações no Inspector ===
    /* -------- Movimento -------- */
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

    /* -------- Dash -------- */
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;

    /* -------- Combo Dash -------- */
    [Header("Combo Dash (antes do Slash)")]
    [SerializeField] private float comboDashSpeed = 10f;
    [SerializeField] private float comboDashDuration = 0.2f;

    /* -------- Wall / Jump -------- */
    [Header("Wall Slide & Jump")]
    [SerializeField] private float wallSlideSpeed = 3f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(14f, 16f);
    [SerializeField] private float wallJumpTime = 0.2f;

    /* -------- Checagens -------- */
    [Header("Checagens (Empty Children)")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.15f, 0.04f);
    [Space(4)]
    [SerializeField] private Transform wallCheck = null;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 0.2f);
    [Space(4)]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask barrierLayer;

    /* -------- Ataques -------- */
    [Header("Slice")]
    [SerializeField] private float sliceCooldown = 0.9f;
    [SerializeField] private float sliceFreezeTime = 0.6f;

    [Header("Slash")]
    [SerializeField] private float slashCooldown = 0.35f;
    [SerializeField] private float slashDuration = 0.3f;

    /* -------- Hitboxes -------- */
    [Header("Hitboxes")]
    [SerializeField] private Transform sliceAttackPoint = null;
    [SerializeField] private Vector2 sliceBoxSize = new Vector2(1.2f, 0.6f);
    [Space(4)]
    [SerializeField] private Transform slashAttackPoint = null;
    [SerializeField] private Vector2 slashBoxSize = new Vector2(1.0f, 1.0f);
    [SerializeField] private LayerMask enemyLayer;

    /* -------- Vida -------- */
    [Header("Vida")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;
    public bool isTakingDamage = false;

    [Header("Barra de Vida")]
    [SerializeField] public HealthBar healthBar; // arraste seu prefab/objeto HealthBar aqui

    [Header("Coleta de Moedas")]
    public int coinsCollected = 0;

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

    /* Movimento */
    public bool isGrounded, isJumping, isWallSliding, isWallJumping;
    private float wallJumpCounter;
    private int jumpsLeft;

    /* Dash */
    private bool isDashing;
    private bool isComboDashing;
    private bool isDashCoroutineActive;
    private float lastDashTime;
    private bool hasDashedInAir;        // <-- NOVO : controla 1 dash aéreo

    /* Ataques */
    private float lastSliceTime, lastSlashTime;
    private bool isSliceFrozen, isSlashActive;
    private bool queuedSlashAfterSlice, queuedSliceAfterSlash;
    private bool isPerformingSlice, isPerformingSlash;
    #endregion

    #region === Inicialização & Loop ===
    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHealth = maxHealth;
        if (healthBar) healthBar.SetMaxHealth(maxHealth);
        jumpsLeft = extraJumps;
    }

    private void Update()
    {
        if (isDead) return;

        HandleTimers();

        if (!isTakingDamage) ReadInput();
        else                 moveInput.x = 0f;

        CheckCollisions();
        HandleJump();
        HandleFlip();
        UpdateAnimator();
        HandleAttacks();
    }

    private void FixedUpdate()
    {
        if (isDashing || isComboDashing) return;
        ApplyHorizontalMovement();
        ApplyWallSlide();
    }
    #endregion

    #region === Utilidades ===
    private bool IsAttacking()
        => isSliceFrozen || isSlashActive || isComboDashing ||
           isPerformingSlice || isPerformingSlash;
    #endregion

    #region === Timers / Entrada ===
    private void HandleTimers()
    {
        lastOnGroundTime  -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
        if (isWallJumping) wallJumpCounter -= Time.deltaTime;
    }

    private void ReadInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f && !isWallJumping)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        /* ---------- DASH ---------- */
        bool dashPressed = Input.GetMouseButtonDown(1);
        bool cdOK        = Time.time >= lastDashTime + dashCooldown;

        if (dashPressed && cdOK && !isDashCoroutineActive && !isWallSliding && !IsAttacking())
        {
            if (isGrounded)                           // dashes ilimitados no chão (respeita cooldown)
            {
                StartCoroutine(Dash());
            }
            else if (!hasDashedInAir)                 // 1 dash por salto
            {
                StartCoroutine(Dash());
                hasDashedInAir = true;
            }
        }
    }
    #endregion

    #region === Checagens de Colisão ===
    private void CheckCollisions()
    {  
        int maskSemBarrier = groundLayer & ~LayerMask.GetMask("Barrier");
        bool groundedNow = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, maskSemBarrier);


        bool onWall = Physics2D.OverlapBox(wallCheck.position,  wallCheckSize,  0f, groundLayer);
        bool onBarrier = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, barrierLayer);

        if (groundedNow && !isGrounded)               // acabou de aterrar
        {
            hasDashedInAir = false;                  // reseta dash aéreo
        }

        if (groundedNow)
        {
            lastOnGroundTime = coyoteTime;
            jumpsLeft        = extraJumps;
        }

        isGrounded    = groundedNow;
        isWallSliding = (onWall || onBarrier) && !groundedNow && rb.linearVelocity.y < 0f && !isWallJumping;
    }
    #endregion

    #region === Movimento Horizontal & Wall Slide ===
    private void ApplyHorizontalMovement()
    {
        if (isGrounded && IsAttacking() || isTakingDamage) return;
        if (isWallJumping)                                 return;

        bool onWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
        bool onBarrier = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, barrierLayer);

        if ((onBarrier || onWall) && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float targetSpeed = moveInput.x * runMaxSpeed;
        float accelRate   = Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration : runDeceleration;
        if (lastOnGroundTime <= 0f)
            accelRate *= Mathf.Abs(targetSpeed) > 0.01f ? accelInAir : decelInAir;

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
    #endregion

    #region === Pulos ===
    private void HandleJump()
    {
        if (IsAttacking() || isDashing || isTakingDamage) return;

        if (jumpBufferCounter > 0f)
        {
            if (lastOnGroundTime > 0f)      PerformGroundJump();
            else if (isWallSliding && Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer)) PerformWallJump();
            else if (jumpsLeft > 0 && !Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, barrierLayer)) PerformDoubleJump();
        }

        if (isWallJumping && wallJumpCounter <= 0f)
            isWallJumping = false;
    }

    private void PerformGroundJump()
    {
        isJumping = true;
        jumpBufferCounter = 0f;
        lastOnGroundTime  = 0f;

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

        if ((dir > 0 && !isFacingRight) || (dir < 0 && isFacingRight))
            Flip();

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
    #endregion

    #region === DASH normal (protegido) ===
    private IEnumerator Dash()
    {
        if (isDashCoroutineActive) yield break;
        isDashCoroutineActive = true;
        lastDashTime          = Time.time;

        isDashing = true;
        animator.SetTrigger("Dash");

        Vector2 originalVelocity = rb.linearVelocity;
        float   originalGravity  = rb.gravityScale;

        rb.gravityScale   = 0f;
        rb.linearVelocity = new Vector2((isFacingRight ? 1f : -1f) * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale   = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isDashing             = false;
        isDashCoroutineActive = false;
    }
    #endregion

    #region === Animator ===
    private void UpdateAnimator()
    {
        animator.SetFloat("Speed",         Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool ("IsGrounded",    isGrounded);
        animator.SetBool ("IsFalling",     rb.linearVelocity.y < -0.1f && !isGrounded);
        animator.SetBool ("IsWallSliding", isWallSliding);
    }
    #endregion

    #region === Ataques (Slice ↔ Slash) ===
    private void HandleAttacks()
    {
        if (!Input.GetMouseButtonDown(0) || isWallSliding || isTakingDamage) return;

        if (isSliceFrozen)                    { queuedSlashAfterSlice = true; return; }
        if (isSlashActive || isComboDashing)  { queuedSliceAfterSlash = true; return; }

        if (isGrounded)                      TryExecuteSlice();
        else if (rb.linearVelocity.y > 0.1f) TryExecuteSlash();
        else                                 TryExecuteSlice();
    }

    /* --- Slice (sem mudanças) --- */
    private void TryExecuteSlice()
    {
        if (Time.time < lastSliceTime + sliceCooldown || isSlashActive) return;

        isPerformingSlice = true;
        isPerformingSlash = false;
        lastSliceTime     = Time.time;
        animator.SetTrigger("Slice");

        PerformAttack();

        if (isGrounded) StartCoroutine(FreezeDuringSlice());
        else            StartCoroutine(EndAirSlice());
    }

    private IEnumerator FreezeDuringSlice()
    {
        isSliceFrozen = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        moveInput.x = 0f;

        yield return new WaitForSeconds(sliceFreezeTime);

        isSliceFrozen     = false;
        isPerformingSlice = false;

        if (queuedSlashAfterSlice && !isTakingDamage)
            StartCoroutine(ComboDashThenSlash());

        queuedSlashAfterSlice = false;
    }

    private IEnumerator EndAirSlice()
    {
        yield return new WaitForSeconds(sliceFreezeTime);
        isPerformingSlice = false;
    }

    /* --- Slash (sem mudanças) --- */
    private void TryExecuteSlash()
    {
        if (Time.time < lastSlashTime + slashCooldown || isSliceFrozen) return;
        ExecuteSlash();
    }

    private void ExecuteSlash()
    {
        isPerformingSlash = true;
        isPerformingSlice = false;
        isSlashActive     = true;
        lastSlashTime     = Time.time;

        if (isGrounded)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        animator.SetTrigger("Slash");

        PerformAttack();
        StartCoroutine(EndSlashAfterTime());
    }

    private IEnumerator EndSlashAfterTime()
    {
        yield return new WaitForSeconds(slashDuration);
        isSlashActive     = false;
        isPerformingSlash = false;

        if (queuedSliceAfterSlash && !isTakingDamage)
            StartCoroutine(ExecuteSliceAfterCooldown());

        queuedSliceAfterSlash = false;
    }

    /* --- Combo Dash → Slash (sem mudanças) --- */
    private IEnumerator ComboDashThenSlash()
    {
        float waitCD = Mathf.Max(0f, (lastSlashTime + slashCooldown) - Time.time);
        if (waitCD > 0f) yield return new WaitForSeconds(waitCD);

        isComboDashing = true;
        float dir = isFacingRight ? 1f : -1f;
        float storedGravity = rb.gravityScale;
        rb.gravityScale     = 0f;
        rb.linearVelocity   = new Vector2(dir * comboDashSpeed, 0f);

        yield return new WaitForSeconds(comboDashDuration);

        rb.gravityScale = storedGravity;
        isComboDashing  = false;

        ExecuteSlash();  // Slash parado
    }

    private IEnumerator ExecuteSliceAfterCooldown()
    {
        float wait = Mathf.Max(0f, (lastSliceTime + sliceCooldown) - Time.time);
        if (wait > 0f) yield return new WaitForSeconds(wait);
        TryExecuteSlice();
    }
    #endregion

    #region === Hitbox (sem mudanças) ===
    private void PerformAttack()
    {
        Collider2D[] hits = null;

        if (isPerformingSlice && sliceAttackPoint)
            hits = Physics2D.OverlapBoxAll(sliceAttackPoint.position, sliceBoxSize, 0f, enemyLayer);
        else if (isPerformingSlash && slashAttackPoint)
            hits = Physics2D.OverlapBoxAll(slashAttackPoint.position, slashBoxSize, 0f, enemyLayer);

        // if (hits == null) return;
        // foreach (Collider2D h in hits)
        //     h.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);

        // Debug.Log("Ataque executado!");

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("⚠️ Nenhum inimigo atingido.");
        }
        else
        {
            foreach (Collider2D h in hits)
            {
                Debug.Log($"🎯 Acertou: {h.name} (Tag: {h.tag})");
                h.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
            }
        }

    }
    #endregion

    #region === Vida / Dano / Morte (sem mudanças) ===
    public void TakeDamage(int dmg = 10)
    {
        if (isDead || isTakingDamage) return;

        currentHealth = Mathf.Max(currentHealth - dmg, 0);

        // Atualiza barra de vida
        if (healthBar) healthBar.SetHealth(currentHealth);

        bool willDie = currentHealth <= 0;
        if (willDie) { StartCoroutine(Die()); return; }

        animator.SetTrigger("Damage");
        isTakingDamage = true;

        queuedSlashAfterSlice = queuedSliceAfterSlash = false;

        int dir = isFacingRight ? -1 : 1;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir * damagePushForceX, damagePushForceY),
                    ForceMode2D.Impulse);

        StartCoroutine(RecoverFromDamage(0.5f));
    }

    private IEnumerator RecoverFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTakingDamage = false;
    }

    private IEnumerator Die()
    {
        isDead = true;
        queuedSlashAfterSlice = queuedSliceAfterSlash = false;
        isSlashActive = isSliceFrozen = isComboDashing = isDashing = false;

        animator.SetTrigger("Die");
        gameObject.layer = LayerMask.NameToLayer("Dead");

        yield return new WaitForSeconds(0.2f);
        enabled = false;

        FindAnyObjectByType<GameManager>().PlayerMorreu();
    }
    #endregion

    #region === Colisão com Inimigos ===
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Enemy") || col.collider.CompareTag("EnemyAttack"))
            TakeDamage();
    }
    #endregion

    #region === Flip ===
    private void HandleFlip()
    {
        if (IsAttacking() || isWallJumping || isDashing) return;

        if (moveInput.x > 0 && !isFacingRight)      Flip();
        else if (moveInput.x < 0 && isFacingRight)  Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }
    #endregion

    #region === Gizmos ===
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (wallCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
        if (sliceAttackPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(sliceAttackPoint.position, sliceBoxSize);
        }
        if (slashAttackPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(slashAttackPoint.position, slashBoxSize);
        }
    }
    #endregion
}