using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BossController))]
public class BossAI : MonoBehaviour
{
    /*──────────────── refs ─────────────*/
    [Header("Referências")]
    [SerializeField] Transform   player;
    BossController               ctrl;
    Collider2D[]                 colliders;

    /*──────────────── vida / fase ──────*/
    [Header("Vida")]
    [SerializeField] int maxLife = 100;
    int  life;
    enum Phase { P1, P2, Dead }
    Phase phase = Phase.P1;

    /*──────────────── ranges ───────────*/
    [Header("Ranges horizontais")]
    [SerializeField] float rangeMid  = 6f;
    [SerializeField] float rangeLong = 12f;

    /*──────────────── timings ─────────*/
    [Header("Timings base (Phase-1)")]
    [SerializeField] float attackDur   = 1.2f;
    [SerializeField] float moveAtkDur  = 1.2f;
    [SerializeField] float globalCD    = 2f;
    [SerializeField] float vanishDelay = .15f;
    [SerializeField] float reappearLag = .30f;

    [Header("Multiplicadores Phase-2")]
    [SerializeField] float speedMul = 1.25f;
    [SerializeField] float timeMul  = 1f;
    [SerializeField] float cdMulP2    = 0.75f;   // ← NOVO: só p/ cooldown

    /*──────────────── movimento ───────*/
    const float walkSpeed = 5f;
    const float stopDist  = 0.2f;

    /*──────── defensive teleport ──────*/
    [Header("Defensive teleport")]
    [SerializeField] float     defensiveOffset = 8f;      // distância alvo
    [SerializeField] LayerMask obstacleMask;              // paredes / limites
    [SerializeField] float     checkRadius = 0.5f;        // NOVO: raio para checagem de colisão
    [SerializeField] float     checkHeight = 0f;          // NOVO: altura para checagem (y=0 por padrão)
    
    [Header("Visualização de Debug")]
    [SerializeField] bool showTeleportChecks = true;      // Controla se os raios de debug devem ser mostrados
    [SerializeField] float debugDrawDuration = 2f;        // Duração dos desenhos de debug
    [SerializeField] Color freePositionColor = Color.green;   // Cor para posições livres
    [SerializeField] Color blockedPositionColor = Color.red;  // Cor para posições bloqueadas
    
    // Armazena pontos de verificação de teleporte para visualização
    private List<TeleportCheckPoint> teleportCheckPoints = new List<TeleportCheckPoint>();

    /*──────────────── internals ───────*/
    bool  actionLock;
    bool  isDoingDefensiveTeleport = false; // FLAG NOVA: controla state do TP defensivo
    Coroutine currentActionCoroutine; // NOVO: referência para coroutine atual
    enum  Combo { None, Idle, Move, Teleport }
    Combo lastCombo = Combo.None;
    
    // Classe para armazenar informações sobre os pontos de verificação para teleporte
    private class TeleportCheckPoint
    {
        public Vector3 position;
        public bool isFree;
        public float radius;
        public float timeRemaining;
        
        public TeleportCheckPoint(Vector3 pos, bool free, float rad, float duration)
        {
            position = pos;
            isFree = free;
            radius = rad;
            timeRemaining = duration;
        }
    }

    /*============================================================*/
    #region Unity
    void Awake()
    {
        ctrl = GetComponent<BossController>();
        colliders = GetComponents<Collider2D>();

        life = maxLife;
        ctrl.rangeMid = rangeMid;
        ctrl.rangeLong = rangeLong;
        
        ctrl.OnHit   += HandleHit;
        ctrl.OnDeath += KillBoss;
    }

    void Start()  => StartCoroutine(MainLoop());

