using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Objetos visuais")]
    [SerializeField] private GameObject leftOrnament;
    [SerializeField] private GameObject rightOrnament;

    [Header("Som")]
    [SerializeField] private AudioClip hoverClip;   // arraste seu WAV/MP3 aqui
    [SerializeField] private float    volume = 1f;  // 0-1

    private AudioSource _source;
    private bool _hovered;                          // evita spam se o ponteiro ficar oscilando

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.outputAudioMixerGroup = null;       // ou o canal do seu mixer (“UI”)
    }

    /* ----- Ponteiro entrou no botão ----- */
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hovered) return;                       // garante um play por entrada
        _hovered = true;

        leftOrnament?.SetActive(true);
        rightOrnament?.SetActive(true);

        if (hoverClip) _source.PlayOneShot(hoverClip, volume);
    }

    /* ----- Ponteiro saiu ----- */
    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;

        leftOrnament?.SetActive(false);
        rightOrnament?.SetActive(false);
    }
}