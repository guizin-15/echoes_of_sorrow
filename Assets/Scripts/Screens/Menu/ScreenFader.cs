using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;   // apenas para deixar claro que usa CanvasGroup

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    /* ----------------------------------------------------------------------- */
    /*  INSPECTOR                                                              */
    /* ----------------------------------------------------------------------- */

    [Header("Configurações do fade")]
    [Tooltip("Duração (segundos) do fade-in e do fade-out")]
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Se verdadeiro, inicia a cena com tela preta e faz fade-in")]
    [SerializeField] private bool fadeInOnStart = true;

    [Tooltip("Alpha inicial quando o objeto é habilitado")]
    [SerializeField] private bool startBlack = true;

    [Tooltip("Duração (segundos) para o fade da música.\n0 = usa o mesmo fadeDuration visual")]
    [SerializeField] private float bgmFadeDuration = 0f;

    /* ----------------------------------------------------------------------- */
    /*  PROPRIEDADES PÚBLICAS                                                  */
    /* ----------------------------------------------------------------------- */

    /// <summary>Tempo (segundos) usado por todos os fades.</summary>
    public float FadeDuration => fadeDuration;

    /// <summary>Duração (s) do fade aplicado à música.\nSe for 0, usa FadeDuration.</summary>
    public float BgmFadeDuration => bgmFadeDuration > 0f ? bgmFadeDuration : fadeDuration;

    /* ----------------------------------------------------------------------- */
    /*  CAMPOS PRIVADOS                                                        */
    /* ----------------------------------------------------------------------- */

    private CanvasGroup cg;
    private Coroutine   routine;

    /* ----------------------------------------------------------------------- */
    /*  CICLO DE VIDA                                                          */
    /* ----------------------------------------------------------------------- */

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = startBlack ? 1f : 0f;
    }

    private void Start()
    {
        if (fadeInOnStart)
            FadeIn();
    }

    /* ----------------------------------------------------------------------- */
    /*  API PÚBLICA                                                            */
    /* ----------------------------------------------------------------------- */

    /// <summary>Fade da tela de preto (alpha 1) para transparente (alpha 0).</summary>
    public void FadeIn(Action onComplete = null)
    {
        StartFade(1f, 0f, onComplete);
    }

    /// <summary>Fade da tela de transparente (alpha 0) para preto (alpha 1).</summary>
    public void FadeOut(Action onComplete = null)
    {
        StartFade(0f, 1f, onComplete);
    }

    /* ----------------------------------------------------------------------- */
    /*  IMPLEMENTAÇÃO INTERNA                                                  */
    /* ----------------------------------------------------------------------- */

    private void StartFade(float from, float to, Action onComplete)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine(from, to, onComplete));
    }

    private IEnumerator FadeRoutine(float from, float to, Action onComplete)
    {
        cg.alpha = from;
        float t  = 0f;

        while (t < fadeDuration)
        {
            t        += Time.deltaTime;
            cg.alpha  = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        cg.alpha = to;          // garante valor final exato
        routine  = null;        // libera referência

        onComplete?.Invoke();
    }
}