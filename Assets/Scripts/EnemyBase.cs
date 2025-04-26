using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 2f;

    [Header("Detecção de chão e parede")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform edgeCheck;
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.05f);
    public Vector2 wallCheckSize = new Vector2(0.05f, 0.2f);
    public Vector2 edgeCheckSize = new Vector2(0.2f, 0.05f);
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Detecção do Player")]
    public Transform player;
    public float detectionRadius = 5f;

    [Header("Reação a Dano")]
    public float damagePushForceX = 5f;
    public float damagePushForceY = 5f;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Collider2D col;

    protected bool isFacingRight = true;
    protected bool isGrounded;
    protected bool isWallAhead;
    protected bool isEdgeAhead;
    protected bool isPlayerDetected;

    protected bool isPreparingAttack = false;
    protected bool isAttacking = false;
    protected bool isTakingDamage = false;
    protected bool isDead = false;

    protected int health = 2;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    protected virtual void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        anim.SetBool("IsGrounded", isGrounded);

        if (isDead && isGrounded)
        {
            anim.SetTrigger("Death");
            gameObject.layer = LayerMask.NameToLayer("Dead");
            this.enabled = false;
        }

        if (isTakingDamage && isGrounded)
        {
            isTakingDamage = false;
            anim.SetBool("IsTakingDamage", false);
        }

        if (isTakingDamage || !isGrounded) return;

        isWallAhead = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
        isEdgeAhead = Physics2D.OverlapBox(edgeCheck.position, edgeCheckSize, 0f, groundLayer);

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            isPlayerDetected = distanceToPlayer <= detectionRadius;
            anim.SetBool("isPlayerDetected", isPlayerDetected);
        }
        else
        {
            isPlayerDetected = false;
            anim.SetBool("isPlayerDetected", false);
        }

        if (isPlayerDetected)
        {
            if (!isPreparingAttack && !isAttacking)
            {
                isPreparingAttack = true;
                isAttacking = true;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                anim.SetTrigger("PrepAttack");
                anim.SetBool("IsAttacking", true);
            }

            if ((player.position.x > transform.position.x && !isFacingRight) ||
                (player.position.x < transform.position.x && isFacingRight))
            {
                Flip();
            }
        }
        else
        {
            isPreparingAttack = false;
            isAttacking = false;
            anim.SetBool("IsAttacking", false);

            if (!isEdgeAhead || isWallAhead)
            {
                Flip();
            }

            float direction = isFacingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
    }

    public virtual void TakeDamage()
    {
        if (isDead) return;

        anim.SetTrigger("Damage");
        isTakingDamage = true;
        anim.SetBool("IsTakingDamage", true);
        health--;

        float pushDirection = isFacingRight ? -1f : 1f;
        Vector2 pushForce = health <= 0
            ? new Vector2(pushDirection * damagePushForceX, 0f)
            : new Vector2(pushDirection * damagePushForceX, damagePushForceY);

        rb.AddForce(pushForce, ForceMode2D.Impulse);

        if (health <= 0)
        {
            isDead = true;
        }
    }

    protected void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public virtual void OnDrawGizmosSelected()
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

        if (edgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(edgeCheck.position, edgeCheckSize);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}