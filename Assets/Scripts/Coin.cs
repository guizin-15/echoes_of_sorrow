using UnityEngine;

public class Coin : MonoBehaviour
{
    private Transform player;
    private Rigidbody2D rb;
    private bool isAttracted = false;

    [Header("Attraction Settings")]
    public float attractionSpeed = 8f;    // Velocidade de atração
    public float attractionRadius = 2f;   // Raio de ativação da atração
    public float attractionDelay = 5f;    // Tempo antes de permitir a atração
    private float collectionDelay = 0.5f; // Delay antes de poder ser coletada
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnTime = Time.time;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Verifica se o tempo de delay para atração já passou
        if (!isAttracted && Time.time >= spawnTime + attractionDelay && distanceToPlayer <= attractionRadius)
        {
            isAttracted = true;
            rb.gravityScale = 0f; // Desliga a gravidade só quando começa a ser atraída
        }

        if (isAttracted)
        {
            Vector2 direction = (GetPlayerCenter() - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * attractionSpeed;
        }
    }

    private Vector2 GetPlayerCenter()
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        return playerCollider ? playerCollider.bounds.center : (Vector2)player.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time >= spawnTime + collectionDelay)
            {
                PlayerController pc = collision.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.coinsCollected++;

                    SoundManager.Instance.PlaySound(SoundManager.Instance.coinCollectClip);
                }
                Destroy(gameObject);
            }
        }
    }
}