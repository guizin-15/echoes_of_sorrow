using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    // Singleton
    public static MobileInputManager I { get; private set; }

    // Flags de input
    [HideInInspector] public bool left;
    [HideInInspector] public bool right;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool attack;
    [HideInInspector] public bool dash;

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Métodos para ligar/desligar as flags
    public void SetLeft(bool v)   { left   = v; }
    public void SetRight(bool v)  { right  = v; }
    public void SetJump(bool v)   { jump   = v; }
    public void SetAttack(bool v) { attack = v; }
    public void SetDash(bool v) { dash = v; }
}
