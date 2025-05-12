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

    [Header("Vida")]
    public int maxHealth = 2;
    protected int currentHealth;

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
    // Se verdadeiro, o colisor será desativado quando o inimigo morrer.
    [SerializeField] protected bool disableColliderOnDeath = true;

    [Header("Drops")]
    public GameObject coinPrefab;   // Prefab da moeda (opcional)
    public int coinsToDrop = 1;     // Quantidade de moedas a dropar

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
    }

    protected virtual void Update()
    {

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        anim.SetBool("IsGrounded", isGrounded);

        if (isDead) return;

        // Enquanto estiver tomando dano, bloqueia movimentação e flip
        if (isTakingDamage)
        {
            if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
            {
                isTakingDamage = false;
                anim.SetBool("IsTakingDamage", false);
            }
            else
            {
                return; // interrompe Update até fim do knockback
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
        isDead = true;   // <-- AQUI!!
        anim.SetTrigger("Death"); // <-- já toca animação aqui

        // --- Drop de moedas -----------------------------------------//
        if (coinPrefab != null && coinsToDrop > 0)
        {
            for (int i = 0; i < coinsToDrop; i++)
            {
                Vector2 offset = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.3f));
                GameObject coin = Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);

                // Aplica pequena força para espalhar (se tiver Rigidbody2D)
                Rigidbody2D crb = coin.GetComponent<Rigidbody2D>();
                if (crb != null)
                {
                    Vector2 force = new Vector2(Random.Range(-4f, 5f), Random.Range(4f, 10f));
                    crb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }

        OnDeath();       // chama comportamento customizado
    }

    // Método virtual que cada inimigo vai poder sobrescrever
    protected virtual void OnDeath()
    {
        // Nada aqui. Deixa para os filhos fazerem algo.
    }


    public virtual void TakeDamage()
    {
        if (isDead) return;

        if (currentHealth > 1) // ainda tem vida depois de tomar dano
        {
            anim.SetTrigger("Damage");
            anim.SetBool("IsTakingDamage", true);
            isTakingDamage = true;
        }

        currentHealth--;

        rb.linearVelocity = Vector2.zero;                 // zera movimento atual

        // +1 se o player está à direita, -1 se está à esquerda
        float pushDirection = Mathf.Sign(transform.position.x - player.position.x);

        Vector2 pushForce;

        if (currentHealth <= 0)
        {
            pushForce = new Vector2(pushDirection * 5f, 0f);  // morte: só X
        }
        else
        {
            pushForce = new Vector2(pushDirection * 5f, 5f);  // dano normal: X e Y
        }

        rb.AddForce(pushForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();  // <-- agora sim, depois de empurrar!
        }
    }

    protected virtual bool CanMove()
    {
        return true;
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