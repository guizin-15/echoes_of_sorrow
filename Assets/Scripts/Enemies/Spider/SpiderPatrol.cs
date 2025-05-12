using UnityEngine;

public class SpiderPatrol : EnemyBase
{
    [Header("Ataque de Projétil")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f;
    public float projectileCooldown = 1.5f;
    private float projectileCooldownTimer = 0f;
    private bool isAttacking = false; // Variable to track if the spider is attacking

    protected override void Update()
    {
        base.Update();

        if (isDead || isTakingDamage || !isGrounded) return;

        if (isPlayerDetected)
        {
            if (isAttacking)
            {
                projectileCooldownTimer -= Time.deltaTime;

                if (projectileCooldownTimer <= 0f)
                {
                    ShootProjectile();
                    projectileCooldownTimer = projectileCooldown;
                }
            }
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

    protected override bool CanMove()
    {
        return !isPlayerDetected;
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (projectileSpawnPoint != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
        }
    }
}