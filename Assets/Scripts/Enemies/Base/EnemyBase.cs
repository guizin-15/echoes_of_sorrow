using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 2f;
    private Vector3 initialPosition;

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

    [Header("Vida")]
    public int maxHealth = 2;
    protected int currentHealth;
    private int startingHealth;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Collider2D col;

    protected bool isFacingRight = true;
    protected bool isGrounded;
    protected bool isWallAhead;
    protected bool isEdgeAhead;
    protected bool isPlayerDetected;
    protected bool isTakingDamage = false;
    protected bool isDead = false;

    [SerializeField] protected bool disableColliderOnDeath = true;

    [Header("Drops")]
    public GameObject coinPrefab;
    public int coinsToDrop = 1;

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

        currentHealth = maxHealth;
        startingHealth = maxHealth;

        initialPosition = transform.position;
    }

    protected virtual void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        anim.SetBool("IsGrounded", isGrounded);

        if (isDead) return;

        if (isTakingDamage)
        {
            if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
            {
                isTakingDamage = false;
                anim.SetBool("IsTakingDamage", false);
            }
            else
            {
                return;
            }
        }

        if (!isTakingDamage && isGrounded)
        {
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

            if (CanMove())
            {
                if (!isEdgeAhead || isWallAhead)
                {
                    Flip();
                }

                float direction = isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("Death");

        if (coinPrefab != null && coinsToDrop > 0)
        {
            for (int i = 0; i < coinsToDrop; i++)
            {
                Vector2 offset = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.3f));
                GameObject coin = Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);

                Rigidbody2D crb = coin.GetComponent<Rigidbody2D>();
                if (crb != null)
                {
                    Vector2 force = new Vector2(Random.Range(-4f, 5f), Random.Range(4f, 10f));
                    crb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }

        OnDeath();
    }

    protected virtual void OnDeath() { }

    public virtual void TakeDamage()
    {
        if (isDead) return;

        if (currentHealth > 1)
        {
            anim.SetTrigger("Damage");
            anim.SetBool("IsTakingDamage", true);
            isTakingDamage = true;
        }

        currentHealth--;

        rb.linearVelocity = Vector2.zero;

        float pushDirection = Mathf.Sign(transform.position.x - player.position.x);
        Vector2 pushForce = currentHealth <= 0 ? new Vector2(pushDirection * 5f, 0f) : new Vector2(pushDirection * 5f, 5f);
        rb.AddForce(pushForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual bool CanMove() => true;

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

    public virtual void ResetEnemy()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();

        currentHealth = startingHealth;
        isDead = false;
        isTakingDamage = false;

        // REPOSICIONA O INIMIGO
        transform.position = initialPosition;

        anim.Play("Idle");

        foreach (var c in GetComponents<Collider2D>())
            c.enabled = true;

        if (rb != null)
            rb.simulated = true;

        gameObject.layer = LayerMask.NameToLayer("Enemy");
        gameObject.tag = "Enemy";
        enabled = true;
    }

}
