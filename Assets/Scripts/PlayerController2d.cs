// PlayerController2D.cs (v1.2)
// -----------------------------------------------------------------------------
// + Adicionado suporte a **Pulo Duplo**.
//   • Campo [Header("Pulo Duplo")] → `extraJumps` (quantos saltos extras no ar).
//   • Controle interno `jumpsLeft` reseta ao tocar o chão e decrementa ao usar
//     o pulo extra.
//   • Toda a lógica principal, comentários e estrutura foram preservados.
// -----------------------------------------------------------------------------
// Demais instruções de uso permanecem exatamente as mesmas.
// -----------------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    #region === Configurações no Inspector ===
    [Header("Movimentação")]
    [SerializeField] private float runMaxSpeed = 8f;           // Velocidade‑alvo máxima
    [SerializeField] private float runAcceleration = 60f;      // Aceleração no chão
    [SerializeField] private float runDeceleration = 48f;      // Desaceleração no chão
    [SerializeField, Range(0f, 1f)] private float accelInAir = .45f; // Acel. no ar
    [SerializeField, Range(0f, 1f)] private float decelInAir = .45f; // Desacel. no ar

    [Header("Pulo")]
    [SerializeField] private float jumpForce = 16f;            // Força do pulo (Impulse)
    [SerializeField] private float coyoteTime = 0.15f;         // "Tempo do coiote"
    [SerializeField] private float jumpBufferTime = 0.15f;     // Buffer de input
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f; // Corte de pulo

    [Header("Pulo Duplo")]
    [Tooltip("Número de saltos extras permitidos no ar (1 = duplo pulo)")]
    [SerializeField] private int extraJumps = 1;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;

    [Header("Wall Slide & Jump")]
    [SerializeField] private float wallSlideSpeed = 3f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(14f, 16f);
    [SerializeField] private float wallJumpTime = 0.2f;        // Controle horizontal reduzido

    [Header("Checagens (Empty Children)")]
    [SerializeField] private Transform groundCheck = null;     // Centro da caixa
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.2f, 0.05f);
    [Space(4)]
    [SerializeField] private Transform wallCheck = null;       // Lado que o player olha
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 0.2f);
    [Space(4)]
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region === Variáveis internas ===
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    private Vector2 moveInput;                 // Entrada de eixo (–1 a 1)
    private float lastOnGroundTime;            // Timer de coyote
    private float jumpBufferCounter;           // Timer de buffer

    // Estados
    private bool isJumping;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpCounter;
    private bool canDash = true;
    private bool isDashing;

    // Pulo Duplo
    private int jumpsLeft;                     // Saltos extras restantes
    #endregion

    #region === Inicialização ===
    private void Awake()
    {
        // Verifica (ou adiciona) Rigidbody2D
        rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();

        // Configurações padrão recomendadas
        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        jumpsLeft = extraJumps; // inicializa contador de pulo duplo
    }
    #endregion

    #region === Loop principal ===
    private void Update()
    {
        HandleTimers();
        ReadInput();
        CheckCollisions();
        HandleJump();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;               // Dash bloqueia movimento padrão

        ApplyHorizontalMovement();           // Acelera/desacelera suavemente
        ApplyWallSlide();                    // Limita velocidade de queda na parede
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

        // Buffer de pulo
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;

        // Corte de pulo: reduz velocidade ao soltar espaço
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(Dash());
    }

    // ----------- Checagens de colisão -----------
    private void CheckCollisions()
    {
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        bool onWall   = Physics2D.OverlapBox(wallCheck.position,   wallCheckSize,   0f, groundLayer);

        if (grounded)
        {
            lastOnGroundTime = coyoteTime;
            jumpsLeft = extraJumps; // reseta pulo duplo ao tocar o chão
        }

        // Wall Slide = encostado na parede, não no chão e caindo
        if (onWall && !grounded && rb.linearVelocity.y < 0f && !isWallJumping)
            isWallSliding = true;
        else
            isWallSliding = false;
    }

    // ---------------- Movimentação ----------------
    private void ApplyHorizontalMovement()
    {
        bool onWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
        if (onWall && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return; // impede qualquer aceleração lateral
        }

        float targetSpeed = moveInput.x * runMaxSpeed;
        float accelRate   = (Mathf.Abs(targetSpeed) > 0.01f) ? runAcceleration : runDeceleration;
        if (lastOnGroundTime <= 0f)
            accelRate *= (Mathf.Abs(targetSpeed) > 0.01f) ? accelInAir : decelInAir;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement  = speedDiff * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void ApplyWallSlide()
    {
        if (isWallSliding)
        {
            float yVel = Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, yVel);
        }
    }

    // ---------------- Pulo & Wall Jump ----------------
    private void HandleJump()
    {
        // Condições para pular (ground / wall / extra jump)
        if (jumpBufferCounter > 0f)
        {
            // ---- PULO NO CHÃO OU COYOTE ----
            if (lastOnGroundTime > 0f)
            {
                PerformGroundJump();
            }
            // ---- WALL JUMP ----
            else if (isWallSliding)
            {
                PerformWallJump();
            }
            // ---- PULO DUPLO ----
            else if (jumpsLeft > 0)
            {
                PerformDoubleJump();
            }
        }

        // Tempo de controle reduzido após wall jump expirar
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
    }

    private void PerformWallJump()
    {
        isJumping = true;
        isWallJumping = true;
        wallJumpCounter = wallJumpTime;
        jumpBufferCounter = 0f;

        int dir = isFacingRight ? -1 : 1; // Direção oposta à parede

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(wallJumpForce.x * dir, wallJumpForce.y), ForceMode2D.Impulse);

        if ((dir > 0 && !isFacingRight) || (dir < 0 && isFacingRight))
            Flip();
    }

    private void PerformDoubleJump()
    {
        isJumping = true;
        jumpBufferCounter = 0f;
        jumpsLeft--; // consome um pulo extra

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ---------------- Dash ----------------
    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float dashDir = isFacingRight ? 1f : -1f;
        float startTime = Time.time;
        float storedGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        while (Time.time - startTime < dashDuration)
            yield return null;

        rb.gravityScale = storedGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ---------------- Utilidades ----------------
    private void HandleFlip()
    {
        if (isDashing || isWallJumping) return; // Evita flip durante estas ações

        if (moveInput.x > 0 && !isFacingRight) Flip();
        else if (moveInput.x < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }
    #endregion

    #region === Gizmos (editor) ===
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
    }
    #endregion
}
