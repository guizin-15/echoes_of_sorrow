using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class UIButtonFX : MonoBehaviour,
                           IPointerEnterHandler,
                           IPointerExitHandler,
                           IPointerClickHandler
{
    [Header("Objetos visuais")]
    [SerializeField] GameObject leftOrnament;
    [SerializeField] GameObject rightOrnament;

    [Header("Áudio")]
    [SerializeField] AudioClip  hoverClip;
    [SerializeField] AudioClip  clickClip;
    [Range(0,1)]    [SerializeField] float sfxVolume = 1f;

    [Header("Referências")]
    [SerializeField] CanvasGroup fadeCanvas; // mesmo objeto usado no fade-in
    [SerializeField] AudioSource bgmSource;  // música de fundo
    [SerializeField] string       sceneToLoad = ""; // vazio = não troca

    AudioSource  sfx;
    ScreenFader  fader;       // obtém durações
    bool         hovered;

    void Awake()
    {
        sfx = GetComponent<AudioSource>();
        sfx.playOnAwake  = false;
        sfx.spatialBlend = 0f;

        if (fadeCanvas) fader = fadeCanvas.GetComponent<ScreenFader>();
    }

    /* ---------------- Hover ---------------- */
    public void OnPointerEnter(PointerEventData _) { if (hovered) return;
        hovered = true;
        leftOrnament?.SetActive(true);
        rightOrnament?.SetActive(true);
        if (hoverClip) sfx.PlayOneShot(hoverClip,sfxVolume);
    }
    public void OnPointerExit (PointerEventData _)
    {
        hovered = false;
        leftOrnament?.SetActive(false);
        rightOrnament?.SetActive(false);
    }

    /* ---------------- Click ---------------- */
    public void OnPointerClick(PointerEventData ev)
    {
        if (ev.button != PointerEventData.InputButton.Left) return;
        if (clickClip) sfx.PlayOneShot(clickClip,sfxVolume);

        if (gameObject.activeInHierarchy)
            StartCoroutine(FadeAndLoad());
    }

    /* ---------------- Fade / Load ---------------- */
    IEnumerator FadeAndLoad()
    {
        float visualDur = fader ? fader.FadeDuration    : 1f;
        float audioDur  = fader ? fader.BgmFadeDuration : visualDur;
        float maxDur    = Mathf.Max(visualDur, audioDur);

        // garante painel visível
        if (fadeCanvas) fadeCanvas.blocksRaycasts = true;

        for (float t = 0; t < maxDur; t += Time.deltaTime)
        {
            /* tela */
            if (fadeCanvas)
                fadeCanvas.alpha = Mathf.Clamp01(t / visualDur);

            /* música */
            if (bgmSource)
                bgmSource.volume = Mathf.Lerp(1f,0f, Mathf.Clamp01(t / audioDur));

            yield return null;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}