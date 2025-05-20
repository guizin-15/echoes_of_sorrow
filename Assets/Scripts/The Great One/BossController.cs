/*  BossController.cs – v1.12  (Hit-boxes fixados)
 *  • Mantém toda a API que a BossAI já usa.
 *  • Gera dano no Player quando as caixas estão ligadas.
 *  • Hit-boxes ficam visíveis no Scene View.
 *  • Use Animation Events para ligar/desligar cada caixa.
 *  • Corrigido problema com hitboxes que ficavam ativas mesmo após dano.
 */
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class BossController : MonoBehaviour
{
    /*──────────────── Movimento (ocioso) ─────────────*/
    [Header("Movimentação (Idle)")]
    [SerializeField] float moveSpeedIdle = 0f;      // 0 = parado

    /*──────────────── Vida / UI ──────────────────────*/
    [Header("Vida")]
    [SerializeField] int maxHealth = 100;
    int currentHealth;

    [Header("UI")]
    [SerializeField] HealthBar bossHealthBar;

    /*──────────────── Hit-boxes dos ataques ──────────*/
    [Header("Attack hit-boxes  (arraste caixas com IsTrigger)")]

    [Header("Stationary hit-boxes")]
    [SerializeField] Collider2D idleHitboxLow;   // baixo
    [SerializeField] Collider2D idleHitboxHigh;  // alto

    //  ── Hit-boxes de ataque em movimento ─────────────────────

    [Tooltip("MoveAttack (durante a corrida)")]
    [SerializeField] Collider2D moveHitbox;

    /*──────────────── Alcances (gizmos) ──────────────*/
    [Header("Alcances de IA (visuais)")]
    public float rangeMid  = 6f;
    public float rangeLong = 12f;

    /*──────── Air-Slam projectiles ───────*/
/*──────── Air-Slam knives (já existentes na cena) ───────*/
    [Header("Air-Slam Knives (arraste cada faca)")]
    [SerializeField] KnifeProjectile[] knivesHigh;   // duas facas do "primeiro" disparo
    [SerializeField] KnifeProjectile[] knivesLow;    // duas facas do "segundo" disparo

      /*──────────────── Audio ──────────────────────────*/
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Som do ataque estacionário")]
    [SerializeField] private AudioClip stationaryAttackClip;
    [Tooltip("Som do ataque em movimento")]
    [SerializeField] private AudioClip moveAttackClip;
    [Tooltip("Som do air‑slam")]
    [SerializeField] private AudioClip slamAttackClip;
    [SerializeField, Range(0f,1f)] private float attackVolume = 1f;

    [Tooltip("Som ao tomar dano")]
    [SerializeField] private AudioClip damageTakenClip;
    [SerializeField, Range(0f,1f)] private float damageTakenVolume = 1f;

    /*──────────────── Referências & estado ───────────*/
    Animator       anim;
    Rigidbody2D    rb;
    SpriteRenderer sr;

    bool  isMoving, isInvisible, isDead;
    float baseScaleX;
    
    // NOVO: Flag para controlar se está em processo de vanish/appear
    bool isVanishing = false;

    public System.Action        OnDeath;            // BossAI assina
    public System.Action<int>   OnHit;              // vida restante

    [HideInInspector] public bool overrideMovement = false;
    public  bool  IsDead => isDead;

    /*============================================================*/
    #region Unity setup
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        sr   = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        baseScaleX     = Mathf.Abs(transform.localScale.x);
        currentHealth  = maxHealth;

        if (bossHealthBar) bossHealthBar.SetMaxHealth(maxHealth);

        /* garante que todas as caixas começam desligadas/tagueadas */

        InitHitbox(idleHitboxLow);
        InitHitbox(idleHitboxHigh);
        InitHitbox(moveHitbox);
    }

    void InitHitbox(Collider2D c)
    {
        if (!c) return;
        c.enabled = false;
        c.isTrigger = true;
        c.gameObject.tag = "EnemyAttack";
    }
    #endregion
    /*============================================================*/

    void Update()
    {
        if (isDead) return;
        HandleMoveState();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (!overrideMovement)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    /*──────────────── Animator Move / Stop ──────────*/
    void HandleMoveState()
    {
        bool wantsMove = !overrideMovement &&
                         Mathf.Abs(rb.linearVelocity.x) > 0.01f;

        if (!isMoving && wantsMove)
        {
            anim.SetTrigger("StartMoving");
            anim.SetBool   ("IsMoving", true);
            isMoving = true;
        }
        else if (isMoving && !wantsMove)
        {
            anim.ResetTrigger("StartMoving");
            anim.SetBool   ("IsMoving", false);
            isMoving = false;
        }
    }

    /*──────────────── API pública para AI ───────────*/
    public void SetLinearVelocity(Vector2 v) => rb.linearVelocity = v;

    public void FlipTowards(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        Vector3 s = transform.localScale;
        s.x = baseScaleX * Mathf.Sign(dirX);
        transform.localScale = s;
    }

    public void Attack()                      // Idle ou Move
    {
        // Garante que as hitboxes estão desligadas antes de começar um novo ataque
        ForceOffAllAttackColliders();
        
        bool movingNow = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        anim.SetTrigger(movingNow ? "MoveAttackPrep" : "StationaryAttack");

        // toca o som apropriado
        AudioClip clip = movingNow ? moveAttackClip : stationaryAttackClip;
        if (clip != null)
            audioSource.PlayOneShot(clip, attackVolume);
    }

    public void TriggerSlam()                // AirAttackSlam
    {
        if (isInvisible || isDead) return;
        
        // Garante que as hitboxes estão desligadas antes de começar um novo ataque
        ForceOffAllAttackColliders();
        
        anim.SetTrigger("AirAttackSlam");

        if (slamAttackClip != null)
            audioSource.PlayOneShot(slamAttackClip, attackVolume);
    }

    /*──────────────── Vanish / Appear ───────────────*/
    public void BeginVanish()
    {
        if (isInvisible || isDead) return;
        
        // Força desligar todas as hitboxes quando começa a desaparecer
        ForceOffAllAttackColliders();
        
        isInvisible = true;
        isVanishing = true;
        anim.SetBool("IsInvisible", true);
        anim.SetTrigger("Vanish");
        sr.enabled = false;
        rb.linearVelocity = Vector2.zero;
        SetAllBodyColliders(false);          // evita colisão c/ player
    }
    
    public void EndVanish()
    {
        if (!isInvisible || isDead) return;
        
        isInvisible = false;
        isVanishing = false;
        anim.SetBool("IsInvisible", false);
        anim.SetTrigger("Appear");
        sr.enabled = true;
        SetAllBodyColliders(true);
        
        // Reinicia o controle de movimento
        if (!overrideMovement) {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void SetAllBodyColliders(bool on)
    {
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = on;
    }

    /*──────────────── Vida / Dano / Morte ───────────*/

    public void ForceOffAllAttackColliders()
    {
        // Desliga todos os hitboxes de ataque
        if (idleHitboxLow)  idleHitboxLow.enabled  = false;
        if (idleHitboxHigh) idleHitboxHigh.enabled = false;
        if (moveHitbox)     moveHitbox.enabled     = false;

        // Reseta as facas projeteis
        ResetKnives(knivesHigh);
        ResetKnives(knivesLow);
    }
    
    public void TakeDamage()                 // chamado pela IA
    {
        if (isDead) return;

        // som de dano
        if (damageTakenClip != null)
            audioSource.PlayOneShot(damageTakenClip, damageTakenVolume);

        currentHealth = Mathf.Max(currentHealth - 1, 0);
        anim.SetTrigger("Hit");

        /* 1) Desliga QUALQUER hit-box ativa se apanhar no meio do golpe */
        ForceOffAllAttackColliders();

        if (bossHealthBar) bossHealthBar.SetHealth(currentHealth);
        OnHit?.Invoke(currentHealth);

        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Garante que todas as hitboxes estão desligadas ao morrer
        ForceOffAllAttackColliders();
        
        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Kinematic;
        gameObject.layer  = LayerMask.NameToLayer("Dead");

        /* desliga hit-boxes e colisores */
        if (idleHitboxLow)  idleHitboxLow.gameObject.SetActive(false);
        if (idleHitboxHigh) idleHitboxHigh.gameObject.SetActive(false);
        if (moveHitbox)     moveHitbox.gameObject.SetActive(false);
        SetAllBodyColliders(false);

        OnDeath?.Invoke();
        this.enabled = false;     // impede novas chamadas
    }

    /*──────────────── HIT-BOX helpers (Animation Event) ─────────*/
    //  These methods are called from animation key-frames.
    public void HB_IdleLow_On()  { if (!isVanishing && !isDead && idleHitboxLow)  idleHitboxLow.enabled  = true; }
    public void HB_IdleLow_Off() { if (idleHitboxLow)  idleHitboxLow.enabled  = false; }

    public void HB_IdleHigh_On() { if (!isVanishing && !isDead && idleHitboxHigh) idleHitboxHigh.enabled = true; }
    public void HB_IdleHigh_Off(){ if (idleHitboxHigh) idleHitboxHigh.enabled = false; }

    public void HB_Move_On()  { if (!isVanishing && !isDead && moveHitbox)  moveHitbox.enabled  = true; }
    public void HB_Move_Off() { if (moveHitbox)  moveHitbox.enabled  = false; }
    
    /* Slam alto ─ dispara o 1.º par */
    public void SlamHigh_On()   { if (!isVanishing && !isDead) FireKnives(knivesHigh); }
    public void SlamHigh_Off()  { ResetKnives(knivesHigh); }
    /* Slam baixo ─ dispara o 2.º par */
    public void SlamLow_On()    { if (!isVanishing && !isDead) FireKnives(knivesLow);  }
    public void SlamLow_Off()   { ResetKnives(knivesLow); }

    void FireKnives(KnifeProjectile[] arr)
    {
        foreach (var k in arr) k?.Launch();
    }
    
    void ResetKnives(KnifeProjectile[] arr)
    {
        foreach (var k in arr) k?.ResetProjectile();
    }

    /*──────────────── Trigger → dano no player ─────────────────*/
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isDead || isVanishing) return;

        // o hit-box ativo aplica dano
        if (idleHitboxLow && idleHitboxLow.enabled && other.IsTouching(idleHitboxLow)) HitPlayer(other, 10);
        if (idleHitboxHigh && idleHitboxHigh.enabled && other.IsTouching(idleHitboxHigh)) HitPlayer(other, 10);
        if (moveHitbox && moveHitbox.enabled && other.IsTouching(moveHitbox)) HitPlayer(other, 10);
    }

    void HitPlayer(Collider2D playerCol, int damage)
    {
        playerCol.SendMessage("TakeDamage", damage,
                            SendMessageOptions.DontRequireReceiver);
    }

    /*──────────────── Gizmos – hit-boxes ───────────────────────*/
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; DrawRange(rangeMid);
        Gizmos.color = Color.cyan;   DrawRange(rangeLong);

        DrawHitbox(idleHitboxLow , Color.red);
        DrawHitbox(idleHitboxHigh, Color.red);
        DrawHitbox(moveHitbox , new Color(1f, .3f, 0f));
    }
    void DrawRange(float r)
    {
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, r);
    }
    void DrawHitbox(Collider2D box, Color c)
    {
        if (!box) return;
        Gizmos.color = c;
        Gizmos.matrix = box.transform.localToWorldMatrix;
        if (box is BoxCollider2D boxCol)
        {
            Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
        }
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}