    void Update()
    {
        // Atualiza e desenha os pontos de verificação de teleporte
        if (showTeleportChecks)
        {
            UpdateDebugVisuals();
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (phase == Phase.Dead) return;
        if (c.collider.CompareTag("Player") && !isDoingDefensiveTeleport)
            StartDefensiveTeleport();
    }
    
    // Desenha os pontos de verificação no editor
    void OnDrawGizmos()
    {
        if (!showTeleportChecks || !Application.isPlaying) return;
        
        foreach (var point in teleportCheckPoints)
        {
            Gizmos.color = point.isFree ? freePositionColor : blockedPositionColor;
            Gizmos.DrawWireSphere(point.position, point.radius);
        }
    }
    #endregion
    /*============================================================*/

    /*──────────────── LOOP principal ─────────────*/
    IEnumerator MainLoop()
    {
        while (phase != Phase.Dead)
        {
            while (actionLock || isDoingDefensiveTeleport) yield return null;

            Combo c = PickComboDistinct();
            lastCombo = c;

            Coroutine comboCoroutine = null;

            switch (c)
            {
                case Combo.Idle:     comboCoroutine = StartCoroutine(IdleAttack());    break;
                case Combo.Move:     comboCoroutine = StartCoroutine(MoveAttack());    break;
                case Combo.Teleport: comboCoroutine = StartCoroutine(TeleportCombo()); break;
            }
            
            // Armazena referência para poder interromper depois
            currentActionCoroutine = comboCoroutine;

            // Aguarda que a ação seja concluída ou interrompida
            while (actionLock && !isDoingDefensiveTeleport) yield return null;
            
            // Se não estiver em teleporte defensivo, continua com o fluxo normal
            if (!isDoingDefensiveTeleport && phase != Phase.Dead)
            {
                if (phase == Phase.P2)                    // Slam extra
                {
                    ctrl.TriggerSlam();
                    yield return new WaitForSeconds(attackDur * timeMul);
                }

                yield return new WaitForSeconds(globalCD *
                    (phase == Phase.P2 ? cdMulP2 : 1f));
            }
        }
    }

    /*──────────────── escolha de combo ───────────*/
    Combo PickComboDistinct()
    {
        float dist = HDist();
        Combo chosen;

        if (dist > rangeMid)    // LONG 50 % : 50 %
            chosen = (Random.value < .5f) ? Combo.Teleport : Combo.Move;
        else                    // MID  50 % : 50 %
            chosen = (Random.value < .5f) ? Combo.Idle     : Combo.Move;

        /* evita repetir */
        if (chosen == lastCombo)
            chosen = (chosen == Combo.Idle) ? Combo.Move
                   : (chosen == Combo.Move) ? Combo.Idle
                   : (chosen == Combo.Teleport ? Combo.Move : Combo.Teleport);

        return chosen;
    }

    /*──────────────── combos ─────────────────────*/
    IEnumerator IdleAttack()
    {
        actionLock = true;
        ctrl.overrideMovement = true;

        ctrl.Attack();
        yield return new WaitForSeconds(attackDur *
                   (phase == Phase.P2 ? cdMulP2 : 1f));

        ctrl.overrideMovement = false;
        actionLock = false;
    }

    IEnumerator MoveAttack()
    {
        actionLock = true;

        float targetX = player.position.x;
        float dir     = Mathf.Sign(targetX - transform.position.x);
        if (dir == 0) dir = 1f;
        ctrl.FlipTowards(dir);

        ctrl.overrideMovement = true;
        ctrl.SetLinearVelocity(new Vector2(dir * walkSpeed *
                   (phase == Phase.P2 ? speedMul : 1f), 0f));
        yield return null;               // garante 1 frame andando

        ctrl.Attack();                   // animação "MoveAttack"

        float maxT = moveAtkDur * (phase == Phase.P2 ? timeMul : 1f);
        float t = 0f;
        while (t < maxT && Mathf.Abs(transform.position.x - targetX) > stopDist)
        {
            t += Time.deltaTime;
            yield return null;
        }

        ctrl.SetLinearVelocity(Vector2.zero);
        ctrl.overrideMovement = false;
        actionLock = false;
    }

    IEnumerator TeleportCombo()
    {
        actionLock = true;
        ctrl.overrideMovement = true;

        /* Invisível ------------------------------------------------------ */
        BeginGhost();                 // desliga colisão + vanish
        yield return new WaitForSeconds(vanishDelay);

        /* Anda invisível até posição travada ----------------------------- */
        float targetX = player.position.x;
        float dir     = Mathf.Sign(targetX - transform.position.x);
        if (dir == 0) dir = 1f;
        ctrl.FlipTowards(dir);

        while (Mathf.Abs(transform.position.x - targetX) > stopDist)
        {
            ctrl.SetLinearVelocity(new Vector2(dir * walkSpeed *
                   (phase == Phase.P2 ? speedMul : 1f), 0f));
            yield return null;
        }
        ctrl.SetLinearVelocity(Vector2.zero);

        /* Reaparece + ataque parado -------------------------------------- */
        EndGhost();
        yield return new WaitForSeconds(reappearLag);

        ctrl.Attack();                          // Idle variant
        yield return new WaitForSeconds(attackDur *
                   (phase == Phase.P2 ? timeMul : 1f));

        ctrl.overrideMovement = false;
        actionLock = false;
    }

    /*──────────────── dano & TP defensivo ───────────────────────*/
    void HandleHit(int hpLeft)
    {
        if (phase == Phase.Dead) return;

        if (hpLeft <= 25 && phase == Phase.P1)
        {
            phase = Phase.P2;
            Debug.Log("<color=orange>=== PHASE 2 ACTIVATED ===</color>");
        }

        // CORREÇÃO: Sempre que tomar dano, deve fazer o teleporte defensivo
        // Se não estiver já em um teleporte defensivo, inicia um
        if (!isDoingDefensiveTeleport) 
        {
            StartDefensiveTeleport();
        }
    }

    void KillBoss()
    {
        Debug.Log("<color=red>=== BOSS KILLED ===</color>");
        phase = Phase.Dead;
        StopAllCoroutines();
        ctrl.overrideMovement = true; // garante que nunca mais se mova

        // Aguarda um tempo antes de trocar de cena
        StartCoroutine(LoadCreditsAfterDelay(2f));
    }

    IEnumerator LoadCreditsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Credits");
    }

