using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioClip coinCollectClip;
    public AudioClip attackClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
