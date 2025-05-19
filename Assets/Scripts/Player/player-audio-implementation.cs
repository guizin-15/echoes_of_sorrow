using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioSystem : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste seu PlayerController2D aqui ou deixe vazio para buscar no parent")]
    [SerializeField] private PlayerController2D playerController;

    [Header("Sons de Movimentação")]
    [SerializeField] private AudioClip footstepSound;    // usado tanto para passos únicos quanto loop
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip wallSlideSound;

    [Header("Sons de Combate")]
    [SerializeField] private AudioClip sliceAttackSound;
    [SerializeField] private AudioClip slashAttackSound;
    [SerializeField] private AudioClip dashSound;

    [Header("Sons de Feedback")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Configurações")]
    [SerializeField, Range(0f,1f)] private float footstepVolume = 0.5f;
    [SerializeField, Range(0f,1f)] private float combatVolume   = 0.7f;
    [SerializeField, Range(0f,1f)] private float feedbackVolume = 1f;
    [SerializeField] private float footstepRate = 0.3f; // tempo entre passos únicos

    // estados internos
    private AudioSource audioSource;
    private AudioSource loopAudioSource;
    private bool wasGrounded    = true;
    private bool wasWallSliding = false;
    private bool wasRunning     = false;
    private float lastFootstepTime = 0f;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController2D>();
        if (playerController == null)
            Debug.LogError($"[{name}] PlayerController2D não encontrado em {gameObject.name} nem nos pais!");

        audioSource = GetComponent<AudioSource>();

        GameObject loopObj = new GameObject("LoopAudioSource");
        loopObj.transform.SetParent(transform, false);
        loopAudioSource = loopObj.AddComponent<AudioSource>();
        loopAudioSource.spatialBlend = 0f;
        loopAudioSource.loop        = true;
        loopAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        CheckGroundedState();
        CheckWallSlideState();
        CheckWalking();
        CheckRunningLoop();
    }

    private void CheckGroundedState()
    {
        bool isGrounded = playerController.isGrounded;
        if (isGrounded && !wasGrounded)
            PlaySound(landSound, feedbackVolume);
        wasGrounded = isGrounded;
    }

    private void CheckWallSlideState()
    {
        bool slide = playerController.isWallSliding;
        if (slide && !wasWallSliding)
            PlayLoopSound(wallSlideSound, footstepVolume);
        else if (!slide && wasWallSliding)
            StopLoopSound();
        wasWallSliding = slide;
    }

    private void CheckWalking()
    {
        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        bool moving = Mathf.Abs(rb.linearVelocity.x) > 0.5f;
        bool shouldWalk = playerController.isGrounded && moving && !playerController.isSliceFrozen;

        if (shouldWalk && Time.time >= lastFootstepTime + footstepRate)
        {
            PlaySound(footstepSound, footstepVolume);
            lastFootstepTime = Time.time;
        }
    }

    private void CheckRunningLoop()
    {
        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        bool isRunning = playerController.isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        // só inicia o loop depois de um passo (evita overlap one-shot + loop no mesmo frame)
        if (isRunning && !wasRunning && Time.time >= lastFootstepTime + footstepRate)
            PlayLoopSound(footstepSound, footstepVolume);
        else if (!isRunning && wasRunning)
            StopLoopSound();

        wasRunning = isRunning;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    private void PlayLoopSound(AudioClip clip, float volume)
    {
        if (clip != null && loopAudioSource.clip != clip)
        {
            loopAudioSource.clip   = clip;
            loopAudioSource.volume = volume;
            loopAudioSource.Play();
        }
    }

    private void StopLoopSound()
    {
        loopAudioSource.Stop();
    }

    // métodos chamados de fora
    public void PlayJumpSound()
    {
        StopLoopSound();                            // corta o loop ao pular
        PlaySound(jumpSound, feedbackVolume);
    }
    public void PlaySliceSound()  => PlaySound(sliceAttackSound, combatVolume);
    public void PlaySlashSound()  => PlaySound(slashAttackSound, combatVolume);
    public void PlayDashSound()   => PlaySound(dashSound, feedbackVolume);
    public void PlayDamageSound() => PlaySound(damageSound, feedbackVolume);
    public void PlayDeathSound()
    {
        PlaySound(deathSound, feedbackVolume);
        StopLoopSound();
    }
}