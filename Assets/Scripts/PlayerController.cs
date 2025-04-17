using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 6.5f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 0.4f;

    [Header("Wall Jump")]
    public float wallJumpingTime = 0.2f;
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(2.5f, 6.5f);

    [Header("Checagem de chão/parede")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.1f;
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

    private bool isAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
        horizontal = Input.GetAxisRaw("Horizontal");

        // Checa estados
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        bool isWalled = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);

        // Wall Slide
        if (isWalled && !isGrounded && !isWallJumping)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0f, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
            anim.Play("WallSlide");

            // Flip correto baseado na parede
            if (wallCheck.position.x < transform.position.x)
            {
                // Parede à esquerda
                if (!isFacingRight)
                {
                    isFacingRight = true;
                    Flip();
                }
            }
            else
            {
                // Parede à direita
                if (isFacingRight)
                {
                    isFacingRight = false;
                    Flip();
                }
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

        // Pulo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWallSliding || (wallJumpingCounter > 0f && !isGrounded && isWalled))
            {
                isWallJumping = true;
                rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
                wallJumpingCounter = 0f;

                if (transform.localScale.x != wallJumpingDirection)
                {
                    isFacingRight = !isFacingRight;
                    Flip();
                }

                anim.SetTrigger("Jump");
                Invoke(nameof(StopWallJumping), wallJumpingDuration);
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

        // Flip durante movimentação normal
        if (!isWallJumping && !isWallSliding && horizontal != 0f)
        {
            if (horizontal < 0 && isFacingRight || horizontal > 0 && !isFacingRight)
            {
                isFacingRight = !isFacingRight;
                Flip();
            }
        }

        // Aterrissagem (Land)
        if (!wasGrounded && isGrounded)
        {
            anim.SetTrigger("Land");
        }
        wasGrounded = isGrounded;

        // Animações base
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        // NÃO sobrescreve velocity.x durante WallSlide
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
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }
    }
}