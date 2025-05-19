using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoreIntroController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI loreText;
    public GameObject continueButton;
    public CanvasGroup canvasGroup;      // no próprio LoreCanvas

    [Header("Typewriter Settings")]
    [Tooltip("Segundos entre cada caractere (ex: 0.025 = 40 cps)")]
    public float typeSpeed = 0.025f;
    public AudioSource typingAudioSource;
    public AudioClip typingClip;

    [Header("Lore Content")]
    public List<string> paragraphs = new List<string>();

    [Header("Scene Settings")]
    public string nextSceneName = "Cena1";
    public float fadeDuration = 1f;

    private int currentParagraph = 0;
    private bool isTyping = false;

    private void Awake()
    {
        // Garante que, se alguém ativar o Canvas sem chamar StartLore, esteja limpo:
        loreText.text = "";
        continueButton.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Começa a tela de lore — chame logo após ativar o GameObject do Canvas.
    /// </summary>
    public void StartLore(List<string> textos)
    {
        if (textos == null || textos.Count == 0)
        {
            Debug.LogWarning("LoreIntroController: lista de parágrafos vazia.");
            return;
        }

        paragraphs = textos;
        currentParagraph = 0;
        StartCoroutine(FadeInAndPlay());
    }

    private IEnumerator FadeInAndPlay()
    {
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        yield return StartCoroutine(ShowNextParagraph());
    }

    private IEnumerator ShowNextParagraph()
    {
        isTyping = true;
        loreText.text = "";
        continueButton.SetActive(false);

        string fullText = paragraphs[currentParagraph];
        for (int i = 0; i < fullText.Length; i++)
        {
            loreText.text += fullText[i];
            if (typingAudioSource != null && typingClip != null)
                typingAudioSource.PlayOneShot(typingClip);

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        continueButton.SetActive(true);
    }

    /// <summary>
    /// Ligado ao botão “Continuar →”
    /// </summary>
    public void OnContinueButton()
    {
        if (isTyping) return;

        continueButton.SetActive(false);
        currentParagraph++;

        if (currentParagraph < paragraphs.Count)
        {
            StartCoroutine(ShowNextParagraph());
        }
        else
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}