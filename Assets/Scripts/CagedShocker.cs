using UnityEngine;
using System.Collections;

/// <summary>
/// CagedShocker – inimigo parado em sua gaiola que dá choques rápidos ou um ataque forte.  test
/// Deriva de EnemyBase e respeita as seguintes regras:
/// • Knock‑back normal apenas quando morre.
/// • Ao tomar dano vivo, não há knock‑back; ele congela e depois executa um Fast Attack aleatório.
/// • Possui dois círculos: detectionRadius (já herdado do EnemyBase, anda até o jogador) e attackRange (executa ataques cíclicos).
/// • Sempre executa PrepAttack, depois aleatoriamente FastAttack1 ou FastAttack2.
/// • Após qualquer ataque avança alguns pixels.
/// </summary>
public class CagedShocker : EnemyBase
{
    [Header("Ranges")]
    public float attackRange = 2.5f;   // range em que ele realmente ataca

    [Header("Attack settings")]
    public float attackCooldown = 0.5f;     // tempo entre ataques quando em alcance

    [Header("Slide settings")]
    [Tooltip("Distância percorrida (unidades) durante o ataque.")]
    public float slideDistance = 1.5f;
    [Tooltip("Duração do deslizamento em segundos.")]
    public float slideTime = 0.15f;

    [Header("Damage hitbox")]
    [Tooltip("Caixa de dano usada durante os ataques. Precisa estar em IsTrigger.")]
    public BoxCollider2D attackCollider;
    public float fastHitboxActiveTime = 0.15f;

    [Header("Prep settings")]
    public float prepDuration = 0.3f; // duração do PrepAttack (clip deve usar Clamp Forever no último frame)

    private float attackCooldownTimer = 0f;
    private bool isAttacking = false;

    //------------------------------------------------------------------//
    //  INICIALIZAÇÃO                                                   //
    //------------------------------------------------------------------//
    protected override void Start()
    {
        base.Start();
        disableColliderOnDeath = false; // mantém colisor ativo até destroy
        if (attackCollider != null)
        {
            attackCollider.enabled = false; // desliga até o momento do ataque
            attackCollider.gameObject.tag = "EnemyAttack";
        }
    }

    //------------------------------------------------------------------//
    //  LÓGICA PRINCIPAL                                                //
    //------------------------------------------------------------------//
    protected override void Update()
    {
        base.Update(); // mantém animações de grounded etc.

        if (attackCooldownTimer > 0f)
        attackCooldownTimer -= Time.deltaTime;

        if (isDead || isTakingDamage) return;   // mantém bloqueio de dano/morte

        if (isAttacking) return; // só agora bloqueia movimentação

        float dist = Vector2.Distance(transform.position, player.position);

        // 1) Dentro do alcance de ataque -> ataca periodicamente
        if (dist <= attackRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (!isAttacking)
            {
                StartCoroutine(AttackSequence());
            }
        }
        // 2) Dentro do detectionRadius herdado
        else if (isPlayerDetected) // dentro do detectionRadius herdado
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

            if ((dir > 0f && !isFacingRight) || (dir < 0f && isFacingRight))
                Flip();
        }
        // 3) Player fora do alcance -> idle
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    //------------------------------------------------------------------//
    //  ATAQUES                                                         //
    //------------------------------------------------------------------//
    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown; // inicia o cooldown

        // 1. PREP único
        anim.SetTrigger("PrepAttack");
        yield return new WaitForSeconds(prepDuration);

        // Mantém pose carregada até cooldown acabar
        while (attackCooldownTimer > 0f)
            yield return null;

        // 2. Escolhe aleatoriamente Fast1 ou Fast2
        bool fastOne = Random.Range(0, 2) == 0;
        anim.SetTrigger(fastOne ? "FastAttack1" : "FastAttack2");

        // --- Deslizamento suave durante o ataque ---------------------//
        if (attackCollider != null)
            attackCollider.enabled = true;

        float slideSpeed = slideDistance / slideTime;

        float dirSign = isFacingRight ? 1f : -1f;

        float timer = 0f;
        while (timer < slideTime)
        {
            rb.linearVelocity = new Vector2(dirSign * slideSpeed, rb.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // espera restante da janela de hitbox, se houver
        float remaining = fastHitboxActiveTime - slideTime;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        if (attackCollider != null)
            attackCollider.enabled = false;

        isAttacking = false;
    }

    //------------------------------------------------------------------//
    //  TOMAR DANO                                                      //
    //------------------------------------------------------------------//
    public override void TakeDamage()
    {
        if (isDead) return;

        // Cancela qualquer ataque em execução
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            if (attackCollider != null)
                attackCollider.enabled = false;
        }

        // Diminui vida
        currentHealth--;
        rb.linearVelocity = Vector2.zero;

        float pushDir = Mathf.Sign(transform.position.x - player.position.x);

        // Morreu?
        if (currentHealth <= 0)
        {
            Vector2 push = new Vector2(pushDir * 5f, 0f); // força 3 apenas X
            rb.AddForce(push, ForceMode2D.Impulse);
            Die();               // chama morte – NÃO envia trigger Damage
            return;
        }

        //----------------------------------------------------------------//
        // Ainda vivo → animação de dano e knock‑back                      //
        //----------------------------------------------------------------//
        anim.SetTrigger("Damage");
        anim.SetBool("IsTakingDamage", true);
        isTakingDamage = true;

        Vector2 pushForce = new Vector2(pushDir * 4f, 2.5f); // força 3,3
        rb.AddForce(pushForce, ForceMode2D.Impulse);

        // Reinicia cooldown para não desincronizar próximos ataques
        attackCooldownTimer = attackCooldown;
    }


    //------------------------------------------------------------------//
    //  MORTE                                                           //
    //------------------------------------------------------------------//
    protected override void OnDeath()
    {
        // Mantém colisor para não cair, troca layer para Dead
        gameObject.layer = LayerMask.NameToLayer("Dead");
        // limpa flag de dano para garantir transição de animação Death
        anim.SetBool("IsTakingDamage", false);
        isTakingDamage = false;
    }


    //------------------------------------------------------------------//
    //  COLISÃO COM PLAYER                                              //
    //------------------------------------------------------------------//
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isTakingDamage || isAttacking) return;

        if (collision.collider.CompareTag("Player"))
        {
            // Se encostar, aplica choque rápido
            StartCoroutine(AttackSequence());
        }
    }

    //------------------------------------------------------------------//
    //  TRIGGER DETECÇÃO DE DANO NO PLAYER                              //
    //------------------------------------------------------------------//

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (attackCollider == null || !attackCollider.enabled) return;
        if (!isAttacking) return;

        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
        }
    }

    //------------------------------------------------------------------//
    //  EnemyBase movimenta-se apenas via Update() acima                //
    //------------------------------------------------------------------//
    protected override bool CanMove() => false;

    //------------------------------------------------------------------//
    //  GIZMOS – visualização dos ranges no Scene                       //
    //------------------------------------------------------------------//
    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();             // já desenha detectionRadius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = attackCollider.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(attackCollider.offset, attackCollider.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
    //------------------------------------------------------------------//
}