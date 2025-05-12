using System;
using UnityEngine;

public class SpiderController : MonoBehaviour
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

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isWallAhead;
    private bool isEdgeAhead;
    private bool isPlayerDetected;

    private bool isPreparingAttack = false;
    private bool isAttacking = false;
    private bool isTakingDamage = false;

    private int health = 2;
    private bool isDead = false;

    [Header("Ataque de Projétil")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f;
    public float projectileCooldown = 1.5f;
    private float projectileCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Encontra o Player automaticamente pela Tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        // Se está tomando dano, espera cair no chão antes de qualquer outra ação
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
            anim.SetBool("IsGrounded", isGrounded);

            if (isDead && isGrounded)
            {
                anim.SetTrigger("Death");
                gameObject.layer = LayerMask.NameToLayer("Dead"); // Muda para layer "Dead" para manter colisão com chão
                this.enabled = false; // Desativa o script
            }

            if (isGrounded)
            {
                Debug.Log("Spider landed on the ground.");
                isTakingDamage = false;
                anim.SetBool("IsTakingDamage", false);
            }
        }

        if (!isTakingDamage && isGrounded)

        {
            // Checagem de ambiente
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
            isWallAhead = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
            isEdgeAhead = Physics2D.OverlapBox(edgeCheck.position, edgeCheckSize, 0f, groundLayer);

            // Detecta player via distância
            if (!isTakingDamage){}
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

            if (isPlayerDetected && player != null)
            {
                if (!isPreparingAttack && !isAttacking)
                {
                    isPreparingAttack = true;
                    isAttacking = true;
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
                    anim.SetTrigger("PrepAttack");
                    anim.SetBool("IsAttacking", true);
                }

                if (isPreparingAttack || isAttacking)
                {
                    if ((player.position.x > transform.position.x && !isFacingRight) ||
                        (player.position.x < transform.position.x && isFacingRight))
                    {
                        Flip();
                    }
                }
            }
            else
            {
                isPreparingAttack = false;
                isAttacking = false;
                anim.SetBool("IsAttacking", false);

                if (isGrounded)
                {
                    if (!isEdgeAhead || isWallAhead)
                    {
                        Flip();
                    }
                }

                float direction = isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocityY);
            }

            // Atualiza IsGrounded sempre
            anim.SetBool("IsGrounded", isGrounded);


        }

        // Projétil ataque
        if (isAttacking && !isTakingDamage && isGrounded)
        {
            if (projectileCooldownTimer <= 0f)
            {
                ShootProjectile();
                projectileCooldownTimer = projectileCooldown;
            }
            else
            {
                projectileCooldownTimer -= Time.deltaTime;
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead) return; // Se já está morto, não toma mais dano

        // Toca a animação de dano
        anim.SetTrigger("Damage");
        isTakingDamage = true;
        anim.SetBool("IsTakingDamage", true);

        // Reduz a vida
        health--;

        // Calcula direção do empurrão
        float pushDirection = isFacingRight ? -1f : 1f;
        Vector2 pushForce;

        if (health <= 0)
        {
            isDead = true;
            // Se morrer, só empurra no eixo X
            pushForce = new Vector2(pushDirection * damagePushForceX, 0f);
        }
        else
        {
            // Se ainda tem vida, empurra normal X e Y
            pushForce = new Vector2(pushDirection * damagePushForceX, damagePushForceY);
        }

        rb.AddForce(pushForce, ForceMode2D.Impulse);
    }

    public void OnPrepAttackComplete()
    {
        isPreparingAttack = false;
        isAttacking = true;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

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

        if (edgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(edgeCheck.position, edgeCheckSize);
        }

        // Visualizar o raio de percepção
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            float direction = isFacingRight ? 1f : -1f;
            prb.linearVelocity = new Vector2(direction * projectileSpeed, 0f);
        }
    }
}