using UnityEngine;
using System.Collections; // <-- para usar corrotinas

public class CagedSpider : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        // Mantém o colisor ativo após a morte
        disableColliderOnDeath = false;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;
        if (isTakingDamage) return;

        if (!isTakingDamage && isGrounded)
        {
            if (isPlayerDetected)
            {
                float direction = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

                if ((direction > 0f && !isFacingRight) || (direction < 0f && isFacingRight))
                {
                    Flip();
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }

    protected override void OnDeath()
    {
        gameObject.layer = LayerMask.NameToLayer("Dead"); // muda para layer Dead
        Debug.Log("CagedSpider morreu");
        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f); // espera a animação de morte terminar
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            anim.SetTrigger("Attack");
        }
    }

    protected override bool CanMove()
    {
        return false;
    }
}