using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioSystem : MonoBehaviour
{
    private PlayerController2D playerController;
    private AudioSource audioSource;
    private AudioSource loopAudioSource; // Para sons em loop

    [Header("Sons de Movimentação")]
    [SerializeField] private AudioClip footstepSound;
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
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float combatVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float feedbackVolume = 1f;
    [SerializeField] private float footstepRate = 0.3f; // Tempo entre footsteps

    // Estados de controle
    private bool isWalking = false;
    private bool wasGrounded = true;
    private bool isWallSliding = false;
    private bool wasWallSliding = false;
    private float lastFootstepTime = 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Criando uma fonte de áudio dedicada para sons em loop
        GameObject loopAudioObj = new GameObject("LoopAudioSource");
        loopAudioObj.transform.parent = transform;
        loopAudioObj.transform.localPosition = Vector3.zero;
        loopAudioSource = loopAudioObj.AddComponent<AudioSource>();
        loopAudioSource.spatialBlend = 0f; // Configuração padrão para som 2D
        loopAudioSource.loop = true;
        loopAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Não tocar sons se estiver morto
        if (playerController.isDead) return;

        CheckGroundedState();
        CheckWallSlideState();
        CheckWalking();
    }
    
    private void CheckGroundedState()
    {
        bool isGrounded = playerController.isGrounded;
        
        // Detecta quando o personagem aterrissa
        if (isGrounded && !wasGrounded)
        {
            PlaySound(landSound, feedbackVolume);
        }
        
        wasGrounded = isGrounded;
    }
    
    private void CheckWallSlideState()
    {
        isWallSliding = playerController.isWallSliding;
        
        // Inicia ou para o som de wallslide
        if (isWallSliding && !wasWallSliding)
        {
            PlayLoopSound(wallSlideSound, footstepVolume);
        }
        else if (!isWallSliding && wasWallSliding)
        {
            StopLoopSound();
        }
        
        wasWallSliding = isWallSliding;
    }
    
    private void CheckWalking()
    {
        // Detecta se está andando (no chão e com velocidade)
        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        bool moving = Mathf.Abs(rb.linearVelocity.x) > 0.5f;
        bool shouldWalk = playerController.isGrounded && moving && !playerController.isSliceFrozen;
        
        // Inicia ou para o som de passos
        if (shouldWalk)
        {
            if (Time.time >= lastFootstepTime + footstepRate)
            {
                PlaySound(footstepSound, footstepVolume);
                lastFootstepTime = Time.time;
            }
        }
    }

    // Tocar um som único
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    // Iniciar um som em loop
    private void PlayLoopSound(AudioClip clip, float volume)
    {
        if (clip != null && loopAudioSource.clip != clip)
        {
            loopAudioSource.clip = clip;
            loopAudioSource.volume = volume;
            loopAudioSource.Play();
        }
    }
    
    // Parar som em loop
    private void StopLoopSound()
    {
        loopAudioSource.Stop();
    }

    // Métodos para serem chamados via eventos de animação ou pelo PlayerController2D
    public void PlayJumpSound()
    {
        PlaySound(jumpSound, feedbackVolume);
    }
    
    public void PlaySliceSound()
    {
        PlaySound(sliceAttackSound, combatVolume);
    }
    
    public void PlaySlashSound()
    {
        PlaySound(slashAttackSound, combatVolume);
    }
    
    public void PlayDashSound()
    {
        PlaySound(dashSound, feedbackVolume);
    }
    
    public void PlayDamageSound()
    {
        PlaySound(damageSound, feedbackVolume);
    }
    
    public void PlayDeathSound()
    {
        PlaySound(deathSound, feedbackVolume);
        StopLoopSound(); // Parar quaisquer sons em loop
    }
}