using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class Coin : MonoBehaviour
{
    #region --- Inspector ---
    [Header("Attraction Settings")]
    [SerializeField] private float attractionSpeed  =  8f;   // Velocidade de atração
    [SerializeField] private float attractionRadius =  2f;   // Raio de ativação
    [SerializeField] private float collectionDelay  =  0.3f; // Tempo mínimo antes de poder coletar
    [SerializeField] private float attractionDelay  =  1f;   // Tempo para começar a atrair
    #endregion

    #region --- Internos ---
    private Transform   player;
    private Rigidbody2D rb;
    private bool  isAttracted;
    private float spawnTime;

    private CircleCollider2D physicsCol;   // sólido
    private CircleCollider2D triggerCol;   // gatilho (player)
    #endregion

    /* ---------------------------------------------------------------------- */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnTime = Time.time;

        /* ----------  Garantimos os dois colliders  ---------- */
        physicsCol = GetComponent<CircleCollider2D>();
        if (physicsCol == null) physicsCol = gameObject.AddComponent<CircleCollider2D>();
        physicsCol.isTrigger = false;                // colide com cenário

        triggerCol = gameObject.AddComponent<CircleCollider2D>();
        triggerCol.isTrigger = true;                 // detecta player
        triggerCol.radius    = physicsCol.radius*1.2f;
        Physics2D.IgnoreCollision(triggerCol, physicsCol); // evita interferência

        rb.gravityScale = 1f;                        // começa com gravidade
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation           = RigidbodyInterpolation2D.Interpolate;
    }

    /* ---------------------------------------------------------------------- */
    private void Update()
    {
        if (player == null) return;

        /* ------------ Ativação da atração ------------ */
        float dist = Vector2.Distance(transform.position, player.position);

        if (!isAttracted &&
            Time.time >= spawnTime + attractionDelay &&
            dist       <= attractionRadius)
        {
            isAttracted    = true;
            rb.gravityScale = 0f;                    // desliga gravidade
            rb.linearVelocity = Vector2.zero;          // limpa força anterior
        }

        if (isAttracted)
        {
            Vector2 dir = ((Vector2)GetPlayerCenter() - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * attractionSpeed;
        }
    }

    /* ---------------------------------------------------------------------- */
    private Vector2 GetPlayerCenter()
    {
        var col = player.GetComponent<Collider2D>();
        return col ? col.bounds.center : (Vector2)player.position;
    }

    /* ---------- Coleta (gatilho) ---------- */
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Trigger entrou com: " + col.name);

        if (!col.CompareTag("Player")) return;
        Debug.Log("É o Player!");

        if (Time.time < spawnTime + collectionDelay) return;

        if (col.TryGetComponent(out PlayerController2D pc))
        {
            pc.coinsCollected++;
            Debug.Log("Moeda coletada!");
        }

        Destroy(gameObject);
    }

}