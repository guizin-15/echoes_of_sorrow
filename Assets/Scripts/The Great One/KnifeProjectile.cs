using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class KnifeProjectile : MonoBehaviour
{
    public enum Group { High, Low }
    [Header("Grupo (High / Low)")] public Group group = Group.High;

    [Header("Queda")]
    [SerializeField] float     fallSpeed  = 30f;
    [SerializeField] LayerMask groundMask;

    // cache ----------------------------------------------------------
    Rigidbody2D rb;
    Collider2D  col;

    /* referência ao Boss (pai original) e posição LOCAL de “repouso” */
    Transform   originalParent;
    Vector3     localStartPos;
    Quaternion  localStartRot;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        originalParent = transform.parent;   // boss
        localStartPos  = transform.localPosition;
        localStartRot  = transform.localRotation;

        ResetProjectile();
    }

    /*=========================== API ================================*/
    public void Launch()
    {
        // 1) solta do pai para não herdar movimento do boss
        transform.parent = null;

        // 2) torna-se “viva”
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        tag              = "EnemyAttack";

        col.enabled      = true;
        rb.bodyType      = RigidbodyType2D.Dynamic;
        rb.linearVelocity      = transform.up * fallSpeed;   // segue rotação local
    }

    public void ResetProjectile()
    {
        rb.linearVelocity  = Vector2.zero;
        rb.bodyType  = RigidbodyType2D.Kinematic;

        // volta a ser filho do boss e restaura posição local
        transform.parent        = originalParent;
        transform.localPosition = localStartPos;
        transform.localRotation = localStartRot;

        col.enabled    = false;
        gameObject.layer = LayerMask.NameToLayer("Dead");
        tag            = "Untagged";
    }

    /*==================== colisão com o chão ========================*/
    void OnCollisionEnter2D(Collision2D c)
    {
        if (((1 << c.collider.gameObject.layer) & groundMask) != 0)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;   // crava no solo
        }
    }
}