    // MÉTODO NOVO: centraliza a lógica de iniciar o teleporte defensivo
    void StartDefensiveTeleport()
    {
        // Se já estava em uma ação, interrompe
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        
        // Limpa qualquer estado anterior para garantir
        ctrl.ForceOffAllAttackColliders();
        
        // Inicia o novo teleporte defensivo
        currentActionCoroutine = StartCoroutine(DefensiveTeleport());
    }

    IEnumerator DefensiveTeleport()
    {
        // Marca que está em teleporte defensivo para evitar interrupções
        isDoingDefensiveTeleport = true;
        actionLock = true;
        ctrl.overrideMovement = true;
        Debug.Log("<color=blue>=== DEFENSIVE TELEPORT ===</color>");

        // Força desligar todas as hitboxes de ataque para garantir segurança
        ctrl.ForceOffAllAttackColliders();
        
        // Primeiro espera um curto período para a animação de dano (congelamento)
        yield return new WaitForSeconds(0.25f);
        
        // Fica invisível (vanish)
        BeginGhost();
        yield return new WaitForSeconds(vanishDelay);

        // NOVO: Visualização mais clara do ponto inicial do teleporte
        if (showTeleportChecks)
        {
            Debug.DrawRay(transform.position, Vector3.up * 2f, Color.blue, debugDrawDuration);
            Debug.DrawRay(transform.position, Vector3.down * 2f, Color.blue, debugDrawDuration);
        }

        // Calcula a posição segura em Y=0 mas mantém o Y original do boss para movimento
        Vector3 safePos = GetSafeTeleportPosition();
        safePos.y = transform.position.y;
        Debug.Log($"<color=yellow>Movendo para posição segura: {safePos}</color>");

        // Determina a direção e velocidade do movimento
        float dir = Mathf.Sign(safePos.x - transform.position.x);
        if (dir == 0) dir = 1f;
        ctrl.FlipTowards(dir);
        
        // Move o boss enquanto invisível até o ponto seguro (igual ao vanish ofensivo)
        float speed = walkSpeed * (phase == Phase.P2 ? speedMul : 1f);
        while (Mathf.Abs(transform.position.x - safePos.x) > stopDist)
        {
            ctrl.SetLinearVelocity(new Vector2(dir * speed, 0f));
            yield return null;
        }
        
        // Ao chegar ao destino, para o movimento
        ctrl.SetLinearVelocity(Vector2.zero);
        
        // NOVO: Visualização mais clara do ponto final do teleporte
        if (showTeleportChecks)
        {
            Debug.DrawRay(transform.position, Vector3.up * 2f, Color.green, debugDrawDuration);
            Debug.DrawRay(transform.position, Vector3.down * 2f, Color.green, debugDrawDuration);
        }

        // Espera um curto período antes de reaparecer
        yield return new WaitForSeconds(reappearLag);
        
        // Torna visível novamente
        EndGhost();

        // Período de recuperação após o vanish defensivo
        yield return new WaitForSeconds(1.0f);

        // Limpa flags no fim do teleporte
        ctrl.overrideMovement = false;
        actionLock = false;
        isDoingDefensiveTeleport = false;
    }

