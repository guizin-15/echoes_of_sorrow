using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Checagem de chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Checagem de parede")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 0.5f;

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(8f, 12f);

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isAttacking;
    private bool isTouchingWall;
    private bool isWallSliding;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Movimento horizontal
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip do sprite normal
        if (moveInput != 0 && !isWallSliding)
            sr.flipX = moveInput < 0;

        // Pulo comum e wall jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWallSliding)
            {
                // Wall jump direção contrária à parede
                if (Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, wallLayer))
                {
                    rb.linearVelocity = new Vector2(wallJumpForce.x, wallJumpForce.y); // pulo pra direita
                    sr.flipX = false;
                }
                else if (Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayer))
                {
                    rb.linearVelocity = new Vector2(-wallJumpForce.x, wallJumpForce.y); // pulo pra esquerda
                    sr.flipX = true;
                }

                anim.SetTrigger("Jump");
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetTrigger("Jump");
            }
        }

        // Ataque Slash
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            bool canAttack = stateInfo.IsName("Idle") || stateInfo.IsName("Run");

            if (canAttack)
            {
                anim.SetTrigger("Slash");
                isAttacking = true;
                StartCoroutine(ResetAttack(0.4f));
            }
        }

        // Wall Slide lógica
        if (!isGrounded && isTouchingWall && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            anim.Play("WallSlide");

            // Corrigir flip durante wall slide
            if (Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, wallLayer))
                sr.flipX = true;
            else if (Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayer))
                sr.flipX = false;
        }
        else
        {
            isWallSliding = false;
        }

        // Atualiza parâmetros do Animator
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);

        // Aterrissagem
        if (!wasGrounded && isGrounded)
        {
            anim.SetTrigger("Land");
        }

        wasGrounded = isGrounded;
    }

    private IEnumerator ResetAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        Debug.Log("Ataque destravado automaticamente.");
    }

    void FixedUpdate()
    {
        // Checa chão
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Checa parede com raycasts laterais
        bool touchingLeft = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, wallLayer);
        bool touchingRight = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayer);
        isTouchingWall = touchingLeft || touchingRight;

        // Debug visual dos raycasts
        Debug.DrawRay(wallCheckLeft.position, Vector2.left * wallCheckDistance, Color.cyan);
        Debug.DrawRay(wallCheckRight.position, Vector2.right * wallCheckDistance, Color.cyan);
    }

    void OnDrawGizmosSelected()
    {
        // Visual do GroundCheck
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visual dos WallChecks
        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(wallCheckLeft.position, wallCheckLeft.position + Vector3.left * wallCheckDistance);
        }

        if (wallCheckRight != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(wallCheckRight.position, wallCheckRight.position + Vector3.right * wallCheckDistance);
        }
    }
}
