using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeInUI : MonoBehaviour
{
    [Tooltip("Tempo em segundos para o fade-in completar")]
    [SerializeField] private float fadeDuration = 1.5f;

    private Image img;
    private float timer = 0f;

    private void Awake()
    {
        img = GetComponent<Image>();
        Color c = img.color;
        c.a = 1f;          // começa totalmente opaco
        img.color = c;
    }

    private void Start()
    {
        // Opcional: se usar escalas de tempo alteradas em pausa, troque por unscaledDeltaTime
        StartCoroutine(FadeRoutine());
    }

    private System.Collections.IEnumerator FadeRoutine()
    {
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);

            Color c = img.color;
            c.a = alpha;
            img.color = c;

            yield return null;
        }

        // Certifica-se de ficar totalmente transparente e desabilita para não gastar draw-call
        Color done = img.color;
        done.a = 0f;
        img.color = done;
        img.enabled = false;
    }
}