    /*───────────────── NOVO: busca lado seguro ─────────────────*/
    Vector3 GetSafeTeleportPosition()
    {
        // Limpa os pontos de verificação anteriores
        if (showTeleportChecks)
        {
            teleportCheckPoints.Clear();
        }
        
        float half = defensiveOffset;

        // CORREÇÃO PRINCIPAL: Usa a altura definida (y=0 por padrão) para verificação
        // mas manterá a posição Y original ao mover o boss
        float checkY = checkHeight;

        // Tenta lado esquerdo e direito com o Y ajustado para a checagem
        Vector3 left = new Vector3(player.position.x - half, checkY);
        Vector3 right = new Vector3(player.position.x + half, checkY);

        // Usa CircleCast para verificar melhor o espaço livre na altura de checagem
        bool leftFree = !Physics2D.CircleCast(left, checkRadius, Vector2.zero, 0.1f, obstacleMask);
        bool rightFree = !Physics2D.CircleCast(right, checkRadius, Vector2.zero, 0.1f, obstacleMask);

        // Adiciona os pontos de verificação para visualização
        if (showTeleportChecks)
        {
            teleportCheckPoints.Add(new TeleportCheckPoint(left, leftFree, checkRadius, debugDrawDuration));
            teleportCheckPoints.Add(new TeleportCheckPoint(right, rightFree, checkRadius, debugDrawDuration));
            
            // Também desenha linhas na cena para visualização imediata
            Debug.DrawLine(transform.position, left, leftFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
            Debug.DrawLine(transform.position, right, rightFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
        }

        Debug.Log($"Checagem de espaço em y={checkY} - Esquerda: {leftFree}, Direita: {rightFree}");

        if (leftFree && rightFree) // ambos livres → escolhe lado oposto ao boss
        {
            Vector3 chosen = (transform.position.x < player.position.x) ? left : right;
            Debug.Log($"[DefensiveTeleport] Ambos os lados livres. Escolhido: {(chosen == left ? "esquerdo" : "direito")} X={chosen.x}");
            
            // Marca o ponto escolhido com uma cor especial
            if (showTeleportChecks)
            {
                Color highlightColor = new Color(0f, 1f, 1f); // Ciano para destacar
                Debug.DrawLine(transform.position, chosen, highlightColor, debugDrawDuration);
                Debug.DrawRay(chosen, Vector3.up * 2f, highlightColor, debugDrawDuration);
            }
            
            return chosen;
        }

        if (rightFree)
        {
            Debug.Log($"[DefensiveTeleport] Apenas lado direito livre: X={right.x}");
            if (showTeleportChecks)
            {
                Debug.DrawRay(right, Vector3.up * 2f, Color.cyan, debugDrawDuration);
            }
            return right;
        }
        
        if (leftFree)
        {
            Debug.Log($"[DefensiveTeleport] Apenas lado esquerdo livre: X={left.x}");
            if (showTeleportChecks)
            {
                Debug.DrawRay(left, Vector3.up * 2f, Color.cyan, debugDrawDuration);
            }
            return left;
        }

        // Se chegou aqui, nenhum lado estava livre com o offset original
        // Tenta distâncias menores em incrementos graduais
        for (float d = half * 0.75f; d >= 2.0f; d *= 0.75f)
        {
            left = new Vector3(player.position.x - d, checkY);
            right = new Vector3(player.position.x + d, checkY);

            leftFree = !Physics2D.CircleCast(left, checkRadius, Vector2.zero, 0.1f, obstacleMask);
            rightFree = !Physics2D.CircleCast(right, checkRadius, Vector2.zero, 0.1f, obstacleMask);
            
            // Adiciona estes pontos de verificação também
            if (showTeleportChecks)
            {
                teleportCheckPoints.Add(new TeleportCheckPoint(left, leftFree, checkRadius, debugDrawDuration));
                teleportCheckPoints.Add(new TeleportCheckPoint(right, rightFree, checkRadius, debugDrawDuration));
                
                Debug.DrawLine(transform.position, left, leftFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
                Debug.DrawLine(transform.position, right, rightFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
            }

            if (rightFree)
            {
                Debug.Log($"[DefensiveTeleport] Offset reduzido, lado direito livre em d={d}: X={right.x}");
                if (showTeleportChecks)
                {
                    Debug.DrawRay(right, Vector3.up * 2f, Color.cyan, debugDrawDuration);
                }
                return right;
            }
            
            if (leftFree)
            {
                Debug.Log($"[DefensiveTeleport] Offset reduzido, lado esquerdo livre em d={d}: X={left.x}");
                if (showTeleportChecks)
                {
                    Debug.DrawRay(left, Vector3.up * 2f, Color.cyan, debugDrawDuration);
                }
                return left;
            }
        }

        // ÚLTIMA ALTERNATIVA: Tenta um teleporte para um local bem distante do jogador
        Vector3 farPosition = new Vector3(transform.position.x + ((transform.position.x < player.position.x ? -1 : 1) * 15f), checkY);
        bool farPositionFree = !Physics2D.CircleCast(farPosition, checkRadius, Vector2.zero, 0.1f, obstacleMask);
        
        if (showTeleportChecks)
        {
            teleportCheckPoints.Add(new TeleportCheckPoint(farPosition, farPositionFree, checkRadius, debugDrawDuration));
            Debug.DrawLine(transform.position, farPosition, farPositionFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
        }
        
        if (farPositionFree)
        {
            Debug.Log($"[DefensiveTeleport] Usando posição de fuga distante: X={farPosition.x}");
            if (showTeleportChecks)
            {
                Debug.DrawRay(farPosition, Vector3.up * 2f, Color.magenta, debugDrawDuration);
            }
            return farPosition;
        }

        Debug.LogWarning("Defensive teleport sem espaço livre! Ajuste o tamanho do boss ou o offset do teleporte.");
        
        // Último recurso: Tenta um teleporte curto para trás
        Vector3 slightBackup = new Vector3(transform.position.x - (Mathf.Sign(player.position.x - transform.position.x) * 2f), checkY);
        bool backupFree = !Physics2D.CircleCast(slightBackup, checkRadius, Vector2.zero, 0.1f, obstacleMask);
        
        if (showTeleportChecks)
        {
            teleportCheckPoints.Add(new TeleportCheckPoint(slightBackup, backupFree, checkRadius, debugDrawDuration));
            Debug.DrawLine(transform.position, slightBackup, backupFree ? freePositionColor : blockedPositionColor, debugDrawDuration);
            
            if (backupFree)
            {
                Debug.DrawRay(slightBackup, Vector3.up * 2f, Color.yellow, debugDrawDuration);
            }
        }
        
        Debug.Log($"[DefensiveTeleport] Tentativa de emergência: X={slightBackup.x}");
        return slightBackup;
    }

    // NOVO: método para atualizar e limpar os visuais de debug
    void UpdateDebugVisuals()
    {
        // Atualiza a duração dos pontos de verificação e remove os expirados
        for (int i = teleportCheckPoints.Count - 1; i >= 0; i--)
        {
            teleportCheckPoints[i].timeRemaining -= Time.deltaTime;
            if (teleportCheckPoints[i].timeRemaining <= 0)
            {
                teleportCheckPoints.RemoveAt(i);
            }
        }
    }

    /*──────────────── util ───────────*/
    float HDist() => Mathf.Abs(player.position.x - transform.position.x);

    /*──────── ghost helpers ──────────*/
    void BeginGhost()
    {
        ctrl.BeginVanish();
        foreach (var c in colliders) c.enabled = false;
    }
    void EndGhost()
    {
        ctrl.EndVanish();
        foreach (var c in colliders) c.enabled = true;
    }
}